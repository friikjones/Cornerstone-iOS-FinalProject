using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienTimedSpawnerScript : MonoBehaviour {

    // Global Vars
    private GameObject gameManager;
    private GameManagerScript gameManagerScript;
    public GameState gameLastState;

    // Timer
    public bool paused;
    public int timerTick;
    public float alienTimer;

    //Spawner Vars
    public GameObject alienSimple;
    public bool[,] spawnGrid;
    public GameObject[,] spawnedAliens;


    void Start() {
        // external
        gameManager = GameObject.Find("GameManager");
        gameManagerScript = gameManager.GetComponent<GameManagerScript>();

        // internal
        spawnGrid = new bool[4, 6];
    }

    void Update() {

        if ((gameLastState != GameState.Paused && gameManagerScript.gameState == GameState.Paused) ||
                    (gameLastState == GameState.Paused && gameManagerScript.gameState != GameState.Paused)) {
            PauseToggle();
        }
        if (!paused) {
            timerTick++;
        }
        if (timerTick > alienTimer * 50) {
            AddNewAlien();
            // LogGrid();
            timerTick = 0;
        }
        gameLastState = gameManagerScript.gameState;
    }

    public void AddNewAlien() {
        for (int i = 0; i < 5; i++) {
            int x = Random.Range(0, 4);
            int y = Random.Range(0, 6);
            if (spawnGrid[x, y] == false) {
                spawnGrid[x, y] = true;
                GameObject temp = Instantiate(alienSimple, Vector3.zero, Quaternion.identity);
                temp.GetComponent<AlienSimpleBehaviour>().xPos = x;
                temp.GetComponent<AlienSimpleBehaviour>().yPos = y;
                temp.transform.parent = this.transform;
                temp.transform.localPosition = new Vector2(125 - 500 + (x * 250), 125 - 750 + (y * 250));
                break;
            }
        }
    }

    public void LogGrid() {
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < 6; j++) {
                Debug.Log("x" + i + " y" + j + "-" + spawnGrid[i, j]);
            }
        }
        Debug.Log("======================");
    }

    public void PauseToggle() {
        paused = !paused;
    }
}
