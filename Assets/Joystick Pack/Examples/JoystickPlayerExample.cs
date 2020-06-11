using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickPlayerExample : MonoBehaviour {
    public float speed;
    public FixedJoystick leftJoystick;
    public FixedJoystick rightJoystick;
    public Rigidbody rb;

    public void FixedUpdate() {
        Debug.Log("Left------");
        Debug.Log("Horizontal: " + leftJoystick.Horizontal);
        Debug.Log("Vertical: " + leftJoystick.Vertical);
        Debug.Log("Right------");
        Debug.Log("Horizontal: " + rightJoystick.Horizontal);
        Debug.Log("Vertical: " + rightJoystick.Vertical);
    }
}