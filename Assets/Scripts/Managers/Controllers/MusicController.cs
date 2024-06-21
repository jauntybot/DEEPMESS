using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicController : MonoBehaviour {
    
    public enum MusicState { Null, MainMenu, Tutorial, Chunk1, Chunk2, Chunk3, Ginos };
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

    public void SwitchMusicState(MusicState state, bool fadeOut, bool fadeIn = false) {
        loop = fadeIn;
        StartCoroutine(UpdateTracklist(state, fadeOut, fadeIn));
    }

    public IEnumerator UpdateTracklist(MusicState targetState, bool fadeOut, bool fadeIn) { 
        if (targetState != MusicState.Ginos) currentState = targetState;
        if (fadeIn)
            StartCoroutine(StopAudio(fadeOut));
        else
            yield return StartCoroutine(StopAudio(fadeOut));

// Add delay for record scratch - Tutorial 
        float delay = 0;
        if (currentState == MusicState.Tutorial) {
            delay = recordScratch.Get().length;
            audioSources[sourceIndex].PlayOneShot(recordScratch.Get());
        }
        yield return new WaitForSecondsRealtime(delay);


        List<Track> _stateTracks = new();
        foreach(Track t in musicTracks) {
            if (t.state == currentState && t.ginos == (targetState == MusicState.Ginos)) {
                _stateTracks.Add(t);
            }
        }
        stateTracks = new(_stateTracks);

        loop = true;

        if (fadeIn) {
            StartCoroutine(FadeInAudio());
        } else {
            loopPt = AudioSettings.dspTime;
            QueueTrack();
        }
        
        while (loop) {
            if (AudioSettings.dspTime > loopPt - 1)
                QueueTrack();
            yield return null;
        }
    }

    void QueueTrack() {        
        stateTrackIndex++;      
        if (stateTrackIndex > stateTracks.Count - 1)     
            stateTrackIndex = 0;

        sourceIndex = 1 - sourceIndex;

        audioSources[sourceIndex].clip = stateTracks[stateTrackIndex].trackAudioClip;
        audioSources[sourceIndex].PlayScheduled(loopPt);

        trackDur = (double)stateTracks[stateTrackIndex].trackAudioClip.samples / stateTracks[stateTrackIndex].trackAudioClip.frequency;
        loopPt = loopPt + trackDur;
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

    IEnumerator FadeInAudio() {
        sourceIndex = 1 - sourceIndex;
        AudioSource source = audioSources[sourceIndex];
        source.clip = stateTracks[stateTrackIndex].trackAudioClip;
        source.Play();
        Debug.Log(trackDur + ", " + loopPt + ", " + AudioSettings.dspTime + ", " + (loopPt - AudioSettings.dspTime));
        source.time = (float)(trackDur - (loopPt - AudioSettings.dspTime));

        float prevVol = source.volume;
        float timer = 0;
        while (timer < 1.5f) {   
            source.volume = fadeOut.Evaluate(timer / 1.5f);
            yield return new WaitForSecondsRealtime(1/Util.fps);
            timer += Time.deltaTime;
        }
        source.volume = prevVol;
    }
}
