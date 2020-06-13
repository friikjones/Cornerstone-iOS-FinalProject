using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour {
    public float speed;
    public float killTime;

    void Start() {

    }

    void Update() {
        // transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
        Destroy(this.gameObject, killTime);
    }
}
