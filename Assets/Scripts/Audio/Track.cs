using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Scriptable object used for playing looped music audio clips

[CreateAssetMenu(menuName = "Audio/Music Track")]
public class Track : ScriptableObject
{
    public AudioClip trackAudioClip;
    public MusicController.MusicState state; 
}
