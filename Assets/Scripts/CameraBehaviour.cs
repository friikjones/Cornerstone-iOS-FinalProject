using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour {
    public GameObject player;
    public float drag;

    void Update() {
        if (player.transform.position.y - drag > transform.position.y) {
            transform.position = new Vector3(transform.position.x, player.transform.position.y - drag, transform.position.z);
        }
    }
}
