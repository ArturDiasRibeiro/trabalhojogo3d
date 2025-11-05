// ==========================
// File: Projectile.cs
// ==========================
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public float damage = 5f;
    public float lifetime = 5f;
    public GameObject owner;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == owner) return;
        var h = other.collider.GetComponent<Health>();
        if (h != null)
        {
            h.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}


