using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cutscene.TimeLine
{
    public class BlackBarsMixerBehaviour : PlayableBehaviour
    {
        public PlayableDirector Director;
        private bool _hasStarted;
        private List<CanvasGroup> _trackedGroups = new List<CanvasGroup>();
        private RectTransform _rootRef;
        private BlackBarsCanvasBinding _binding;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            RectTransform root = playerData as RectTransform;
            if (root == null) return;

            _rootRef = root;

            // Find the binding component once
            if (_binding == null)
                _binding = Object.FindObjectOfType<BlackBarsCanvasBinding>();

            _hasStarted = true;

            float blendedHeight = CalculateBlend(playable);
            ApplyBarHeight(root, blendedHeight);
        }

        private void ApplyBarHeight(RectTransform root, float height)
        {
            RectTransform topBar    = root.Find("TopBar")    as RectTransform;
            RectTransform bottomBar = root.Find("BottomBar") as RectTransform;

            if (topBar == null)    { Debug.LogError("TopBar not found"); return; }
            if (bottomBar == null) { Debug.LogError("BottomBar not found"); return; }

            topBar.offsetMax = Vector2.zero;
            topBar.offsetMin = new Vector2(0, -height);

            bottomBar.offsetMin = Vector2.zero;
            bottomBar.offsetMax = new Vector2(0, height);

            // Log the actual values AFTER setting them to see if they're sticking
            Debug.Log($"TopBar offsetMin={topBar.offsetMin} offsetMax={topBar.offsetMax} rect={topBar.rect}");
            Debug.Log($"BottomBar offsetMin={bottomBar.offsetMin} offsetMax={bottomBar.offsetMax} rect={bottomBar.rect}");
        }
        public override void OnPlayableDestroy(Playable playable)
        {
            _hasStarted = false;
            RestoreCanvases();

            if (_rootRef == null) return;
            RectTransform topBar    = _rootRef.Find("TopBar")    as RectTransform;
            RectTransform bottomBar = _rootRef.Find("BottomBar") as RectTransform;

            if (topBar != null)
            {
                topBar.offsetMax = Vector2.zero;
                topBar.offsetMin = Vector2.zero;
            }
            if (bottomBar != null)
            {
                bottomBar.offsetMin = Vector2.zero;
                bottomBar.offsetMax = Vector2.zero;
            }
        }

        private float CalculateBlend(Playable playable)
        {
            _trackedGroups.Clear();
            float blendedHeight = 0f;

            int inputCount = playable.GetInputCount();
            Debug.Log($"BlackBarsMixer: input count = {inputCount}");

            for (int i = 0; i < inputCount; i++)
            {
                float weight = playable.GetInputWeight(i);
                Debug.Log($"BlackBarsMixer: input {i} weight = {weight}");

                if (weight <= 0f) continue;

                var input     = (ScriptPlayable<BlackBarsBehaviour>)playable.GetInput(i);
                var behaviour = input.GetBehaviour();

                Debug.Log($"BlackBarsMixer: barHeight = {behaviour.BarHeight}");

                blendedHeight += behaviour.BarHeight * weight;

                ProcessCanvases(input, behaviour);
            }

            Debug.Log($"BlackBarsMixer: blendedHeight = {blendedHeight}");
            return blendedHeight;
        }

        private void ProcessCanvases(ScriptPlayable<BlackBarsBehaviour> input, BlackBarsBehaviour behaviour)
        {
            if (_binding == null || _binding.canvasesToHide == null) return;

            foreach (CanvasGroup group in _binding.canvasesToHide)
            {
                if (group == null) continue;
                _trackedGroups.Add(group);

                double timeLeft = input.GetDuration() - input.GetTime();

                group.alpha = timeLeft <= behaviour.FadeOutTime && behaviour.FadeOutTime > 0f
                    ? Mathf.Lerp(0f, 1f, (float)(1f - (timeLeft / behaviour.FadeOutTime)))
                    : 0f;

                group.interactable   = false;
                group.blocksRaycasts = false;
            }
        }

        private void RestoreCanvases()
        {
            foreach (var group in _trackedGroups)
            {
                if (group == null) continue;
                group.alpha          = 1f;
                group.interactable   = true;
                group.blocksRaycasts = true;
            }
        }
    }
}