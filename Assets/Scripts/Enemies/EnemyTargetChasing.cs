using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTargetChasing : MonoBehaviour {

    private Rigidbody rb;
    public List<Vector3> targetArray;
    public int currentTargetIndex = 1;

    public float speed;

    void Start() {
        rb = GetComponent<Rigidbody>();
        targetArray.Add(transform.position);
        foreach (Transform t in this.transform) {
            if (t.tag == "Target") {
                targetArray.Add(t.position);
            }
        }
        transform.LookAt(targetArray[currentTargetIndex]);
    }

    void Update() {
        rb.velocity = transform.forward * speed;
        if (Vector3.Distance(transform.position, targetArray[currentTargetIndex]) < .2f) {
            currentTargetIndex++;
            currentTargetIndex = currentTargetIndex % targetArray.Count;
            transform.LookAt(targetArray[currentTargetIndex]);
        }
    }

    void OnCollisionEnter(Collision other) {
        Debug.Log("Got hit");
        if (other.transform.tag == "Player") {
            GameObject.Find("GameManager").GetComponent<GameManagerScript>().playerLives--;
        }
    }
}
