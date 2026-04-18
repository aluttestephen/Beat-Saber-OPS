using UnityEngine;
using UnityEngine.UI;


public class HUDController : MonoBehaviour
{

    [Header("Score")]
    public Text scoreValue;


    [Header("Lives - Player 1 (red)")]
    public Image[] livesP1 = new Image[3];

    [Header("Lives - Player 2(blue)")]
    public Image[] livesP2 = new Image[3];

    [Header("Colors")]
    public Color heartActiveColor = new Color (1f, 0.25f, 0.25f);

    public Color heartInactiveColor = new Color (1f, 0.25f, 0.25f);

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Text gameOverText;
    public Text finalScoreText;
    public Button restartButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateScore(0);
        UpdateLives(1,3);
        UpdateLives(2,3);
    }

    public void UpdateScore(int score)
    {
        if (scoreValue != null)
        {
            scoreValue.text = score.ToString ("No");
        }
    }


    public void UpdateLives(int playerID, int livesRemaining)
    {
        Image[] hearts = playerID == 1 ? livesP1  : livesP2;
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            hearts[i].color = i < livesRemaining ? heartActiveColor : heartInactiveColor;
        }
    }

    public void ShowGameOver (int finalScore, bool player1Lost)
    {
        if (gameOverPanel == null) return;
        gameOverPanel.SetActive(true);

        if (gameOverText != null)
        {
            gameOverText.text = player1Lost ? "Red player out!" : "Blue player out!";
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Score:" + finalScore.ToString("No");
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClick);
        }
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    void OnRestartClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
