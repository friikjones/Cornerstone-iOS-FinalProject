using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionScript : MonoBehaviour {

    public void SceneTransitionGame() {
        GameObject.FindGameObjectWithTag("ButtonSFX").GetComponent<AudioSource>().Play();
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void SceneTransitionCredits() {
        GameObject.FindGameObjectWithTag("ButtonSFX").GetComponent<AudioSource>().Play();
        SceneManager.LoadScene("CreditMenu", LoadSceneMode.Single);
    }

    public void SceneTransitionMenu() {
        GameObject.FindGameObjectWithTag("ButtonSFX").GetComponent<AudioSource>().Play();
        SceneManager.LoadScene("InitialMenu", LoadSceneMode.Single);
    }

    public void SceneTransitionLose() {
        SceneManager.LoadScene("LoseMenu", LoadSceneMode.Single);
    }
}
