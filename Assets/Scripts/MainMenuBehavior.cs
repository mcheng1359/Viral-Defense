using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuBehavior : MonoBehaviour {
    public void PlayGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenSettings() {
        
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void PlayHoverSound() {
        if (SoundManager.Instance != null) {
            SoundManager.Instance.PlayHoverSound();
        }
    }
}
