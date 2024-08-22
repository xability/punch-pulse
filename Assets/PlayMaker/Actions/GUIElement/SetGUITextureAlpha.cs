// (c) Copyright HutongGames, LLC 2010-2023. All rights reserved.
using UnityEngine;
using UnityEngine.UI;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.UI)]
    [Tooltip("Sets the Alpha of the Image component attached to a Game Object. Useful for fading UI elements in/out.")]
    public class SetUIImageAlpha : ComponentAction<Image>
    {
        [RequiredField]
        [CheckForComponent(typeof(Image))]
        public FsmOwnerDefault gameObject;

        [RequiredField]
        public FsmFloat alpha;

        public bool everyFrame;

        public override void Reset()
        {
            gameObject = null;
            alpha = 1.0f;
            everyFrame = false;
        }

        public override void OnEnter()
        {
            DoImageAlpha();

            if (!everyFrame)
            {
                Finish();
            }
        }

        public override void OnUpdate()
        {
            DoImageAlpha();
        }

        void DoImageAlpha()
        {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (UpdateCache(go))
            {
                var color = cachedComponent.color;
                cachedComponent.color = new Color(color.r, color.g, color.b, alpha.Value);
            }
        }
    }
}