using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private int currentLevelIndex;
    private int lobbyIndex = 0;
    private int maxLevelIndex;


    private void Start()
    {
        maxLevelIndex = SceneManager.sceneCountInBuildSettings - 1;
        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && currentLevelIndex == 0)
        {
            LoadLevel(1);
        }
    }


    public void LoadNextLevel()
    {
        int nextLevelIndex = currentLevelIndex + 1;
        if (nextLevelIndex > maxLevelIndex)
        {
            nextLevelIndex = lobbyIndex; // Loop back to the lobby if there are no more levels
        }

        LoadLevel(nextLevelIndex);
    }

    public void LoadPreviousLevel()
    {
        int previousLevelIndex = currentLevelIndex - 1;
        if (previousLevelIndex < lobbyIndex)
        {
            previousLevelIndex = maxLevelIndex; // Wrap around to the last level if at the lobby
        }

        LoadLevel(previousLevelIndex);
    }

    public void ReloadLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }

    public void LoadLobby()
    {
        LoadLevel(lobbyIndex);
    }
}