using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    private bool isPaused = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsGameOverShowing())
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    // --- Pausa ---

    public void Pause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    // --- Game Over ---

    public void ShowGameOver()
    {
        if (gameOverPanel == null) { Debug.LogError("gameOverPanel no asignado en UIManager."); return; }
        Debug.Log("Activando: " + gameOverPanel.name);
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private bool IsGameOverShowing() => gameOverPanel.activeSelf;

    // --- Botones ---

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
