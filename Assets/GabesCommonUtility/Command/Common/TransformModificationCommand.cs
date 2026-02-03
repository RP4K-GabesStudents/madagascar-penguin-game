using System;
using UnityEngine;
using Commands.Core;

#if USE_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
#else 
using System.Collections;
#endif

namespace Command.Common
{
    public class TransformModificationCommand : ICommand
    {
        private readonly Transform _target;
        private Vector3 _newPosition;
        private Quaternion _newRotation;
        private Vector3 _newScale;
        private readonly ERules _rules;
        private readonly bool _animate;
        private readonly float _animationDuration;

        private Vector3 _oldPosition;
        private Quaternion _oldRotation;
        private Vector3 _oldScale;

        [Flags]
        public enum ERules
        {
            Position = 1 << 0,
            Rotation = 1 << 1,
            Scale = 1 << 2,
            PositionAndRotation = Position | Rotation,
            All = Position | Rotation | Scale
        }

        public TransformModificationCommand(Transform target, ERules rules = ERules.All, bool animate = true, float animationDuration = 0.3f)
        {
            _target = target;
            _rules = rules;
            _animate = animate;
            _animationDuration = animationDuration;
        }

        public void SetNewInfo(Transform t) { SetNewInfo(t.position, t.rotation, t.localScale); }

        public void SetNewInfo(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            _newPosition = pos;
            _newRotation = rot;
            _newScale = scale;
        }

        // --- Execute Logic ---
#if USE_UNITASK
        public string DisplayName => "TransformModificationCommand";

        public async UniTask ExecuteAsync(CancellationToken ct = default)
        {
            CaptureBeforeState();
            
            if (_animate)
                await AnimateToState(_newPosition, _newRotation, _newScale, ct);
            else
                ApplyState(_newPosition, _newRotation, _newScale);
        }

        public async UniTask UndoAsync(CancellationToken ct = default)
        {
            if (_animate)
                await AnimateToState(_oldPosition, _oldRotation, _oldScale, ct);
            else
                ApplyState(_oldPosition, _oldRotation, _oldScale);
        }

        private async UniTask AnimateToState(Vector3 targetPos, Quaternion targetRot, Vector3 targetScale, CancellationToken ct)
        {
            if (!_target) return;

            float elapsed = 0f;

            Vector3 startPos = _target.position;
            Quaternion startRot = _target.rotation;
            Vector3 startScale = _target.localScale;

            // Check if animation is needed
            bool needsAnimation = false;
            if (_rules.HasFlag(ERules.Position) && Vector3.Distance(startPos, targetPos) > 0.001f)
                needsAnimation = true;
            if (_rules.HasFlag(ERules.Rotation) && Quaternion.Angle(startRot, targetRot) > 0.1f)
                needsAnimation = true;
            if (_rules.HasFlag(ERules.Scale) && Vector3.Distance(startScale, targetScale) > 0.001f)
                needsAnimation = true;

            if (!needsAnimation)
            {
                ApplyState(targetPos, targetRot, targetScale);
                return;
            }

            while (elapsed < _animationDuration)
            {
                if (ct.IsCancellationRequested)
                    break;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _animationDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                Vector3 currentPos = _rules.HasFlag(ERules.Position) ? Vector3.Lerp(startPos, targetPos, smoothT) : startPos;
                Quaternion currentRot = _rules.HasFlag(ERules.Rotation) ? Quaternion.Slerp(startRot, targetRot, smoothT) : startRot;
                Vector3 currentScale = _rules.HasFlag(ERules.Scale) ? Vector3.Lerp(startScale, targetScale, smoothT) : startScale;

                ApplyState(currentPos, currentRot, currentScale);

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            // Ensure final state is exact
            ApplyState(targetPos, targetRot, targetScale);
        }
#else
        public IEnumerator Execute()
        {
            CaptureBeforeState();
            
            if (_animate)
            {
                yield return AnimateToState(_newPosition, _newRotation, _newScale);
            }
            else
            {
                ApplyState(_newPosition, _newRotation, _newScale);
            }
        }

        public IEnumerator Undo()
        {
            if (_animate)
            {
                yield return AnimateToState(_oldPosition, _oldRotation, _oldScale);
            }
            else
            {
                ApplyState(_oldPosition, _oldRotation, _oldScale);
            }
        }

        private IEnumerator AnimateToState(Vector3 targetPos, Quaternion targetRot, Vector3 targetScale)
        {
            if (!_target) yield break;

            float elapsed = 0f;

            Vector3 startPos = _target.position;
            Quaternion startRot = _target.rotation;
            Vector3 startScale = _target.localScale;

            // Check if animation is needed
            bool needsAnimation = false;
            if (_rules.HasFlag(ERules.Position) && Vector3.Distance(startPos, targetPos) > 0.001f)
                needsAnimation = true;
            if (_rules.HasFlag(ERules.Rotation) && Quaternion.Angle(startRot, targetRot) > 0.1f)
                needsAnimation = true;
            if (_rules.HasFlag(ERules.Scale) && Vector3.Distance(startScale, targetScale) > 0.001f)
                needsAnimation = true;

            if (!needsAnimation)
            {
                ApplyState(targetPos, targetRot, targetScale);
                yield break;
            }

            while (elapsed < _animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _animationDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                Vector3 currentPos = _rules.HasFlag(ERules.Position) ? Vector3.Lerp(startPos, targetPos, smoothT) : startPos;
                Quaternion currentRot = _rules.HasFlag(ERules.Rotation) ? Quaternion.Slerp(startRot, targetRot, smoothT) : startRot;
                Vector3 currentScale = _rules.HasFlag(ERules.Scale) ? Vector3.Lerp(startScale, targetScale, smoothT) : startScale;

                ApplyState(currentPos, currentRot, currentScale);

                yield return null;
            }

            // Ensure final state is exact
            ApplyState(targetPos, targetRot, targetScale);
        }
#endif

        // --- Helper Methods ---
        private void CaptureBeforeState()
        {
            if (_rules.HasFlag(ERules.Position)) _oldPosition = _target.position;
            if (_rules.HasFlag(ERules.Rotation)) _oldRotation = _target.rotation;
            if (_rules.HasFlag(ERules.Scale)) _oldScale = _target.localScale;
        }

        private void ApplyState(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (!_target) return;
            if (_rules.HasFlag(ERules.Position))
            {
                if (_rules.HasFlag(ERules.Rotation))
                    _target.SetPositionAndRotation(pos, rot);
                else _target.position = pos;
            }
            else if (_rules.HasFlag(ERules.Rotation)) _target.rotation = rot;
            if (_rules.HasFlag(ERules.Scale)) _target.localScale = scale;
        }
    }
}