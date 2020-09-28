using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletAI : MonoBehaviour
{
    //speed of bullet
    private float bulletSpeed;

    //damge dealt by bullet
    private float bulletDamage;

    // Start is called before the first frame update
    void Start()
    {
        bulletSpeed = 10 * Time.fixedDeltaTime;

        bulletDamage = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.parent.position, bulletSpeed);
    }

    //deal damage to enemy (eventually there will probably be enums for enemy type and bullet type which this method will take in and check if the damage should be altered based on that)
    public float DamageToDeal()
    {
        return bulletDamage;
    }
}
