using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioManager 
{

    private static Dictionary<AudioAtlas.Sound, float> soundTimerDictionary;

    private static GameObject oneShotGO;
    private static AudioSource oneShotAudioSource;


// Used for 3D sounds, but needs object pooling before implementation
    public static void PlaySound(AudioAtlas.Sound sound, Vector3 position) {
        if (CanPlaySound(sound)) {
            GameObject soundGO = new GameObject("Sound");
            soundGO.transform.position = position;
            soundGO.transform.parent = AudioAtlas.instance.gameObject.transform;
            AudioSource audioSource = soundGO.AddComponent<AudioSource>();
            audioSource.clip = GetAudioClip(sound);
            audioSource.maxDistance = 100f;
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.dopplerLevel = 0f;
            audioSource.Play();

            Object.Destroy(soundGO, audioSource.clip.length);
        }
    }

    public static void PlaySound(AudioAtlas.Sound sound) {
        if (CanPlaySound(sound)) {
            if (oneShotGO == null) {
                oneShotGO = new GameObject("One Shot Audio Source");
                oneShotGO.transform.position = AudioAtlas.instance.gameObject.transform.position;
                oneShotGO.transform.parent = AudioAtlas.instance.gameObject.transform;
            }
            if (oneShotAudioSource == null) {
                AudioSource audioSource = oneShotGO.AddComponent<AudioSource>();
            }
            oneShotAudioSource.clip = GetAudioClip(sound);
            oneShotAudioSource.Play();
        }
    }

// Use for looping sounds to play after delay ?
    private static bool CanPlaySound(AudioAtlas.Sound sound) {
        switch (sound) {
            default:
                return true;
        }
    }

    private static AudioClip GetAudioClip(AudioAtlas.Sound sound) {
        AudioAtlas.SFX sfx = AudioAtlas.instance.serializedSFX.Find(sfx => sfx.sound == sound);
        if (sfx != null) {
            return sfx.GetAudioClip();
        }
        Debug.LogError("SFX " + sound + " not serialized in atlas.");
        return null;
    }

}