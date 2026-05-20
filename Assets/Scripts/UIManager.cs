using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    private bool isPaused = false;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }
private void Start()
{
    pausePanel.SetActive(false);
    gameOverPanel.SetActive(false);

    if (winPanel != null)
        winPanel.SetActive(false);

    AudioManager.Instance?.PlayGameMusic();
}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsGameOverShowing())
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public bool AnyMenuOpen()
{
    return
        pausePanel.activeSelf ||
        gameOverPanel.activeSelf ||
        (
            winPanel != null &&
            winPanel.activeSelf
        );
}

    public void Pause()
    {
        pausePanel.SetActive(true);

        Time.timeScale = 0f;

        AudioManager.Instance.PauseMusic();

        isPaused = true;
    }

    public void Resume()
    {
        pausePanel.SetActive(false);

        Time.timeScale = 1f;

        AudioManager.Instance.ResumeMusic();

        isPaused = false;
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);

        AudioManager.Instance.PlayLose();

        Time.timeScale = 0f;
    }

    public void ShowWin()
    {
        winPanel.SetActive(true);

        AudioManager.Instance.PlayWin();

        Time.timeScale = 0f;
    }

    private bool IsGameOverShowing()
    {
        return gameOverPanel.activeSelf ||
               (winPanel != null && winPanel.activeSelf);
    }

    public void Retry()
    {
        Time.timeScale = 1f;

        AudioManager.Instance.PlayGameMusic();

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void Quit()
    {
        Time.timeScale = 1f;

        Application.Quit();
    }

    public void BonusLevel()
    {
        AudioManager.Instance.PlayGameMusic();

        SceneManager.LoadScene("Level2");
    }
}