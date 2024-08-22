// (c) Copyright HutongGames, LLC 2010-2023. All rights reserved.
using UnityEngine;
using UnityEngine.UI;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.UI)]
    [Tooltip("Sets the Sprite used by the Image component attached to a Game Object.")]
    public class SetUIImage : ComponentAction<Image>
    {
        [RequiredField]
        [CheckForComponent(typeof(Image))]
        [Tooltip("The GameObject that owns the Image component.")]
        public FsmOwnerDefault gameObject;

        [Tooltip("Sprite to apply.")]
        public FsmObject sprite;

        public override void Reset()
        {
            gameObject = null;
            sprite = null;
        }

        public override void OnEnter()
        {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (UpdateCache(go))
            {
                if (sprite.Value is Sprite spriteValue)
                {
                    cachedComponent.sprite = spriteValue;
                }
                else
                {
                    LogError("Sprite value is not a valid Sprite object.");
                }
            }

            Finish();
        }
    }
}