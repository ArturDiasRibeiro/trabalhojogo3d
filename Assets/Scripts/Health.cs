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
        // Awake() agora SÓ se preocupa com ele mesmo.
        currentHealth = maxHealth;
    }

    void Start()
    {
        // Start() é chamado DEPOIS de todos os Awakes terem corrido.
        // É 100% garantido que o HudManager.instance já existe.
        HudManager.instance.AtualizarVida(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f) Die();
        HudManager.instance.AtualizarVida(currentHealth, maxHealth);
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

        // --- INÍCIO DA MELHORIA ---
        // Destrava o cursor e o torna visível
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // --- FIM DA MELHORIA ---
    }

    public void Heal(float amount)
    {
        // Adiciona a cura à vida atual
        currentHealth += amount;

        // "Clamp" (trava) a vida para que ela não ultrapasse o máximo
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Atualiza a HUD para mostrar a nova vida!
        HudManager.instance.AtualizarVida(currentHealth, maxHealth);

        Debug.Log($"Jogador curou {amount}. Nova vida: {currentHealth}");
    }
}

