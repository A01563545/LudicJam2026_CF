using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    Dead,
    Won
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameState state;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        state = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void WinLevel()
    {
        state = GameState.Won;

        Debug.Log("LEVEL COMPLETE");

        // simple option for now:
        Time.timeScale = 0f;

        // later you can load next scene instead
        // SceneManager.LoadScene(nextLevelIndex);
    }
}