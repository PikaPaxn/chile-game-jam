using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class OnAudioEnd : MonoBehaviour
{
    public AudioSource audioToCheck;
    public AudioSource secondAudio;

    
    void Start() {
        audioToCheck.PlayScheduled(AudioSettings.dspTime + 1.0f);
        secondAudio.PlayScheduled(AudioSettings.dspTime + audioToCheck.clip.length + 1.0f);
    }
}
