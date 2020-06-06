using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCheck : MonoBehaviour
{
    PlayerController player;

    void Start()
    {
        player = this.gameObject.GetComponent<PlayerController>();
    }

    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision");
        if(other.gameObject.tag == "Enemy")
        {
            if(player.health > 0)
            {
                player.health--;
            }
            player.KnockBack(other.transform.position);
        }
    }
}
