using UnityEngine;
using UnityEngine.Audio;

public class BackgroundMusic : MonoBehaviour
{
    public AudioMixer audioMixer;
    private static BackgroundMusic instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadVolume()
    {
        float savedVolume = PlayerPrefs.HasKey("musicVolume") ? PlayerPrefs.GetFloat("musicVolume") : 0.5f;
        bool isMuted = PlayerPrefs.HasKey("muteState") && PlayerPrefs.GetInt("muteState") == 1;

        ApplyVolume(savedVolume);
        ApplyMute(isMuted);
    }

    public void ApplyVolume(float volume)
    {
        if (PlayerPrefs.HasKey("muteState") && PlayerPrefs.GetInt("muteState") == 1)
        {
            audioMixer.SetFloat("music", -80f);
        }
        else
        {
            audioMixer.SetFloat("music", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20);
        }
    }

    public void ApplyMute(bool isMuted)
    {
        audioMixer.SetFloat("music", isMuted ? -80f : Mathf.Log10(PlayerPrefs.GetFloat("musicVolume")) * 20);
    }
}
