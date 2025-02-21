using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle muteToggle;

    private float _lastVolume = 0.5f;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        {
            _lastVolume = 0.5f;
            SetVolume();
        }

        if (PlayerPrefs.HasKey("muteState"))
        {
            bool isMuted = PlayerPrefs.GetInt("muteState") == 1;
            if (muteToggle != null)
                muteToggle.isOn = isMuted;
            ApplyMute(isMuted);
        }
    }

    private void Start()
    {
        // Aplica imediatamente o volume salvo ao iniciar a cena
        FindObjectOfType<BackgroundMusic>()?.ApplyVolume(PlayerPrefs.GetFloat("musicVolume", 0.5f));
    }

    private void OnEnable()
    {
        if (muteToggle != null)
            muteToggle.onValueChanged.AddListener(MuteToggle);
    }

    private void OnDisable()
    {
        if (muteToggle != null)
            muteToggle.onValueChanged.RemoveListener(MuteToggle);
    }

    public void SetVolume()
    {
        if (musicSlider == null) return;

        float volume = musicSlider.value;
        _lastVolume = volume;

        if (muteToggle == null || !muteToggle.isOn)
        {
            myMixer.SetFloat("music", Mathf.Log10(volume) * 20);
            FindObjectOfType<BackgroundMusic>()?.ApplyVolume(volume);
        }

        PlayerPrefs.SetFloat("musicVolume", volume);
        PlayerPrefs.Save();
    }

    public void MuteToggle(bool isMuted)
    {
        ApplyMute(isMuted);
        FindObjectOfType<BackgroundMusic>()?.ApplyMute(isMuted);
        PlayerPrefs.SetInt("muteState", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyMute(bool isMuted)
    {
        myMixer.SetFloat("music", isMuted ? -80f : Mathf.Log10(_lastVolume) * 20);
    }

    private void LoadVolume()
    {
        _lastVolume = PlayerPrefs.GetFloat("musicVolume");

        if (musicSlider != null)
            musicSlider.value = _lastVolume;

        SetVolume();
    }
}
