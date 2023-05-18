using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueTooltip : Tooltip
{
    
    public bool skip = true;
    [SerializeField] Button clickToSkip;
    [SerializeField] SFX nailSpeak;
    [SerializeField] GameObject clickToAdvanceText;
    AudioSource audioSource;

    void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    public override void SetText(Vector2 pos, string content, string header = "", bool clickToSkip = false) {
        base.SetText(pos, content, header, clickToSkip);
        PlaySound(nailSpeak);
        if (clickToSkip)     
            StartCoroutine(WaitForClick());
        
    }

    public IEnumerator WaitForClick() {
        skip = false;
        clickToAdvanceText.SetActive(true);
        clickToSkip.enabled = true;
        while (!skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }
        clickToAdvanceText.SetActive(false);
        clickToSkip.enabled = false;
    }

    public void ClickToSkip() {
        skip = true;
    }

    public virtual void PlaySound(SFX sfx = null) {
        if (sfx) {
            if (sfx.outputMixerGroup) 
                audioSource.outputAudioMixerGroup = sfx.outputMixerGroup;   

            audioSource.PlayOneShot(sfx.Get());
        }
    }

}
