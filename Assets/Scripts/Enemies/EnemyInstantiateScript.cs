using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInstantiateScript : MonoBehaviour {

    public GameObject movingEnemy;
    public GameObject targetEnemy;
    public Mesh SpawnArea;

    void Start() {
        GameObject tmp_Enemy = Instantiate(movingEnemy, Vector3.zero, Quaternion.identity);
        //Find a GODDAMN point within the specified area
        Vector3 randomPos = Vector3.zero;

        tmp_Enemy.transform.position = this.transform.position + randomPos;
        tmp_Enemy.transform.parent = this.transform;

        int number = Random.Range(0, 4);
        for (int i = 0; i < number; i++) {
            GameObject tmp_target = Instantiate(targetEnemy, Vector3.zero, Quaternion.identity);
            //Find a GODDAMN point within the specified area
            Vector3 randomTarget = Vector3.zero;

            tmp_target.transform.position = this.transform.position + randomTarget;
            tmp_target.transform.parent = this.transform;
            tmp_target.transform.parent = tmp_Enemy.transform;
        }

    }
}
