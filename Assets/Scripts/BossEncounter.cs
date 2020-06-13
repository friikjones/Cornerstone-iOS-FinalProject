using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEncounter : MonoBehaviour {
    public GameObject enemy;
    public int roomsEntered;
    public GameManagerScript gameManagerScript;
    public Vector3 initialDiff;
    public int range = 4;

    private void Start() {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            SpawnWave(gameManagerScript.roomsEntered / 2);
            gameManagerScript.roomsEntered++;
        }
    }

    void SpawnWave(int difficulty) {
        // for (int i = 0; i < (difficulty + 1) * 2; i++) {
        GameObject tmp_enemy = Instantiate(enemy, Vector3.zero, Quaternion.identity);
        tmp_enemy.transform.parent = this.transform;
        tmp_enemy.transform.localPosition = initialDiff + new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
        // }
    }
}
