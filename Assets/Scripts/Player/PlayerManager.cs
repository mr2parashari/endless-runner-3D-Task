using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerManager : MonoBehaviour
{
    public static bool gameOver;
    public GameObject gameOverPanel;

    public static bool isGameStarted;
    public GameObject startingText;
    public GameObject newRecordPanel;

    public static int score;
    public Text scoreText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI newRecordText;

    public static bool isGamePaused;
    public GameObject characterPrefabs;

    // Health System
    public int health = 90;  
    public TextMeshProUGUI healthText;
    private bool isRecovering = false;
    private bool isImmune = false;  // **Immunity after taking damage**

    private void Awake()
    {
        if (characterPrefabs != null)
        {
            Instantiate(characterPrefabs, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("⚠️ Character Prefab is missing! Assign in the Inspector.");
        }
    }

    void Start()
    {
        score = 0;
        
        PlayerPrefs.SetInt("TotalGems", 0); // Reset gems to 0 at the start
        PlayerPrefs.Save();
        Time.timeScale = 1;
        gameOver = isGameStarted = isGamePaused = false;
        UpdateHealthUI();
    }

    void Update()
    {
        // Update UI
        gemsText.text = "GEMS " + PlayerPrefs.GetInt("TotalGems", 0).ToString();
        scoreText.text = "SCORE " + score.ToString();

        // Game Over
        if (gameOver)
        {
            Time.timeScale = 0;
            if (score > PlayerPrefs.GetInt("HighScore", 0))
            {
                newRecordPanel.SetActive(true);
                newRecordText.text = "New \nRecord\n" + score;
                PlayerPrefs.SetInt("HighScore", score);
            }

            gameOverPanel.SetActive(true);
            Destroy(gameObject);
        }

        // Start Game
        if (SwipeManager.tap && !isGameStarted)
        {
            isGameStarted = true;
            Destroy(startingText);
        }
    }

    // Handle Health Reduction
    public void TakeDamage(int damage)
    {
        if (isImmune) return;  // **Prevent multiple damage in quick succession**

        health -= damage;
        UpdateHealthUI();

        if (health < 1)
        {
            gameOver = true;
        }
        else
        {
            StartCoroutine(DamageCooldown());  // Start immunity timer
            if (!isRecovering)
            {
                StartCoroutine(RecoverHealth());
            }
        }
    }

    // **Immunity Timer**
    private IEnumerator DamageCooldown()
    {
        isImmune = true;
        yield return new WaitForSeconds(1f);  // **1 second immunity**
        isImmune = false;
    }

    // **Health Recovery Coroutine**
    private IEnumerator RecoverHealth()
    {
        isRecovering = true;
        yield return new WaitForSeconds(5f); // Wait 5 seconds
        if (health > 0 && health < 90) // Recover only if alive and not max HP
        {
            health += 10;
            if (health > 90) health = 90; // Limit max HP
            UpdateHealthUI();
        }
        isRecovering = false;
    }

    // Update UI for Health
    private void UpdateHealthUI()
    {
        healthText.text = "HEALTH: " + health;

        if (health <= 30)
        {
            healthText.color = Color.red; // Change text color to red when at 30 HP
        }
        else
        {
            healthText.color = Color.white; // Default color
        }
    }
}
