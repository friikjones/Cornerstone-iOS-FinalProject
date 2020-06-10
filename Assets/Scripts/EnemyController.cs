using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    private Transform player;
    public float movementSpeed;

    void Start()
    {
        player = GameObject.Find("Player").transform;
    }

    void Update()
    {
        MovementHandler();
    }

    void MovementHandler()
    {
        transform.LookAt(player);
        transform.position += transform.forward * movementSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Projectile")
        {
            Destroy(other.gameObject);
            Destroy(this.gameObject);
        }
    }
}
