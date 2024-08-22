// (c) Copyright HutongGames, LLC 2010-2023. All rights reserved.
using UnityEngine;
using UnityEngine.Video;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.Movie)]
    [Tooltip("Stops playing the Video Player, and rewinds it to the beginning.")]
    public class StopVideoPlayer : FsmStateAction
    {
        [RequiredField]
        [CheckForComponent(typeof(VideoPlayer))]
        [Tooltip("The GameObject with a VideoPlayer component.")]
        public FsmOwnerDefault gameObject;

        public override void Reset()
        {
            gameObject = null;
        }

        public override void OnEnter()
        {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (go == null)
            {
                Finish();
                return;
            }

            var videoPlayer = go.GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                videoPlayer.time = 0;  // Rewind to the beginning
            }
            else
            {
                LogError("VideoPlayer component not found on " + go.name);
            }

            Finish();
        }
    }
}