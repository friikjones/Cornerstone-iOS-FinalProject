using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum State
    {
        normal,
        onCoolDown
    }

    public int health;
    public float coolDownCounter;
    public float movementSpeed;
    public float knockBackForce;
    public State _state;

    void Start()
    {
        health = 5;
        coolDownCounter = 0;
        _state = State.normal;
    }

    void Update()
    {
        InputHandler();
        coolDownHandler();
    }

    void InputHandler()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if(movement != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(movement);
            transform.Translate(movement * movementSpeed * Time.deltaTime, Space.World);
        }

        if(Input.GetKeyDown("o"))
        {
            if(_state != State.onCoolDown)
            {
                Debug.Log("Attack");
                _state = State.onCoolDown;
            }
        }

        if(Input.GetKeyDown("p"))
        {
            if(_state != State.onCoolDown)
            {
                Debug.Log("Defend");
                _state = State.onCoolDown;
            }
        }
    }

    void coolDownHandler()
    {
        if(_state == State.onCoolDown)
        {
            coolDownCounter += Time.deltaTime;
            if(coolDownCounter >= 3)
            {
                _state = State.normal;
                coolDownCounter = 0;
            }
        }
    }

    public void KnockBack(Vector3 enemyPos)
    {
        Vector3 posDiff = (enemyPos - transform.position).normalized;
        
        Vector3 knockBackDirection = new Vector3(posDiff.x, 0, posDiff.z);
        transform.Translate(knockBackDirection * knockBackForce, Space.World);
    }
}
