using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private FloatingJoystick joystick;
    private GameObject joyBG;
    private Rigidbody rb;
    private Vector2 inputVector;
    private GameManagerScript gameManagerScript;

    public bool pauseOnDrag;
    public float speed;
    public InputState inputState;

    void Start() {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
        rb = GetComponent<Rigidbody>();
        joystick = GameObject.Find("Canvas/Floating Joystick").GetComponent<FloatingJoystick>();
        joyBG = joystick.transform.GetChild(0).gameObject;
        inputState = InputState.Idle;
    }

    void Update() {
        CheckForInput();
    }

    void CheckForInput() {
        switch (inputState) {
            case InputState.Idle:
                inputVector = Vector2.zero;
                if (joyBG.activeSelf) {
                    inputState = InputState.Dragging;
                }
                break;
            case InputState.Dragging:
                if (pauseOnDrag) {
                    rb.velocity = Vector3.zero;
                    rb.useGravity = false;
                    gameManagerScript.paused = true;
                }

                if (joystick.Horizontal != 0 && joystick.Vertical != 0) {
                    inputVector = new Vector2(joystick.Horizontal, joystick.Vertical);
                }

                if (!joyBG.activeSelf) {
                    inputState = InputState.Released;
                    rb.useGravity = true;
                    gameManagerScript.paused = false;
                }
                break;
            case InputState.Released:
                rb.velocity = new Vector3(-inputVector.x, -inputVector.y, 0) * speed;
                inputState = InputState.Idle;
                break;
        }
    }
}
