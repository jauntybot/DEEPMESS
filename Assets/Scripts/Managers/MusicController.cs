using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicController : MonoBehaviour
{
    [Header("List of Tracks")]
    [SerializeField] private Track[] audioTracks;

    private int trackIndex;

    [Header("Text UI")]
    [SerializeField] private TMP_Text trackTextUI;
    
    private AudioSource musicAudioSource;
    private Coroutine playing = null;

    private void Start()
    {
        musicAudioSource = GetComponent<AudioSource>();


        trackIndex = Random.Range(0, audioTracks.Length - 1);
        UpdateTrack(trackIndex);
    }

    public void PlayAudio()
    {
        musicAudioSource.Play();
        playing = StartCoroutine(EndOfTrack());
    }

    private IEnumerator EndOfTrack() {
        while (musicAudioSource.time < musicAudioSource.clip.length) {
            yield return null;
        }
        SkipForward();
    }
    
    public void PauseAudio()
    {
        StopAllCoroutines();
        musicAudioSource.Pause();
    }
    
    public void StopAudio()
    {
        StopAllCoroutines();
        musicAudioSource.Stop();
    }
    
    public void SkipBack()
    {
        StopAllCoroutines();
        if (trackIndex >= 1)
        {
            trackIndex--;  
        } else {
            trackIndex = audioTracks.Length - 1;
        }
    
        UpdateTrack(trackIndex);
    }

    public void SkipForward()
    {
        StopAllCoroutines();
        if (trackIndex < audioTracks.Length - 1)
        {
            trackIndex++;
        }
        else {
            trackIndex = 0;
        }
        UpdateTrack(trackIndex);
    }

    void UpdateTrack(int index)
    {
        musicAudioSource.clip = audioTracks[index].trackAudioClip;
        trackTextUI.text = audioTracks[index].name;
        
        PlayAudio();
    }

    public void AudioVolume(float musicVolume)
    {
        musicAudioSource.volume = musicVolume;
    }
}
