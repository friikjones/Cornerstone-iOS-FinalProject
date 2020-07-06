using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehaviour : MonoBehaviour {

    private Rigidbody2D rb;
    public float speed;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.down * speed;
    }

    private void OnCollisionExit2D(Collision2D other) {
        if (rb.velocity.y < 0.3f && rb.velocity.y > -0.3f) {
            rb.velocity += new Vector2(0, rb.velocity.y * 1.3f);
        }
        rb.velocity = rb.velocity.normalized * speed;
    }
}
