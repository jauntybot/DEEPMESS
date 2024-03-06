using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net.Mime;

public class DialogueTypewriter : MonoBehaviour {

    [SerializeField] float typewriteSpeed;
    public bool writing;
    TMP_Text tmp;
    int visibleChar;
    int charIndex = 0;

    void Awake() {
        tmp = GetComponent<TMP_Text>();
    }

    public IEnumerator Typerwrite(string content) {
        writing = true;
        tmp.ForceMeshUpdate();
        tmp.text = content;
        visibleChar = tmp.textInfo.characterCount;

        charIndex = 0;
        while (writing) {

            tmp.maxVisibleCharacters = charIndex;
            charIndex++;
// Delay by typewriteSpeed
            float t = 0; while (t < typewriteSpeed) { yield return null; t+=Time.deltaTime; }
            if (charIndex >= visibleChar) writing = false;
        }
        SkipWriting();

    }

    public void SkipWriting() {
        writing = false;
        charIndex = visibleChar;
        tmp.maxVisibleCharacters = visibleChar;
    }

}
