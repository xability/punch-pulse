using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioOnBoundaryCollision : MonoBehaviour
{
    public AudioClip clip;
    private AudioSource source;
    public string targetTag;


    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();

        
    }

    // OnTriggerLeave
    void OnTriggerLeave(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            source.PlayOneShot(clip);
        }
        
    }
}
