using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour {

    private int flipTick;
    public float flipTimer;

    private void FixedUpdate() {
        flipTick++;
        if (flipTick > flipTimer * 50) {
            transform.localScale = new Vector2(transform.localScale.x, -transform.localScale.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.transform.tag != "Player") {
            Destroy(this.gameObject);
        }
    }
}
