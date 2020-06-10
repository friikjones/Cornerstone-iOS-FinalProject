using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Vector2 movementInput;
    public Vector2 aimInput;

    void Start()
    {
        
    }

    void Update()
    {
        getInput();
    }

    void getInput()
    {
        movementInput = new Vector2(Input.GetAxisRaw("WASDHorizontal"), Input.GetAxisRaw("WASDVertical"));
        aimInput = new Vector2(Input.GetAxis("ArrowHorizontal"), Input.GetAxis("ArrowVertical"));
    }
}
