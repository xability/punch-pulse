// (c) Copyright HutongGames, LLC 2010-2023. All rights reserved.
using UnityEngine;
using UnityEngine.UI;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.UI)]
    [Tooltip("Sets the Color of the Image component attached to a Game Object.")]
    public class SetUIImageColor : ComponentAction<Image>
    {
        [RequiredField]
        [CheckForComponent(typeof(Image))]
        public FsmOwnerDefault gameObject;

        [RequiredField]
        public FsmColor color;

        public bool everyFrame;

        public override void Reset()
        {
            gameObject = null;
            color = Color.white;
            everyFrame = false;
        }

        public override void OnEnter()
        {
            DoSetImageColor();

            if (!everyFrame)
            {
                Finish();
            }
        }

        public override void OnUpdate()
        {
            DoSetImageColor();
        }

        void DoSetImageColor()
        {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (UpdateCache(go))
            {
                cachedComponent.color = color.Value;
            }
        }
    }
}