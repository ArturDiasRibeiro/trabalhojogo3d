// ==========================

// File: Projectile.cs

// ==========================

using UnityEngine;



[RequireComponent(typeof(Rigidbody))]

public class PlayerProjectile : MonoBehaviour

{

    public float damage = 20f;

    public float lifetime = 2f;

    public GameObject owner;



    void Start()

    {

        Destroy(gameObject, lifetime);

    }



    void OnCollisionEnter(Collision other)

    {

        if (other.gameObject == owner) return;

        var h = other.collider.GetComponent<EnemyHealth>();

        if (h != null)

        {

            h.TakeDamage(damage);

        }

        Destroy(gameObject);

    }

}