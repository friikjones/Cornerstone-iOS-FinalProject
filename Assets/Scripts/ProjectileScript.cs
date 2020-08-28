using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour {


    private Rigidbody2D rb;
    public float speed;
    public Vector2 lastSpeed;

    private int flipTick;
    public float flipTimer;

    public bool paused;

    // Game manager Vars
    private GameManagerScript gameManagerScript;
    public GameState gameLastState;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
    }

    private void FixedUpdate() {
        if (!paused) {
            flipTick++;
        }
        if (flipTick > flipTimer * 50) {
            transform.localScale = new Vector2(transform.localScale.x, -transform.localScale.y);
            flipTick = 0;
        }
        if ((gameLastState != GameState.Paused && gameManagerScript.gameState == GameState.Paused) ||
            (gameLastState == GameState.Paused && gameManagerScript.gameState != GameState.Paused)) {
            PauseToggle();
        }
        gameLastState = gameManagerScript.gameState;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.transform.tag != "Player") {
            Destroy(this.gameObject);
        }
    }

    public void PauseToggle() {
        if (!paused) {
            lastSpeed = rb.velocity;
            rb.velocity = Vector2.zero;
        } else {
            rb.velocity = lastSpeed;
        }
        paused = !paused;
    }

}
