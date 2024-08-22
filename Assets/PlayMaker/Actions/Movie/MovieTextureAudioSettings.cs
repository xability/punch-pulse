using UnityEngine;
using UnityEngine.Video;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.Movie)]
    [Tooltip("Sets the Audio Settings of a VideoPlayer component.")]
    public class VideoPlayerAudioSettings : ComponentAction<VideoPlayer>
    {
        [RequiredField]
        [CheckForComponent(typeof(VideoPlayer))]
        [Tooltip("The GameObject with the VideoPlayer component.")]
        public FsmOwnerDefault gameObject;

        [Tooltip("The audio volume")]
        public FsmFloat volume;

        [Tooltip("Mute/unmute the audio")]
        public FsmBool mute;

        [Tooltip("Set the pitch of the audio")]
        public FsmFloat pitch;

        public override void Reset()
        {
            gameObject = null;
            volume = null;
            mute = null;
            pitch = null;
        }

        public override void OnEnter()
        {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (UpdateCache(go))
            {
                DoSetAudioSettings();
            }

            Finish();
        }

        void DoSetAudioSettings()
        {
            if (!volume.IsNone)
            {
                cachedComponent.SetDirectAudioVolume(0, volume.Value);
            }

            if (!mute.IsNone)
            {
                cachedComponent.SetDirectAudioMute(0, mute.Value);
            }

            // Note: VideoPlayer doesn't have a direct pitch setting.
            // You might need to use an AudioSource for pitch control.
            if (!pitch.IsNone)
            {
                Debug.LogWarning("Pitch control is not directly available for VideoPlayer. Consider using an AudioSource for pitch control.");
            }
        }
    }
}