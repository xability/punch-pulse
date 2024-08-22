using UnityEngine;
using UnityEngine.UI;
using System;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.GUI)]
    [Tooltip("Performs a Hit Test on a GameObject with an Image or Text component.")]
#if UNITY_2017_2_OR_NEWER
#pragma warning disable CS0618
    [Obsolete("GUIElement is part of the legacy UI system and will be removed in a future release")]
#endif
    public class GUIElementHitTest : FsmStateAction
    {
        [RequiredField]
        [CheckForComponent(typeof(RectTransform))]
        [Tooltip("The GameObject that has an Image or Text component.")]
        public FsmOwnerDefault gameObject;
        [Tooltip("Specify camera or use MainCamera as default.")]
        public Camera camera;
        [Tooltip("A vector position on screen. Usually stored by actions like GetTouchInfo, or World To Screen Point.")]
        public FsmVector3 screenPoint;
        [Tooltip("Specify screen X coordinate.")]
        public FsmFloat screenX;
        [Tooltip("Specify screen Y coordinate.")]
        public FsmFloat screenY;
        [Tooltip("Whether the specified screen coordinates are normalized (0-1).")]
        public FsmBool normalized;
        [Tooltip("Event to send if the Hit Test is true.")]
        public FsmEvent hitEvent;
        [UIHint(UIHint.Variable)]
        [Tooltip("Store the result of the Hit Test in a bool variable (true/false).")]
        public FsmBool storeResult;
        [Tooltip("Repeat every frame. Useful if you want to wait for the hit test to return true.")]
        public FsmBool everyFrame;

        // cache component
        private RectTransform rectTransform;
        private Graphic graphic;

        // remember game object cached, so we can re-cache component if it changes
        private GameObject gameObjectCached;

        public override void Reset()
        {
            gameObject = null;
            camera = null;
            screenPoint = new FsmVector3 { UseVariable = true };
            screenX = new FsmFloat { UseVariable = true };
            screenY = new FsmFloat { UseVariable = true };
            normalized = true;
            hitEvent = null;
            everyFrame = true;
        }

        public override void OnEnter()
        {
            DoHitTest();

            if (!everyFrame.Value)
            {
                Finish();
            }
        }

        public override void OnUpdate()
        {
            DoHitTest();
        }

        void DoHitTest()
        {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (go == null)
            {
                return;
            }

            // cache RectTransform and Graphic component

            if (go != gameObjectCached)
            {
                rectTransform = go.GetComponent<RectTransform>();
                graphic = go.GetComponent<Graphic>();
                gameObjectCached = go;
            }

            if (rectTransform == null || graphic == null)
            {
                Finish();
                return;
            }

            // get screen point to test

            var testPoint = screenPoint.IsNone ? new Vector3(0, 0) : screenPoint.Value;

            if (!screenX.IsNone)
            {
                testPoint.x = screenX.Value;
            }

            if (!screenY.IsNone)
            {
                testPoint.y = screenY.Value;
            }

            if (normalized.Value)
            {
                testPoint.x *= Screen.width;
                testPoint.y *= Screen.height;
            }

            // perform hit test

            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, testPoint, camera))
            {
                storeResult.Value = true;
                Fsm.Event(hitEvent);
            }
            else
            {
                storeResult.Value = false;
            }
        }
    }
}
