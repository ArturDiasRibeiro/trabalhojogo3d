using UnityEngine;

public class Pickup : MonoBehaviour
{
    // Criamos um "dropdown" para o Inspector
    public enum PickupType
    {
        Health,
        Coin
    }

    [Header("Configuração do Item")]
    public PickupType type; // O dropdown (Saúde ou Moeda)
    public int value = 1;     // Quanto vale (ex: 25 de vida ou 5 pontos)

    [Header("Efeitos")]
    public AudioClip pickupSound; // Som de coleta (opcional)
                                  // Você poderia adicionar um 'public GameObject pickupEffect;' aqui


    // Esta função é a chave. Ela é chamada quando algo ENTRA no "Trigger"
    void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto que tocou é o "Player" (pela Tag)
        if (other.CompareTag("Player"))
        {
            // Se for o jogador, decide o que fazer
            switch (type)
            {
                case PickupType.Health:
                    // Tenta encontrar o script Health no jogador e curá-lo
                    Health playerHealth = other.GetComponent<Health>();
                    if (playerHealth != null)
                    {
                        playerHealth.Heal(value);
                    }
                    break;

                case PickupType.Coin:
                    // Chama o HudManager e adiciona a pontuação
                    HudManager.instance.AdicionarPontos(value);
                    break;
            }

            // --- Finalmente, se destrói ---

            // Toca o som (se houver um)
            if (pickupSound != null)
            {
                // Toca o som na posição do item antes de destruí-lo
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // Destrói o objeto (a moeda ou o healthpack)
            Destroy(gameObject);
        }
    }
}