using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour {

    public int maxLives;
    public int lives;
    public Material bottomWallMat;
    public Color color;

    // GameState Vars
    public GameState gameState;

    //UI Vars
    public GameObject pausedLabel;
    public Text pointsLabel;

    // Test Vars
    public bool pauseTest;

    private void Start() {
        lives = maxLives;
        gameState = GameState.Playing;
    }

    private void Update() {
        UpdateUI();
        UpdateState();

        if (pauseTest) {
            PauseToggle();
            pauseTest = false;
        }
    }

    void UpdateUI() {
        //Update Bottom Wall Color
        color = Color.HSVToRGB(lives * (120 / maxLives) / 360f, .93f, .75f);
        bottomWallMat.SetColor("_EmissionColor", color);
        if (gameState == GameState.Paused) {
            pausedLabel.SetActive(true);
        } else {
            pausedLabel.SetActive(false);
        }
    }

    void UpdateState() {
        if (lives < 1) {
            gameState = GameState.Ended;
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



}
