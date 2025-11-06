using UnityEngine; // Removi o System.Collections, não era necessário

public class EnemyHealth : MonoBehaviour
{
    [Header("Configuração da Vida")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool destroyOnDeath = true;

    [Header("Configuração do Loot (Seu Objetivo!)")]
    public GameObject healthPackPrefab; // Slot para o Prefab de Vida
    public GameObject coinPrefab;       // Slot para o Prefab de Moeda

    [Tooltip("Define a chance de cair a moeda (ex: 0.5 = 50%)")]
    [Range(0f, 1f)] // Cria um slider no inspector (de 0% a 100%)
    public float coinDropChance = 0.5f; // 50% de chance de dropar moeda

    void Awake()
    {
        currentHealth = maxHealth;
    }

    // Esta função é chamada pelo script de tiro do jogador
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f; // Trava a vida em 0 para não ficar negativa
            Die();
        }
    }

    void Die()
    {
        // --- INÍCIO DA NOVA LÓGICA DE LOOT ---

        // 1. Sorteia um número aleatório (entre 0.0 e 1.0)
        float randomValue = Random.value;

        // 2. Compara o número sorteado com a chance de dropar a moeda
        if (randomValue <= coinDropChance)
        {
            // Se o número for MENOR ou IGUAL à chance (ex: 0.3 <= 0.5), dropa MOEDA
            if (coinPrefab != null) // Verifica se você lembrou de colocar o prefab
            {
                Instantiate(coinPrefab, transform.position, Quaternion.identity);
            }
        }
        else
        {
            // Se o número for MAIOR (ex: 0.7 > 0.5), dropa VIDA
            if (healthPackPrefab != null) // Verifica se você lembrou de colocar o prefab
            {
                Instantiate(healthPackPrefab, transform.position, Quaternion.identity);
            }
        }
        HudManager.instance.AdicionarPontos(10);
        // --- FIM DA NOVA LÓGICA DE LOOT ---
        // Avisa ao GameManager que este inimigo morreu
        GameManager.instance.EnemyDied();
        // Esta parte continua igual:
        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }

        // Nota: Removi a linha 'Instantiate(Resources.Load("HealthPack")...',
        // pois agora usamos os prefabs que você arrasta no Inspector.
    }
}