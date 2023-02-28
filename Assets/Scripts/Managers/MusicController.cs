using System;
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

    private void Start()
    {
        musicAudioSource = GetComponent<AudioSource>();

        trackIndex = 0;
        musicAudioSource.clip = audioTracks[trackIndex].trackAudioClip;
        trackTextUI.text = audioTracks[trackIndex].name;
    }

    public void PlayAudio()
    {
        musicAudioSource.Play();
    }
    
    public void PauseAudio()
    {
        musicAudioSource.Pause();
    }
    
    public void StopAudio()
    {
        musicAudioSource.Stop();
    }
    
    public void SkipBack()
    {
        if (trackIndex >= 1)
        {
            trackIndex--;
            UpdateTrack(trackIndex);
        }
    }

    public void SkipForward()
    {
        if (trackIndex < audioTracks.Length - 1)
        {
            trackIndex++;
            UpdateTrack(trackIndex);
        }
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
