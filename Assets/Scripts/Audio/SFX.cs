
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Audio/SFX")]
public class SFX : ScriptableObject {
    public List<AudioClip> audioClips;
    public AudioMixerGroup outputMixerGroup;

    public AudioClip Get() {
        return audioClips[Random.Range(0, audioClips.Count)];
    }
}
