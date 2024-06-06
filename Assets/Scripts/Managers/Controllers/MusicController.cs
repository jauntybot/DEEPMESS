using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicController : MonoBehaviour {
    
    public enum MusicState { MainMenu, Tutorial, Game, Null };
    public MusicState currentState;
    [SerializeField] List<Track> musicTracks;
    [SerializeField] List<Track> stateTracks = new();
    [SerializeField] SFX recordScratch;
    [SerializeField] AnimationCurve fadeOut;
    int stateTrackIndex;


    // [Header("Text UI")]
    // [SerializeField] private TMP_Text trackTextUI;
    
    [SerializeField] AudioSource[] audioSources;
    [SerializeField] int sourceIndex = 0;
    double trackDur;
    double loopPt;
    bool loop = false;

    public void SwitchMusicState(MusicState state, bool fade) {
        loop = false;
        StartCoroutine(UpdateTracklist(state, fade));
    }

    public IEnumerator UpdateTracklist(MusicState targetState, bool fade) { 
        currentState = targetState;
        yield return StartCoroutine(StopAudio(fade));

// Add delay for record scratch - Tutorial 
        float delay = 0;
        if (targetState == MusicState.Tutorial) {
            delay = recordScratch.Get().length;
            audioSources[sourceIndex].PlayOneShot(recordScratch.Get());
        }
        yield return new WaitForSecondsRealtime(delay);


        List<Track> _stateTracks = new();
        foreach(Track t in musicTracks) {
            if (t.state == targetState) {
                _stateTracks.Add(t);
            }
        }
        stateTracks = new();
        stateTracks = _stateTracks;

        loopPt = AudioSettings.dspTime;
        yield return StartCoroutine(QueueTrack());
        loop = true;
        while (loop) {
            if (AudioSettings.dspTime > loopPt - 1)
                yield return StartCoroutine(QueueTrack());
            yield return null;
        }
    }

    IEnumerator QueueTrack() {        
        if (stateTrackIndex < stateTracks.Count - 1)     
            stateTrackIndex++;      
        else 
            stateTrackIndex = 0;
        sourceIndex = 1 - sourceIndex;

        audioSources[sourceIndex].clip = stateTracks[stateTrackIndex].trackAudioClip;
        audioSources[sourceIndex].PlayScheduled(loopPt);

        trackDur = (double)stateTracks[stateTrackIndex].trackAudioClip.samples / stateTracks[stateTrackIndex].trackAudioClip.frequency;
        loopPt = loopPt + trackDur;

        yield return null;
    }    

    public IEnumerator StopAudio(bool fade) { 
        AudioSource prevSource = audioSources[sourceIndex];
        float prevVol = prevSource.volume;
        if (fade) {
            float timer = 0;
            while (timer < 1.5f) {   
                prevSource.volume = prevVol - fadeOut.Evaluate(timer / 1.5f);
                yield return new WaitForSecondsRealtime(1/Util.fps);
                timer += Time.deltaTime;
            }
        }
        prevSource.Stop();
        prevSource.volume = prevVol;
    }
}
