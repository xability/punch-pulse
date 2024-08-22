#if !(UNITY_SWITCH || UNITY_TVOS || UNITY_IPHONE || UNITY_IOS || UNITY_ANDROID || UNITY_FLASH || UNITY_PS3 || UNITY_PS4 || UNITY_XBOXONE || UNITY_BLACKBERRY || UNITY_WP8 || UNITY_PSM || UNITY_WEBGL || UNITY_SWITCH)

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using System.Collections;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("WWW")]
    [Tooltip("Gets data from a url and store it in variables. See UnityWebRequest docs for more details.")]
    public class WWWObject : FsmStateAction
    {
        [RequiredField]
        [Tooltip("Url to download data from.")]
        public FsmString url;

        [ActionSection("Results")]

        [UIHint(UIHint.Variable)]
        [Tooltip("Gets text from the url.")]
        public FsmString storeText;

        [UIHint(UIHint.Variable)]
        [Tooltip("Gets a Texture from the url.")]
        public FsmTexture storeTexture;

        [UIHint(UIHint.Variable)]
        [ObjectType(typeof(VideoPlayer))]
        [Tooltip("Gets a Texture from the url.")]
        public FsmObject storeVideoPlayer;

        [UIHint(UIHint.Variable)]
        [Tooltip("Error message if there was an error during the download.")]
        public FsmString errorString;

        [UIHint(UIHint.Variable)]
        [Tooltip("How far the download progressed (0-1).")]
        public FsmFloat progress;

        [ActionSection("Events")]

        [Tooltip("Event to send when the data has finished loading (progress = 1).")]
        public FsmEvent isDone;

        [Tooltip("Event to send if there was an error.")]
        public FsmEvent isError;

        private UnityWebRequest webRequest;

        public override void Reset()
        {
            url = null;
            storeText = null;
            storeTexture = null;
            errorString = null;
            progress = null;
            isDone = null;
        }

        public override void OnEnter()
        {
            if (string.IsNullOrEmpty(url.Value))
            {
                Finish();
                return;
            }

            StartCoroutine(GetRequest(url.Value));
        }

        private IEnumerator GetRequest(string uri)
        {
            webRequest = UnityWebRequest.Get(uri);
            webRequest.SendWebRequest();

            while (!webRequest.isDone)
            {
                progress.Value = webRequest.downloadProgress;
                yield return null;
            }

            errorString.Value = webRequest.error;

            if (!string.IsNullOrEmpty(webRequest.error))
            {
                Fsm.Event(isError);
            }
            else
            {
                storeText.Value = webRequest.downloadHandler.text;

                var textureRequest = UnityWebRequestTexture.GetTexture(uri);
                yield return textureRequest.SendWebRequest();
                storeTexture.Value = DownloadHandlerTexture.GetContent(textureRequest);

                // Assigning VideoPlayer is more complex and context-dependent
                var videoPlayer = storeVideoPlayer.Value as VideoPlayer;
                if (videoPlayer != null)
                {
                    videoPlayer.url = uri;
                    videoPlayer.Play();
                }

                Fsm.Event(isDone);
            }

            Finish();
        }
    }
}

#endif