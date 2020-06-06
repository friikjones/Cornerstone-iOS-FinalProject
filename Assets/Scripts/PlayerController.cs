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
    private CapsuleCollider collider;
    private CapsuleCollider swordCollider;

    public int health;
    public float coolDownCounter;
    public float movementSpeed;
    public float knockBackForce;
    public float attackDuration;
    public float attackCounter;
    public bool isAttacking;
    public float dodgeDuration;
    public float dodgeCounter;
    public bool isDodging;
    public State _state;
    
    void Start()
    {
        collider = GetComponent<CapsuleCollider>();
        health = 5;
        coolDownCounter = 0;
        isDodging = false;
        _state = State.normal;
    }

    void Update()
    {
        InputHandler();
        coolDownHandler();
        dodgeHandler();
        attackHandler();
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
                isAttacking = true;
                _state = State.onCoolDown;
            }
        }

        if(Input.GetKeyDown("p"))
        {
            if(_state != State.onCoolDown)
            {
                _state = State.onCoolDown;
                isDodging = true;
                collider.enabled = false;
                movementSpeed = movementSpeed * 2;
            }
        }
    }

    void attackHandler()
    {
        if(isAttacking)
        {
            attackCounter += Time.deltaTime;
            if(attackCounter > attackDuration)
            {
                isAttacking = false;
                attackCounter = 0;
            }
        }
    }

    void dodgeHandler()
    {
        if(isDodging)
        {
            dodgeCounter += Time.deltaTime;
            if(dodgeCounter > dodgeDuration)
            {
                isDodging = false;
                dodgeCounter = 0;
                movementSpeed = movementSpeed / 2;
                collider.enabled = true;
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
