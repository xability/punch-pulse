using UnityEngine;
using UnityEngine.UIElements;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.Device)]
    [Tooltip("Sends an event when a VisualElement is touched.")]
    public class TouchVisualElementEvent : FsmStateAction
    {
        [RequiredField]
        [Tooltip("The GameObject with the VisualElement attached.")]
        public FsmOwnerDefault gameObject;

        [Tooltip("Event to send when the VisualElement is touched.")]
        public FsmEvent sendEvent;

        private VisualElement visualElement;

        public override void Reset()
        {
            gameObject = null;
            sendEvent = null;
        }

        public override void OnEnter()
        {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (go == null)
            {
                Finish();
                return;
            }

            visualElement = go.GetComponent<UIDocument>().rootVisualElement;
            if (visualElement == null)
            {
                LogError("Missing VisualElement component!");
                Finish();
                return;
            }

            visualElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        public override void OnExit()
        {
            if (visualElement != null)
            {
                visualElement.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            }
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            Fsm.Event(sendEvent);
        }
    }
}
