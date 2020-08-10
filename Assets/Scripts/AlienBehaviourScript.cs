using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienBehaviourScript : MonoBehaviour {

    // Global Vars
    private GameObject openModel;
    private GameObject closedModel;
    private Rigidbody2D rb;
    private GameObject gameManager;
    private GameManagerScript gameManagerScript;
    public AlienState state;
    public AlienState initialState;

    //Pause State
    public AlienState lastState;
    public bool paused;


    // Closed State
    private int closedTick;
    private int projectileTick;
    public float projectileRate;
    public GameObject projectile;
    public float projectileSpeed;
    public float closedTimer;

    // Open State
    private GameObject ball;
    private float angle;
    private bool ballCaptured;
    private int openTick;
    public float openTimer;

    // BallGrab State
    public float grabSpeed;
    private float ballSpeed;

    // BallRelease State
    public float releaseSpeed;

    // Hit Vars
    public float deathTimer;

    // Game manager Vars
    public GameState gameLastState;

    private void Start() {
        // external
        gameManager = GameObject.Find("GameManager");
        gameManagerScript = gameManager.GetComponent<GameManagerScript>();

        // local
        openModel = transform.Find("Open").gameObject;
        closedModel = transform.Find("Closed").gameObject;
        rb = GetComponent<Rigidbody2D>();
        ballCaptured = false;
    }

    // Update is called once per frame
    void FixedUpdate() {
        BehaviourState();
        gameLastState = gameManagerScript.gameState;
    }


    void BehaviourState() {

        if ((gameLastState != GameState.Paused && gameManagerScript.gameState == GameState.Paused) ||
            (gameLastState == GameState.Paused && gameManagerScript.gameState != GameState.Paused)) {
            PauseToggle();
        }

        switch (state) {
            case AlienState.Initiated:
                //Transitions
                if (initialState == AlienState.Closed) {
                    state = AlienState.Closed;
                    openModel.SetActive(false);
                }
                if (initialState == AlienState.Open) {
                    state = AlienState.Open;
                    closedModel.SetActive(false);
                }
                break;

            case AlienState.Closed:
                closedTick++;
                projectileTick++;
                if (projectileTick > projectileRate * 50) {
                    FireProjectile();
                }

                //Transitions
                if (closedTick > closedTimer * 50) {
                    state = AlienState.Open;
                    closedTick = 0;
                }
                break;

            case AlienState.Open:
                openTick++;
                //Transitions
                if (openTick > openTimer * 50) {
                    state = AlienState.Closed;
                    openTick = 0;
                }
                if (ballCaptured) {
                    GrabBall();
                    angle = Random.Range(-180, 180);
                    state = AlienState.BallGrab;
                }
                break;

            case AlienState.BallGrab:
                if (angle > 0) {
                    transform.Rotate(Vector3.forward, grabSpeed);
                } else {
                    transform.Rotate(Vector3.forward, -grabSpeed);
                }
                //Transitions
                if (Mathf.Abs(Mathf.DeltaAngle(angle, transform.rotation.eulerAngles.z)) < 1) {
                    state = AlienState.BallRelease;
                    ReleaseBall();
                }
                break;

            case AlienState.BallRelease:
                if (transform.rotation.eulerAngles.z < 180) {
                    transform.Rotate(Vector3.forward, -releaseSpeed);
                } else {
                    transform.Rotate(Vector3.forward, releaseSpeed);
                }
                //Transitions
                if (Mathf.Abs(Mathf.DeltaAngle(0f, transform.rotation.eulerAngles.z)) < 1) {
                    state = AlienState.Open;
                }
                break;

            case AlienState.Hit:
                if (ballCaptured) {
                    ReleaseBall();
                }
                Destroy(this.gameObject, deathTimer);
                break;

            case AlienState.Paused:
                if (!paused) {
                    state = lastState;
                }
                break;

        }
    }

    public void PauseToggle() {
        if (!paused) {
            lastState = state;
            state = AlienState.Paused;
        } else {
            state = lastState;
        }
        paused = !paused;
    }

    void FireProjectile() {
        projectileTick = 0;
        GameObject tmp_proj = Instantiate(projectile, Vector3.zero, Quaternion.identity);
        tmp_proj.transform.parent = this.transform;
        tmp_proj.transform.localPosition = new Vector2(0, -3);
        tmp_proj.SetActive(true);
        tmp_proj.GetComponent<Rigidbody2D>().velocity = Vector2.down * projectileSpeed;
    }

    void ReleaseBall() {
        Vector2 ballReleaseSpeed = (ball.transform.position - this.transform.position).normalized * ballSpeed;
        ball.transform.parent = gameManager.transform;
        ball.SetActive(true);
        ball.GetComponent<Rigidbody2D>().velocity = ballReleaseSpeed;
        ballCaptured = false;
    }

    void GrabBall() {
        ballSpeed = ball.GetComponent<BallBehaviour>().speed;
        ball.SetActive(false);
        ball.transform.parent = this.transform;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        state = AlienState.Hit;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.transform.tag == "Ball" && state == AlienState.Open) {
            ball = other.gameObject;
            ballCaptured = true;
        }
    }


}
