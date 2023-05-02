
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Audio/SFX")]
public class SFX : ScriptableObject {
    public List<AudioClip> audioClips;

    public AudioClip Get() {
        return audioClips[Random.Range(0, audioClips.Count)];
    }
}
