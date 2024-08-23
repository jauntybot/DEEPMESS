using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueTooltip : Tooltip {
    
    public bool skip = true;
    public DialogueTypewriter tw;
    [SerializeField] Button clickToSkip;
    [SerializeField] Animator clickToSkipAnim;
    [SerializeField] SFX nailSpeak, ginoSpeak;
    [SerializeField] RuntimeAnimatorController nailAnim, ginoAnim;
    Animator anim;
    AudioSource audioSource;
    [SerializeField] Animator portraitAnim;

    void Awake() {
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        tw = contentField.GetComponent<DialogueTypewriter>();

        RectTransform anchor = transform.GetChild(0).GetComponent<RectTransform>();
        anchor.anchorMin = new Vector2(0.5f, 0.5f);
        anchor.anchorMax = new Vector2(0.5f, 0.5f);
        anchor.anchoredPosition = Vector2.zero;
    }

    public virtual void SetText(string content, string header = "", bool _skip = false, bool gino = false, List<RuntimeAnimatorController> gif = null) {
        base.SetText(content, header, gif);
        if (tw)
            tw.StartCoroutine(tw.Typerwrite(content));

        if (anim) {
            anim.SetTrigger("AnimIn");
        }
        
        PlaySound(gino? ginoSpeak : nailSpeak);
        portraitAnim.runtimeAnimatorController = gino ? ginoAnim : nailAnim;

        portraitAnim.GetComponent<RectTransform>().anchoredPosition = gino? new(77, -95) : new(40, -120);
        portraitAnim.GetComponent<RectTransform>().sizeDelta = gino? new(200, 200) : new(235, 235);

        if (_skip)     
            StartCoroutine(WaitForClick());
        else
            clickToSkipAnim.gameObject.SetActive(false);
        
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
