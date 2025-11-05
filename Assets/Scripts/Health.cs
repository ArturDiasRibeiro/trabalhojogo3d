// ==========================
// File: Health.cs
// ==========================
using UnityEngine;

public class Health : MonoBehaviour
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
        // play death animation if animator present
        var anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("die");

        // disable AI movement
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.isStopped = true;

        // simple destroy after short delay so animation can play
        //if (destroyOnDeath) Destroy(gameObject, 3f);
        //show death screen
        deathscreen.SetActive(true);
        //pause game
        Time.timeScale = 0f;
    }
}

