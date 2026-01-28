using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI MainMenu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private TextMeshProUGUI startBestScoreText;
    
    [Header("UI GamePlay")]
    [SerializeField] private GameObject gamePlayMenuPanel;
    [SerializeField] private TextMeshProUGUI currentScoreText;

    [Header("UI PauseMenu")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("UI EndMenu")]
    [SerializeField] private GameObject endMenuPanel;
    [SerializeField] private TextMeshProUGUI endCurrentScoreText;
    [SerializeField] private TextMeshProUGUI endBestScoreText;

    [Header("UI SkinsMenu")]
    [SerializeField] private GameObject skinsMenuPanel;

    [Header("Player")]
    [SerializeField] private Transform playerTransform;
    private PlayerMovement playerMovement;

    public event Action<int> OnPoinChanged;

    private bool isGameActive = false;
    private float score = 0f;
    private float startY = 0f;
    private int totalPoins = 0;

    private string key_poin = "TotalPoins";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        UpdateHeighScoreUI();
    }

    void Start()
    {
        Time.timeScale = 0f;
        isGameActive = false;
        score = 0f;

        if (playerTransform != null)
        {
            playerMovement = playerTransform.GetComponent<PlayerMovement>(); 
            startY = playerTransform.position.y;   
        }
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        if (gamePlayMenuPanel != null)
            gamePlayMenuPanel.SetActive(false);
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (endMenuPanel != null)
            endMenuPanel.SetActive(false);
        if (skinsMenuPanel != null)
            skinsMenuPanel.SetActive(false);

        totalPoins = PlayerPrefs.GetInt(key_poin, 0);
        Invoke(nameof(NotifyInitialPoin), 0.1f);
    }

    void Update()
    {
        if (!isGameActive || playerTransform == null) return;

        float currentHeight = playerTransform.position.y - startY;
        if (currentHeight > score)
        {
            score = currentHeight;
            UpdateScoreUI();
        }
    }

    public void StartGame()
    {
        isGameActive = true;
        Time.timeScale = 1f;

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (gamePlayMenuPanel != null)
            gamePlayMenuPanel.SetActive(true);
    }

    public void GameOver()
    {
        if (!isGameActive) return;
        Time.timeScale = 0f;
        isGameActive = false;
        if (playerMovement != null) playerMovement.enabled = false;

        int currentScoreInt = Mathf.FloorToInt(score);
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (currentScoreInt > highScore)
        {
            highScore = currentScoreInt;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        if (gamePlayMenuPanel != null)
            gamePlayMenuPanel.SetActive(false);
        
        if (endCurrentScoreText != null)
            endCurrentScoreText.text = currentScoreInt.ToString();
        if (endBestScoreText != null)
            endBestScoreText.text = $"Best {highScore}";
        if (endMenuPanel != null)
            endMenuPanel.SetActive(true);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public bool IsGameActive()
    {
        return isGameActive;
    }

    private void UpdateScoreUI()
    {
        if (currentScoreText != null)
            currentScoreText.text = Mathf.FloorToInt(score).ToString();
    }

    private void UpdateHeighScoreUI()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (startBestScoreText != null)
            startBestScoreText.text = $"Best {highScore}";
    }

    // ================== Poins System ==================

    private void NotifyInitialPoin()
    {
        OnPoinChanged?.Invoke(totalPoins);
    }

    public void AddPoin(int amount)
    {
        totalPoins += amount;

        PlayerPrefs.SetInt(key_poin, totalPoins);
        PlayerPrefs.Save();

        OnPoinChanged?.Invoke(totalPoins);
    }

    public int GetTotalPoin()
    {
        return totalPoins;
    }
}