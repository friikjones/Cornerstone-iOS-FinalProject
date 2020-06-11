using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private InputManager manager;
    public GameObject projectile;
    public Rigidbody rb;
    public float projectileSpeed;
    public float timeBetweenShots;
    public float shotCounter;
    public Transform firePoint;
    public int health;
    public float movementSpeed;

    void Start() {
        manager = GameObject.Find("GameManager").GetComponent<InputManager>();
        rb = GetComponent<Rigidbody>();
        health = 5;
    }

    void Update() {
        getMovement();
        fireHandler();
    }

    void getMovement() {
        Vector3 movement = new Vector3(manager.movementInput.x, 0, manager.movementInput.y);
        Vector3 aim = new Vector3(manager.aimInput.x, 0, manager.aimInput.y);
        if (aim != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation(aim);
        }
        if (movement != Vector3.zero) {
            // transform.Translate(movement * movementSpeed * Time.deltaTime, Space.World);
            rb.velocity = movement * movementSpeed;
        }
    }

    void fireHandler() {
        if (manager.aimInput != Vector2.zero) {
            shotCounter -= Time.deltaTime;
            if (shotCounter <= 0) {
                shotCounter = timeBetweenShots;
                GameObject newProjectile = Instantiate(projectile, firePoint.position, firePoint.rotation);
                newProjectile.GetComponent<ProjectileController>().speed = projectileSpeed;
            }
        }
    }
}