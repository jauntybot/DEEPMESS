using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SlimeArmVFX : MonoBehaviour
{

    AudioSource audioSource;
    [SerializeField] SFX unfurlSFX, loopSFX;

    void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    public void UnfurlSFX() {
        PlaySound(unfurlSFX);
    }

    public void LoopSFX() {
        PlaySound(loopSFX, true);
    }

    public virtual void PlaySound(SFX sfx = null, bool loop = false) {
        if (sfx) {
            audioSource.loop = loop;
            if (sfx.outputMixerGroup) 
                audioSource.outputAudioMixerGroup = sfx.outputMixerGroup;   
            if (!loop)
                audioSource.PlayOneShot(sfx.Get());
            else {
                audioSource.clip = loopSFX.Get();
                audioSource.Play();
            }
        }
    }    

}
