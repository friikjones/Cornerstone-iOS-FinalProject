using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public FixedJoystick joystick;
    public float speed;

    public float margin;

    private Vector2 movementInput;
    private RectTransform thisRect;
    private RectTransform targetRect;
    private Rigidbody2D rb;

    public bool paused;


    // Game manager Vars
    private GameManagerScript gameManagerScript;
    public GameState gameLastState;


    void Start() {
        thisRect = GetComponent<RectTransform>();
        targetRect = joystick.transform.Find("Handle").GetComponent<RectTransform>();
        rb = GetComponent<Rigidbody2D>();
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
    }


    void Update() {
        if (!paused)
            getInput();

        if ((gameLastState != GameState.Paused && gameManagerScript.gameState == GameState.Paused) ||
            (gameLastState == GameState.Paused && gameManagerScript.gameState != GameState.Paused)) {
            PauseToggle();
        }
        gameLastState = gameManagerScript.gameState;
    }

    void getInput() {
        movementInput = new Vector2(targetRect.localPosition.x, 0);
        // thisRect.localPosition = Vector2.MoveTowards(thisRect.localPosition, movementInput, Time.deltaTime * speed * 100);
        rb.velocity = Vector2.zero;
        if (thisRect.localPosition.x < (movementInput.x - 2 * margin)) {
            rb.velocity = Vector2.right * speed;
        }
        if (thisRect.localPosition.x < (movementInput.x - margin)) {
            rb.velocity = Vector2.right * speed / 2;
        }
        if (thisRect.localPosition.x > (movementInput.x + 2 * margin)) {
            rb.velocity = Vector2.left * speed;
        }
        if (thisRect.localPosition.x > (movementInput.x + margin)) {
            rb.velocity = Vector2.left * speed / 2;
        }
    }

    public void PauseToggle() {
        paused = !paused;
    }
}
