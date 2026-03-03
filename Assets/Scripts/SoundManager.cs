using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance { get; private set; }
    [Header("Slider")]
    public Slider volume;

    [Header("Audio Sources")]
    private AudioSource musicSource;
    private AudioSource audioSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip hoverSFX;
    public AudioClip decideSFX;

    void FixedUpdate(){
        if (volume != null) {
        musicSource.volume = volume.value;
        audioSource.volume = volume.value;
        }
    }

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        } else {
            Destroy(gameObject);
            return;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        audioSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.volume = 0.25f;
        audioSource.volume = 0.75f;

        PlayMusic();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        RestartMusic();
    }

    public void PlayMusic() {
        if (backgroundMusic != null) {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void RestartMusic() {
        if (musicSource.isPlaying) {
            musicSource.Stop(); 
        }
        PlayMusic();
    }

    public void PlayHoverSound() {
        if (hoverSFX != null) {
            audioSource.PlayOneShot(hoverSFX);
        }
    }

    public void PlayDecideSound() {
        if (decideSFX != null) {
            audioSource.PlayOneShot(decideSFX);
        }
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
