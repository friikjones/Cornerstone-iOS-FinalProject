using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundScript : MonoBehaviour {
    private Rigidbody rb;

    public GameObject player;
    public float safeDist;
    public float speed;
    public float multiplier;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        rb.velocity = new Vector3(0, speed, 0);

        if (player.transform.position.y - safeDist > transform.position.y) {
            rb.velocity *= multiplier;
        }
    }

    private void OnTriggerExit(Collider other) {
        Destroy(other.gameObject);
    }

}
