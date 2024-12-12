using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // For TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isGameActive = false;

    // Gameplay fields
    public int score = 0;
    public float timeRemaining = 60f;

    // In-Game UI (displayed during gameplay)
    public GameObject inGameUI;       // Contains scoreText, timeText, etc.
    public TMP_Text scoreText;
    public TMP_Text timeText;

    // End Game UI (displayed after game is over)
    public GameObject endGameUI;      // Contains GameOverText, Leaderboard UI, Restart Button
    public TMP_Text gameOverText;
    public TMP_Text leaderboardText;  // Display top 10 scores + player rank together

    private const int MaxScores = 10; // Top 10 scores
    private int playerRank = -1;      // Will store player's rank in the leaderboard

    void Awake()
    {
        // Ensure only one instance of GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (isGameActive)
        {
            // Countdown timer
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                EndGame();
            }

            UpdateUI();
        }
    }

    public void StartGame()
    {
        isGameActive = true;
        score = 0;
        timeRemaining = 60f;

        // Show in-game UI, hide end game UI
        if (inGameUI != null) inGameUI.SetActive(true);
        if (endGameUI != null) endGameUI.SetActive(false);

        UpdateUI();
    }

    public void EndGame()
    {
        isGameActive = false;
        SaveScore(score); // Save the player's final score to leaderboard
        DisplayEndGameUI();
    }

    public void RestartGame()
    {
        Debug.Log("Restarting Game...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddScore(int amount)
    {
        if (isGameActive)
        {
            score += amount;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        // Update in-game UI
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (timeText != null)
            timeText.text = "Time: " + Mathf.FloorToInt(timeRemaining);
    }

    void DisplayEndGameUI()
    {
        if (inGameUI != null) inGameUI.SetActive(false);
        if (endGameUI != null) endGameUI.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = "Game Over! Your Final Score: " + score;

        // Load scores and display top 10 plus player's rank
        int[] scores = LoadScores();
        UpdateLeaderboard(scores, score);

        // Construct leaderboard text (Top 10 + Player Rank)
        if (leaderboardText != null)
        {
            leaderboardText.text = "Top " + MaxScores + " Scores:\n";
            for (int i = 0; i < Mathf.Min(scores.Length, MaxScores); i++)
            {
                leaderboardText.text += (i + 1) + ". " + scores[i] + "\n";
            }

            // If player's rank is found, display it at the end
            if (playerRank > 0)
            {
                leaderboardText.text += "\nYour Rank: " + playerRank;
            }
        }
    }

    // Save the current player's score to the leaderboard
    void SaveScore(int newScore)
    {
        // Load existing scores
        int[] scores = LoadScores();

        // Insert the new score into the array
        int[] updatedScores = new int[scores.Length + 1];
        for (int i = 0; i < scores.Length; i++)
        {
            updatedScores[i] = scores[i];
        }
        updatedScores[scores.Length] = newScore;

        // Sort scores descending
        System.Array.Sort(updatedScores);
        System.Array.Reverse(updatedScores);

        // Save back to PlayerPrefs
        for (int i = 0; i < updatedScores.Length; i++)
        {
            PlayerPrefs.SetInt("Score" + i, updatedScores[i]);
        }
        PlayerPrefs.SetInt("ScoreCount", updatedScores.Length);
        PlayerPrefs.Save();
    }

    // Load all saved scores from PlayerPrefs
    int[] LoadScores()
    {
        int count = PlayerPrefs.GetInt("ScoreCount", 0);
        int[] scores = new int[count];
        for (int i = 0; i < count; i++)
        {
            scores[i] = PlayerPrefs.GetInt("Score" + i, 0);
        }

        // Sort descending
        System.Array.Sort(scores);
        System.Array.Reverse(scores);
        return scores;
    }

    // Find the player's rank after loading and sorting scores
    void UpdateLeaderboard(int[] scores, int playerScore)
    {
        playerRank = -1;
        for (int i = 0; i < scores.Length; i++)
        {
            if (scores[i] == playerScore)
            {
                playerRank = i + 1;
                break;
            }
        }
    }
}
