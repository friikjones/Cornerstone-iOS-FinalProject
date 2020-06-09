using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputManager manager;
    public ProjectileController projectile;
    public float projectileSpeed;
    public float timeBetweenShots;
    private float shotCounter;
    public Transform firePoint;
    public int health;
    public float movementSpeed;
    
    void Start()
    {
        manager = GameObject.Find("GameManager").GetComponent<InputManager>();
        health = 5;
    }

    void Update()
    {
        getMovement();
        fireHandler();
    }

    void getMovement()
    {
        Vector3 movement = new Vector3(manager.movementInput.x, 0, manager.movementInput.y);
        Vector3 aim = new Vector3(manager.aimInput.x, 0, manager.aimInput.y);
        if(aim != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(aim);
        }
        if(movement != Vector3.zero)
        {
            transform.Translate(movement * movementSpeed * Time.deltaTime, Space.World);
        }
    }

    void fireHandler()
    {
        while(manager.aimInput != Vector2.zero)
        {
            shotCounter -= Time.deltaTime;
            if(shotCounter <= 0)
            {
                shotCounter = timeBetweenShots;
                ProjectileController newProjectile = Instantiate(projectile, firePoint.position, firePoint.rotation) as ProjectileController;
                newProjectile.speed = projectileSpeed;
            }
        }
    }
}
