using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienSimpleBehaviour : MonoBehaviour {

    // Global Vars
    private Rigidbody2D rb;
    private GameObject gameManager;
    private GameManagerScript gameManagerScript;
    public AlienState state;
    public AlienState initialState;
    public GameState gameLastState;
    public int xPos;
    public int yPos;
    public AlienTimedSpawnerScript alienTimedSpawnerScript;

    //Pause State
    public AlienState lastState;
    public bool paused;

    // Hit Vars
    public float deathTimer;
    public AudioSource deathSound;

    // Idle Vars
    public int moveTick;
    public float moveTimer;
    public float moveSpeed;
    public bool goUp;
    public Vector2 movementDirection;


    void Start() {
        // external
        gameManager = GameObject.Find("GameManager");
        gameManagerScript = gameManager.GetComponent<GameManagerScript>();
        alienTimedSpawnerScript = transform.parent.GetComponent<AlienTimedSpawnerScript>();

        // local
        rb = GetComponent<Rigidbody2D>();
        movementDirection = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)).normalized;
        moveTick = Mathf.RoundToInt(moveTimer * 25);

        deathSound = GameObject.Find("GameManager/AlienDeathSound").GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update() {
        if ((gameLastState != GameState.Paused && gameManagerScript.gameState == GameState.Paused) ||
            (gameLastState == GameState.Paused && gameManagerScript.gameState != GameState.Paused)) {
            PauseToggle();
        }

        switch (state) {
            case AlienState.Idle:
                moveTick++;
                if (goUp) {
                    rb.velocity = movementDirection * moveSpeed;
                    if (moveTick > moveTimer * 50) {
                        goUp = !goUp;
                        moveTick = 0;
                    }
                } else {
                    rb.velocity = -movementDirection * moveSpeed;
                    if (moveTick > moveTimer * 50) {
                        goUp = !goUp;
                        moveTick = 0;
                    }
                }
                break;

            case AlienState.Hit:
                deathSound.Play();
                transform.parent.GetComponent<AlienTimedSpawnerScript>().spawnGrid[xPos, yPos] = false;
                gameManagerScript.ballSpeed += gameManagerScript.ballSpeedAdded;
                gameManagerScript.currentScore += gameManagerScript.alienPoints;
                alienTimedSpawnerScript.timerTick = 0;
                if (alienTimedSpawnerScript.alienTimer > 1.5f)
                    alienTimedSpawnerScript.alienTimer -= .1f;
                Destroy(this.gameObject);
                break;

            case AlienState.Paused:
                if (!paused) {
                    state = lastState;
                }
                break;

        }

        gameLastState = gameManagerScript.gameState;
    }

    public void PauseToggle() {
        if (!paused) {
            rb.velocity = Vector2.zero;
            lastState = state;
            state = AlienState.Paused;
        } else {
            state = lastState;
        }
        paused = !paused;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.transform.tag != "Enemy") {
            state = AlienState.Hit;
        }
    }
}
