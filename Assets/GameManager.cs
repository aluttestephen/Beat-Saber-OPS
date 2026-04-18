using UnityEngine;
using TMPro; 


public class GameManager : MonoBehaviour
{
   public static GameManager  Instance; 

    [Header("Game State")]
    public int totalScore = 0;
    public int lives = 3; 
    public int playerID = 1;

    [Header("UI")]
    public TextMeshProUGUI scoreText; 
    public TextMeshProUGUI livesText; 

    public GameObject gameOverPanel;

    public HUDController hud;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
    }
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        UpdateUI();
        if (gameOverPanel !=null) gameOverPanel.SetActive(false);
    }

    public void AddScore(int amount)
    {
        totalScore += amount; 
        UpdateUI();
        hud.UpdateScore(totalScore);
    }

    public void LoseLife()
    {
        lives--;
        UpdateUI();
        hud.UpdateLives(playerID, lives);
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + totalScore; 
        if (livesText !=null) livesText.text = "❤︎" + lives; 
    }

    void GameOver()
    {
        Debug.Log("Game Over! Final Score: " + totalScore);
        Time.timeScale = 0;

        if(gameOverPanel != null) gameOverPanel.SetActive(true);
        hud.ShowGameOver(totalScore, playerID == 1);
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
