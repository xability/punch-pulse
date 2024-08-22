using UnityEngine;
using UnityEngine.UI;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.GUIElement)]
    [Tooltip("Sets the Text of a Text component.")]
    public class SetText : ComponentAction<Text>
    {
        [RequiredField]
        [CheckForComponent(typeof(Text))]
        [Tooltip("The GameObject with the Text component.")]
        public FsmOwnerDefault gameObject;

        [UIHint(UIHint.TextArea)]
        [Tooltip("The text to set.")]
        public FsmString text;

        [Tooltip("Reset when exiting this state.")]
        public FsmBool resetOnExit;

        private Text textComponent;
        private string originalText;

        public override void Reset()
        {
            gameObject = null;
            text = "";
            resetOnExit = null;
        }

        public override void OnEnter()
        {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (UpdateCache(go))
            {
                textComponent = cachedComponent;
                originalText = textComponent.text;
            }

            DoSetText();

            Finish();
        }

        void DoSetText()
        {
            if (textComponent != null)
            {
                textComponent.text = text.Value;
            }
        }

        public override void OnExit()
        {
            if (textComponent == null) return;

            if (resetOnExit.Value)
            {
                textComponent.text = originalText;
            }
        }
    }
}