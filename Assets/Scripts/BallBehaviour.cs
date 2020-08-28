using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehaviour : MonoBehaviour {

    private Rigidbody2D rb;
    public float speed;
    public Vector2 lastSpeed;

    public bool paused;

    public AudioSource impactSound;
    public AudioSource liveLost;

    // Game manager Vars
    private GameManagerScript gameManagerScript;
    public GameState gameLastState;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.down * speed;
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
    }

    private void OnCollisionExit2D(Collision2D other) {
        if (rb.velocity.y < 0.3f && rb.velocity.y > -0.3f) {
            rb.velocity += new Vector2(0, rb.velocity.y * 1.3f);
        }
        rb.velocity = rb.velocity.normalized * speed;
        if (other.transform.tag == "BottomWall") {
            gameManagerScript.lives--;
            liveLost.Play();
        }
        impactSound.Play();
    }

    private void Update() {
        if ((gameLastState != GameState.Paused && gameManagerScript.gameState == GameState.Paused) ||
            (gameLastState == GameState.Paused && gameManagerScript.gameState != GameState.Paused)) {
            PauseToggle();
        }
        gameLastState = gameManagerScript.gameState;
        speed = gameManagerScript.ballSpeed;
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
