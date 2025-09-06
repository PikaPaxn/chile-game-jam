using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Audio;

public class VolumeControl : MonoBehaviour
{
    public AudioMixer mixer;
    public string musicVolume  = "MusicVol";
    public string soundsVolume = "SoundVol";

    public UnityEngine.UI.Slider initializeMusicSlider;
    public UnityEngine.UI.Slider initializeSoundSlider;

    void Start() {
        _oldMusicVolume = PlayerPrefs.GetFloat("MusicVol", 1);
        if (initializeMusicSlider != null)
            initializeMusicSlider.value = _oldMusicVolume;
        SetMusicVolume(_oldMusicVolume);

        _oldSoundsVolume = PlayerPrefs.GetFloat("SoundsVol", 1);
        if (initializeSoundSlider != null)
            initializeSoundSlider.value = _oldSoundsVolume;
        SetSoundsVolume(_oldSoundsVolume);
    }

    float _oldMusicVolume;
    float _oldSoundsVolume;

    public void SetMusicVolume(float vol) {
        mixer.SetFloat(musicVolume, Mathf.Log10(vol) * 20);
        PlayerPrefs.SetFloat("MusicVol", vol);
        PlayerPrefs.Save();
    }
    public void SetSoundsVolume(float vol) {
        mixer.SetFloat(soundsVolume, Mathf.Log10(vol) * 20);
        PlayerPrefs.SetFloat("SoundsVol", vol);
        PlayerPrefs.Save();
    }

    public void UnmuteMusic()  => SetMusicVolume(_oldMusicVolume);
    public void UnmuteSounds() => SetSoundsVolume(_oldSoundsVolume);

    public void MuteMusic() {
        mixer.GetFloat(musicVolume, out _oldMusicVolume);
        _oldMusicVolume = Mathf.Pow(10, _oldMusicVolume / 20);
        SetMusicVolume(0.0001f);
    }

    public void MuteSounds() {
        mixer.GetFloat(soundsVolume, out _oldSoundsVolume);
        _oldSoundsVolume = Mathf.Pow(10, _oldSoundsVolume / 20);
        SetSoundsVolume(0.0001f);
    }

}
