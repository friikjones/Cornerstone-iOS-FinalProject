using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreLoader : MonoBehaviour {

    public Text highscoreLabel;

    void Start() {
        highscoreLabel.text = PlayerPrefs.GetInt("highscore", 0).ToString();
    }

}
