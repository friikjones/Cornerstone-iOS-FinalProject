using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private InputManager manager;
    public GameObject projectile;
    public float projectileSpeed;
    public float timeBetweenShots;
    public float shotCounter;
    public Transform firePoint;
    public int health;
    public float movementSpeed;

    public Vector3 movement;
    public Vector3 aim;

    public GameObject body;
    public GameObject cannon;

    private Rigidbody rb;

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
        movement = new Vector3(manager.movementInput.x, 0, manager.movementInput.y);
        aim = new Vector3(manager.aimInput.x, 0, manager.aimInput.y);
        if (movement != Vector3.zero) {
            rb.velocity = movement * movementSpeed;
            transform.rotation = Quaternion.LookRotation(movement);
        }
        if (aim != Vector3.zero) {
            Quaternion tmp_aim = Quaternion.LookRotation(aim);
            cannon.transform.localRotation = Quaternion.Euler(0, 0, tmp_aim.eulerAngles.y) *
                Quaternion.Euler(0, 0, -transform.rotation.eulerAngles.y);
        }
    }

    void fireHandler() {
        if (manager.aimInput != Vector2.zero) {
            shotCounter -= Time.deltaTime;
            if (shotCounter <= 0) {
                shotCounter = timeBetweenShots;
                GameObject newProjectile = Instantiate(projectile, firePoint.position, cannon.transform.localRotation);
                newProjectile.GetComponent<Rigidbody>().velocity = aim.normalized * projectileSpeed;
            }
        }
    }
}