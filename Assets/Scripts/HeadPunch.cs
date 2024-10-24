using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadPunch : PlayAudioOnBoxing
{
    // OnTriggerEnter
    protected override void OnTriggerEnter(Collider other)
    {
        if (!hasPlayed && other.CompareTag(targetTag))
        {
            // Check if either controller is in front of the player
            bool isLeftControllerInFront = base.IsControllerInFront(leftControllerTransform);
            bool isRightControllerInFront = base.IsControllerInFront(rightControllerTransform);
            // Debug.Log("Left controller extended : " + isLeftControllerInFront + "\n Right controller extended : " + isRightControllerInFront + "\n Cheering sound : " + isCheering);

            if (isLeftControllerInFront || isRightControllerInFront)
            {
                
                Debug.Log("Head hit! : " + other.gameObject.tag);
                base.hasPlayed = true;
                base.PlaySound(other); // --> adding punching sound
                base.TriggerHeadHitAnimation();
                ScoreManager.AddScore(2);
                // PlayCheerSound(); -- commenting out cheering sound

            }
        }
    }
}
