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
        RectTransform anchor = transform.GetChild(0).GetComponent<RectTransform>();
        anchor.anchorMin = new Vector2(0.5f, 0.5f);
        anchor.anchorMax = new Vector2(0.5f, 0.5f);
        anchor.anchoredPosition = Vector2.zero;
    }

    public override void SetText(string content, string header = "", bool skip = false,  List<RuntimeAnimatorController> gif = null) {
        base.SetText(content, header, clickToSkip, gif);
        
        PlaySound(nailSpeak);
        if (skip)     
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
