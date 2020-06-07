using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEncounter : MonoBehaviour {

    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            SpawnWave();
        }
    }

    void SpawnWave() {
    }
}
