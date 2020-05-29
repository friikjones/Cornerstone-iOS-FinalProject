using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float movementSpeed;

    void Start()
    {
        
    }

    void Update()
    {
        MovementInputHandler();
    }

    void MovementInputHandler()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0, moveVertical);
        transform.Translate(movement * movementSpeed * Time.deltaTime, Space.World);
    }
}
