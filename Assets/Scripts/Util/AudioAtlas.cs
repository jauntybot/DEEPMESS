using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAtlas : MonoBehaviour
{

#region
    public static AudioAtlas instance;
    void Awake() {
        if (AudioAtlas.instance) {
            Debug.Log("More than one instance of AudioAtlas found.");
            DestroyImmediate(gameObject);
        }
        instance = this;
    }
#endregion

    public enum Sound {
        selectionUnit,
        selectionButton,
        selectionError,
        attackStrike,
        moveSlide,
        hammerPass,
        deathUnit,
        deathEnemy,
    }

    public List<SFX> serializedSFX;


    [System.Serializable]
    public class SFX {
        public AudioAtlas.Sound sound;
        public List<AudioClip> audioClips;

        public AudioClip GetAudioClip() {
            return audioClips[Random.Range(0, audioClips.Count - 1)];
        }
    
    
    }
}
