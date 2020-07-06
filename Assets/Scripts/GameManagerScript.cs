using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour {

    public int maxLives;
    public int lives;
    public Material bottomWallMat;

    public Color color;

    private void Start() {
        lives = maxLives;
    }

    private void Update() {
        UpdateUI();
    }

    void UpdateUI() {
        //Update Bottom Wall Color
        color = Color.HSVToRGB(lives * (120 / maxLives) / 360f, .93f, .75f);
        bottomWallMat.SetColor("_EmissionColor", color);
    }


}
