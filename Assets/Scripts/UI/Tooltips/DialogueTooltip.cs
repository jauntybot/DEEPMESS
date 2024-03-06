using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueTooltip : Tooltip {
    
    public bool skip = true;
    [SerializeField] DialogueTypewriter tw;
    [SerializeField] Button clickToSkip;
    [SerializeField] Animator clickToSkipAnim;
    [SerializeField] SFX nailSpeak;
    
    Animator anim;
    AudioSource audioSource;

    void Start() {
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        tw = contentField.GetComponent<DialogueTypewriter>();

        RectTransform anchor = transform.GetChild(0).GetComponent<RectTransform>();
        anchor.anchorMin = new Vector2(0.5f, 0.5f);
        anchor.anchorMax = new Vector2(0.5f, 0.5f);
        anchor.anchoredPosition = Vector2.zero;
    }

    public override void SetText(string content, string header = "", bool skip = false,  List<RuntimeAnimatorController> gif = null) {
        base.SetText(content, header, clickToSkip, gif);
        if (tw)
            tw.StartCoroutine(tw.Typerwrite(content));

        if (anim) {
            anim.SetTrigger("AnimIn");
        }
        PlaySound(nailSpeak);
        if (skip)     
            StartCoroutine(WaitForClick());

        
    }

    public IEnumerator WaitForClick() {
        skip = false;
        clickToSkipAnim.gameObject.SetActive(true);
        clickToSkip.enabled = true;
        clickToSkipAnim.SetBool("Blink", false);
        bool skippable = false;
        while (!skip) {
            if (!skippable) {
                if (tw && !tw.writing) { skippable = true; clickToSkipAnim.SetBool("Blink", true); }
                else if (!tw) { skippable = true; clickToSkipAnim.SetBool("Blink", true); }
            }
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
        clickToSkipAnim.gameObject.SetActive(false);
        clickToSkip.enabled = false;
    }

    public void ClickToSkip() {
        if (tw && tw.writing) {
            tw.SkipWriting();
        } else
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
