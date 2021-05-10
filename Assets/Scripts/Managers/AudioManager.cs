using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoSingleton<AudioManager> // Needs Cleaning
{
    #region Variables
    [Header("Audio")]
    [SerializeField] private bool _updateAudioSourcesDuringPlaymode;
    [SerializeField] private Sound[] _sounds;

    [Header("Volume Sliders")]
    [SerializeField] private Slider[] _musicVolumeSliders;
    [SerializeField] private Slider[] _sfxVolumeSliders;

    [Header("Components")]
    [SerializeField] private AudioMixer _mainMixer;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        UpdateAudioSourceVariables(true);

        LoadSliderValues();
    }

    private void Update()
    {
        if (_updateAudioSourcesDuringPlaymode)
        {
            UpdateAudioSourceVariables(false);
        }
    }
    #endregion

    #region Sound
    public void UpdateAudioSourceVariables(bool addAudioSource)
    {
        foreach (var sound in _sounds)
        {
            if (addAudioSource)
                sound.source = gameObject.AddComponent<AudioSource>();

            sound.source.clip = sound.clip;
            sound.source.outputAudioMixerGroup = sound.outputAudioMixerGroup;

            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;

            sound.source.enabled = !sound.disable;
        }
    }

    public void PlaySound(string name)
    {
        Sound sound = Array.Find(_sounds, _sounds => _sounds.name == name);
        sound.source.Play();
    }
    #endregion

    #region Volume Sliders
    public void SetMusicVolume(float sliderValue)
    {
        _mainMixer.SetFloat("MusicVolume", Mathf.Log10(sliderValue) * 20);

        foreach (var slider in _musicVolumeSliders)
        {
            slider.value = sliderValue;
        }

        SaveSliderValues();
    }

    public void SetSFXVolume(float sliderValue)
    {
        _mainMixer.SetFloat("SFXVolume", Mathf.Log10(sliderValue) * 20);

        foreach (var slider in _sfxVolumeSliders)
        {
            slider.value = sliderValue;
        }

        SaveSliderValues();
    }

    public void LoadSliderValues()
    {
        if (PlayerPrefs.HasKey("Music Volume"))
        {
            foreach (var slider in _musicVolumeSliders)
            {
                slider.value = PlayerPrefs.GetFloat("Music Volume");
            }

            foreach (var slider in _sfxVolumeSliders)
            {
                slider.value = PlayerPrefs.GetFloat("SFX Volume");
            }
        }
    }

    private void SaveSliderValues()
    {
        PlayerPrefs.SetFloat("Music Volume", _musicVolumeSliders[0].value);
        PlayerPrefs.SetFloat("SFX Volume", _sfxVolumeSliders[0].value);
    }
    #endregion
}
