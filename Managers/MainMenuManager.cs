using UnityEngine;

using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    
    [Tooltip("Type the EXACT name of your game scene here.")]
    public string gameSceneName = "WorldListScene"; 

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game triggered!");
        Application.Quit();
    }
}
