using UnityEngine;
using TMPro; // Importante: Precisamos disso para o TextMeshPro

public class HudManager : MonoBehaviour
{
    // Crie uma instância "pública" para que outros scripts possam nos achar
    public static HudManager instance;

    [Header("Elementos da HUD")]
    public TextMeshProUGUI vidaText;
    public TextMeshProUGUI pontosText; // <-- ADICIONAMOS ESTE CAMPO

    // Variável privada para guardar a pontuação
    private int pontuacao = 0;

    void Awake()
    {
        // Configura a instância (padrão Singleton)
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Define o texto inicial da pontuação
        if (pontosText != null)
        {
            pontosText.text = "PONTOS: 0";
        }
    }

    // Esta é a função que o script de vida do Jogador chama
    public void AtualizarVida(float vidaAtual, float vidaMaxima)
    {
        if (vidaText != null)
        {
            vidaText.text = $"VIDA: {vidaAtual} / {vidaMaxima}";
        }
    }

    // --- FUNÇÃO NOVA PARA PONTUAÇÃO ---
    // Esta função será chamada pelo inimigo quando ele morrer
    public void AdicionarPontos(int pontos)
    {
        pontuacao += pontos; // Soma os pontos
        if (pontosText != null)
        {
            // Atualiza o texto na tela
            pontosText.text = $"PONTOS: {pontuacao}";
        }
    }
}