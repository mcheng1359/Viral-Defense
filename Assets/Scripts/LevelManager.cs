using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void NextLevel()
    {
        Time.timeScale = 1;
        if(SceneManager.sceneCountInBuildSettings <= SceneManager.GetActiveScene().buildIndex + 1){
            SceneManager.LoadScene(0);
        }else{
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}