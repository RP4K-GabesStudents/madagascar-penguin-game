using System.Collections.Generic;
using GLTFast.Schema;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cutscene.TimeLine
{
    public class BlackBarsMixerBehaviour : PlayableBehaviour
    {
        public PlayableDirector Director;
        private float _defaultHeight;
        private bool _hasStarted;
        private List<CanvasGroup> _trackedGroups = new ();
        private RectTransform _rootRef;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            RectTransform root = playerData as RectTransform;
    
            // Check 1 — is the bound object arriving correctly
            if (root == null)
            {
                Debug.LogError("BlackBarsMixer: playerData is null — RectTransform not bound correctly");
                return;
            }

            _rootRef = root;
            CaptureDefaultHeight(root);

            // Check 2 — is the graph calculating any blend weight at all
            float blendedHeight = CalculateBlend(playable);
            Debug.Log($"BlackBarsMixer: blendedHeight = {blendedHeight}");

            ApplyBarHeight(root, blendedHeight);
        }

        private void ApplyBarHeight(RectTransform root, float height)
        {
            RectTransform topBar    = root.Find("TopBar")    as RectTransform;
            RectTransform bottomBar = root.Find("BottomBar") as RectTransform;

            // Check 3 — are the children being found by name
            if (topBar == null)    Debug.LogError("BlackBarsMixer: could not find child named TopBar");
            if (bottomBar == null) Debug.LogError("BlackBarsMixer: could not find child named BottomBar");

            if (topBar != null)
            {
                Vector2 size = topBar.sizeDelta;
                size.y = height;
                topBar.sizeDelta = size;
                Debug.Log($"BlackBarsMixer: setting TopBar height to {height}");
            }

            if (bottomBar != null)
            {
                Vector2 size = bottomBar.sizeDelta;
                size.y = height;
                bottomBar.sizeDelta = size;
            }
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            _hasStarted = false;
            RestoreCanvases();

            // Also reset bar heights on stop
            if (_rootRef == null) return;
            RectTransform topBar    = _rootRef.Find("TopBar")    as RectTransform;
            RectTransform bottomBar = _rootRef.Find("BottomBar") as RectTransform;

            if (topBar    != null) { Vector2 s = topBar.sizeDelta;    s.y = 0; topBar.sizeDelta    = s; }
            if (bottomBar != null) { Vector2 s = bottomBar.sizeDelta; s.y = 0; bottomBar.sizeDelta = s; }
        }

        private void CaptureDefaultHeight(RectTransform root)
        {
            if (_hasStarted) return;
            _hasStarted = true; // nothing to capture anymore
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

                Debug.Log($"BlackBarsMixer: barHeight on input {i} = {behaviour.BarHeight}");

                blendedHeight += behaviour.BarHeight * weight;

                var clip = GetClipAsset(playable, i);
                if (clip == null)
                {
                    Debug.LogWarning($"BlackBarsMixer: could not get clip asset for input {i}");
                    continue;
                }

                ProcessCanvases(clip, input, behaviour);
            }

            return blendedHeight;
        }

        private void ProcessCanvases(BlackBarsClip clip, ScriptPlayable<BlackBarsBehaviour> input, BlackBarsBehaviour behaviour)
        {
            List<CanvasGroup> groups = clip.ResolveCanvases(Director);

            foreach (CanvasGroup group in groups)
            {
                _trackedGroups.Add(group);

                double timeLeft = input.GetDuration() - input.GetTime();
                
                group.alpha = timeLeft <= behaviour.FadeOutTime && behaviour.FadeOutTime > 0f ? Mathf.Lerp(0f, 1f, (float)(1f - (timeLeft / behaviour.FadeOutTime))) : 0f;
                group.interactable = false;
                group.blocksRaycasts = false;
            }
        }
        
        private void RestoreCanvases()
        {
            foreach (var group in _trackedGroups)
            {
                if (group == null) continue;
                group.alpha = 1f;
                group.interactable = true;
                group.blocksRaycasts = true;
            }
        }
        
        private BlackBarsClip GetClipAsset(Playable mixer, int inputIndex)
        {
            if (Director == null) return null;

            var playableAsset = Director.playableAsset as TimelineAsset;
            if (playableAsset == null) return null;

            int clipIndex = 0;
            foreach (var track in playableAsset.GetOutputTracks())
            {
                if (!(track is BlackBarsTrack)) continue;
                foreach (var clip in track.GetClips())
                {
                    if (clipIndex == inputIndex) return clip.asset as BlackBarsClip;
                    clipIndex++;
                }
            }
            return null;
        }
    }
}
