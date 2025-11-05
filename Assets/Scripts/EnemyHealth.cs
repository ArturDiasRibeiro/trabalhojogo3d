using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public bool destroyOnDeath = true;
    public GameObject deathscreen;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        //when an enemy dies, it should drop a health pack
        //instantiate health pack at enemy position
        Instantiate(Resources.Load("HealthPack"), transform.position, Quaternion.identity);
        //dont need any animations, just destroy the enemy
        if (destroyOnDeath) Destroy(gameObject);
        //need anything else?


    }
}
