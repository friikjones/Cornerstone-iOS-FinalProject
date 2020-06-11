using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    private Transform player;
    public Transform firePoint;
    public GameObject projectile;
    public Rigidbody rb;
    public bool shouldFire;
    public float projectileSpeed;
    public float timeBetweenShots;
    public float shotCounter;
    public float movementSpeed;

    void Start()
    {
        player = GameObject.Find("Player").transform;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        MovementHandler();
        fireHandler();
    }

    void MovementHandler()
    {
        transform.LookAt(player);
        rb.velocity = transform.forward * movementSpeed;
    }

    void fireHandler()
    {
        if(shouldFire)
        {
            shotCounter -= Time.deltaTime;
            if(shotCounter <=0)
            {
                shotCounter = timeBetweenShots;
                GameObject newProjectile = Instantiate(projectile, firePoint.position, firePoint.rotation);
                newProjectile.GetComponent<ProjectileController>().speed = projectileSpeed;
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Projectile")
        {
            Destroy(other.gameObject);
            Destroy(this.gameObject);
        }
    }
}
