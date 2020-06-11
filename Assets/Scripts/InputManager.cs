using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
    public Vector2 movementInput;
    public Vector2 aimInput;

    public FixedJoystick leftJoystick;
    public FixedJoystick rightJoystick;

    void Start() {

    }

    void Update() {
        getInput();
    }

    void getInput() {
        movementInput = new Vector2(leftJoystick.Horizontal, leftJoystick.Vertical);
        aimInput = new Vector2(rightJoystick.Horizontal, rightJoystick.Vertical);
    }
}
