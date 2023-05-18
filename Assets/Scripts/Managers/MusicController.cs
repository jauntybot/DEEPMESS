using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicController : MonoBehaviour
{
    
    public enum MusicState { MainMenu, Tutorial, Game };
    public MusicState currentState;
    [SerializeField] List<Track> musicTracks;
    List<Track> stateTracks = new List<Track>();
    [SerializeField] SFX recordScratch;
    [SerializeField] AnimationCurve fadeOut;
    int stateTrackIndex;


    // [Header("Text UI")]
    // [SerializeField] private TMP_Text trackTextUI;
    
    AudioSource musicAudioSource;
    Coroutine playing = null;

    void Awake()
    {
        musicAudioSource = GetComponent<AudioSource>();
    }

    public void SwitchMusicState(MusicState state, bool fade) {
        StartCoroutine(UpdateTracklist(state, fade));
    }

    public IEnumerator UpdateTracklist(MusicState targetState, bool fade) {
        
        yield return StartCoroutine(StopAudio(fade));
        
        float delay = 0;
        if (targetState == MusicState.Tutorial) {
            delay = recordScratch.Get().length;
            musicAudioSource.PlayOneShot(recordScratch.Get());
        }
        yield return new WaitForSecondsRealtime(delay);

        currentState = targetState;

        List<Track> _stateTracks = new List<Track>();
        foreach(Track t in musicTracks) {
            if (t.state == targetState) {
                _stateTracks.Add(t);
            }
        }
        stateTracks = _stateTracks;

        UpdateTrack(0);
    }

    void UpdateTrack(int index = 0)
    {
        musicAudioSource.clip = stateTracks[index].trackAudioClip;
        //trackTextUI.text = audioTracks[index].name;
        
        PlayAudio();
    }

    public void PlayAudio()
    {
        musicAudioSource.Play();
        playing = StartCoroutine(EndOfTrack());
    }

    IEnumerator EndOfTrack() {
        while (musicAudioSource.time < musicAudioSource.clip.length) {
            yield return null;
        }
        SkipForward();
    }
    
    public void PauseAudio()
    {
        
        musicAudioSource.Pause();
    }
    
    public IEnumerator StopAudio(bool fade)
    {      
        float prevVol = musicAudioSource.volume;
        if (fade) {
            float timer = 0;
            while (timer < 1.5f) {
                musicAudioSource.volume = prevVol - fadeOut.Evaluate(timer / 1.5f);
                yield return new WaitForSecondsRealtime(1/Util.fps);
                timer += Time.deltaTime;
            }
        }
        musicAudioSource.volume = prevVol;
        musicAudioSource.Stop();
    }
    
    // public void SkipBack()
    // {
    //     StopAllCoroutines();
    //     if (trackIndex >= 1)
    //     {
    //         trackIndex--;  
    //     } else {
    //         trackIndex = audioTracks.Length - 1;
    //     }
    
    //     UpdateTrack(trackIndex);
    // }

    public void SkipForward()
    {
        StopAllCoroutines();
        if (stateTrackIndex < stateTracks.Count - 1)
        {
            stateTrackIndex++;
        }
        else {
            stateTrackIndex = 0;
        }
        UpdateTrack(stateTrackIndex);
    }

    // public void AudioVolume(float musicVolume)
    // {
    //     musicAudioSource.volume = musicVolume;
    // }
}
