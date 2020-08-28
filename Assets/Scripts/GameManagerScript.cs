using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour {

    // Game Vars
    public int maxLives;
    public int lives;
    public Material bottomWallMat;
    public Color color;
    public int currentScore;
    public int highScore;
    public float ballSpeed;
    public float ballSpeedAdded;
    public int alienPoints;

    // GameState Vars
    public GameState gameState;
    public float gameTimer;
    public int gameTick;

    //UI Vars
    public GameObject pausedLabel;
    public Text pointsLabel;
    public Text timerLabel;

    // Pause Vars
    public bool pauseFlag;
    public GameObject pauseMenu;

    private void Start() {
        lives = maxLives;
        gameState = GameState.Playing;
        highScore = PlayerPrefs.GetInt("highscore", 0);
    }

    private void FixedUpdate() {
        if (gameState != GameState.Paused) {
            gameTick++;
            gameTimer = gameTick / 50;
        }
    }

    private void Update() {
        UpdateUI();
        UpdateState();

        alienPoints = Mathf.RoundToInt(ballSpeed * 50);

        if (pauseFlag) {
            PauseToggle();
            pauseFlag = false;
        }
    }

    void UpdateUI() {
        //Update Bottom Wall Color
        color = Color.HSVToRGB(lives * (120 / maxLives) / 360f, .93f, .75f);
        bottomWallMat.SetColor("_EmissionColor", color);
        if (gameState == GameState.Paused) {
            pausedLabel.SetActive(true);
            pauseMenu.SetActive(true);
        } else {
            pausedLabel.SetActive(false);
            pauseMenu.SetActive(false);
        }

        //Update Points Label
        pointsLabel.text = currentScore.ToString();

        //Update Timer Label
        // timerLabel.text = gameTimer.ToString();
    }

    void UpdateState() {
        if (lives < 1) {
            gameState = GameState.Ended;
            GameObject.Find("SceneTransitionHelper").GetComponent<SceneTransitionScript>().SceneTransitionLose();
        }
    }

    public void PauseToggle() {
        switch (gameState) {
            case GameState.Playing:
                gameState = GameState.Paused;
                break;
            case GameState.Paused:
                gameState = GameState.Playing;
                break;
        }
    }

    private void OnDestroy() {
        if (currentScore > highScore) {
            PlayerPrefs.SetInt("highscore", currentScore);
            PlayerPrefs.Save();
        }
    }


}
