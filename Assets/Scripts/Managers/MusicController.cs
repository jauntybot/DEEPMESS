using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicController : MonoBehaviour
{
    
    public enum MusicState { MainMenu, Tutorial, Game, Null };
    public MusicState currentState;
    public bool playing;
    [SerializeField] List<Track> musicTracks;
    [SerializeField] List<Track> stateTracks = new List<Track>();
    [SerializeField] SFX recordScratch;
    [SerializeField] AnimationCurve fadeOut;
    int stateTrackIndex;


    // [Header("Text UI")]
    // [SerializeField] private TMP_Text trackTextUI;
    
    AudioSource musicAudioSource;
    Coroutine playingCo = null;

    void Awake() {
        musicAudioSource = GetComponent<AudioSource>();
    }

    public void SwitchMusicState(MusicState state, bool fade) {
        StartCoroutine(UpdateTracklist(state, fade));
    }

    public IEnumerator UpdateTracklist(MusicState targetState, bool fade) {
        
        currentState = targetState;
        yield return StartCoroutine(StopAudio(fade));

// Add delay for record scratch - Tutorial 
        float delay = 0;
        if (targetState == MusicState.Tutorial) {
            playing = false;
            delay = recordScratch.Get().length;
            musicAudioSource.PlayOneShot(recordScratch.Get());
        }
        yield return new WaitForSecondsRealtime(delay);


        List<Track> _stateTracks = new List<Track>();
        foreach(Track t in musicTracks) {
            if (t.state == targetState) {
                _stateTracks.Add(t);
            }
        }
        stateTracks = new();
        stateTracks = _stateTracks;

        UpdateTrack(0);
    }


    public void PlayAudio() {
        musicAudioSource.Play();
        playingCo = StartCoroutine(EndOfTrack());
    }

    IEnumerator EndOfTrack() {
        playing = true;
        while (musicAudioSource.time < musicAudioSource.clip.length && playing) {
            yield return null;
        }
        if (playing)
            SkipForward();
    }

    public IEnumerator StopAudio(bool fade) {      
        playing = false;
        float prevVol = musicAudioSource.volume;
        if (fade) {
            Debug.Log("Fade start");
            float timer = 0;
            while (timer < 1.5f) {
                musicAudioSource.volume = prevVol - fadeOut.Evaluate(timer / 1.5f);
                yield return new WaitForSecondsRealtime(1/Util.fps);
                timer += Time.deltaTime;
            }
            Debug.Log("Fade done");
        }
        musicAudioSource.volume = prevVol;
        musicAudioSource.Stop();
    }
    

    public void SkipForward() {    
        if (stateTrackIndex < stateTracks.Count - 1)     
            stateTrackIndex++;      
        else 
            stateTrackIndex = 0;
        
        UpdateTrack(stateTrackIndex);
    }

    void UpdateTrack(int index = 0) {
        musicAudioSource.Stop();
        playing = false;
        musicAudioSource.clip = stateTracks[index].trackAudioClip;
        Debug.Log("Playing: " + stateTracks[index]);
        //trackTextUI.text = audioTracks[index].name;
        
        PlayAudio();
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

    // public void AudioVolume(float musicVolume)
    // {
    //     musicAudioSource.volume = musicVolume;
    // }
}
