#if !(UNITY_SWITCH || UNITY_TVOS || UNITY_IPHONE || UNITY_IOS  || UNITY_ANDROID || UNITY_FLASH || UNITY_PS3 || UNITY_PS4 || UNITY_XBOXONE || UNITY_BLACKBERRY || UNITY_METRO || UNITY_WP8 || UNITY_PSM || UNITY_WEBGL || UNITY_SWITCH)

using UnityEngine;
using UnityEngine.Video;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.Movie)]
    [Tooltip("Plays a Video Player. Use the Video Player in a Material, or in the GUI.")]
    public class PlayVideoPlayer : FsmStateAction
    {
        [RequiredField]
        [ObjectType(typeof(VideoPlayer))]
        public FsmObject videoPlayer;

        public FsmBool loop;

        public override void Reset()
        {
            videoPlayer = null;
            loop = false;
        }

        public override void OnEnter()
        {
            var player = videoPlayer.Value as VideoPlayer;

            if (player != null)
            {
                player.isLooping = loop.Value;
                player.Play();
            }

            Finish();
        }
    }
}

#endif
