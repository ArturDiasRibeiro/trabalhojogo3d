using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Precisamos disso para os textos da UI

public class GameManager : MonoBehaviour
{
    // Singleton (para que o Inimigo possa nos achar)
    public static GameManager instance;

    [Header("Referências de Jogo")]
    public GameObject enemyPrefab; // O prefab do Inimigo que vamos spawnar
    public Transform[] spawnPoints; // A lista de locais onde eles podem nascer

    [Header("Configuração das Waves")]
    public int baseEnemiesPerWave = 2; // Começa com 2 inimigos na Wave 1
    public float timeBetweenWaves = 5.0f; // 5 segundos de espera

    [Header("Referências da UI")]
    public TextMeshProUGUI waveText; // Texto "Wave: 1"
    public TextMeshProUGUI enemiesLeftText; // Texto "Inimigos: 5"

    // Variáveis privadas de controle
    private int currentWave = 0;
    private int enemiesAlive;
    private bool isSpawningWave = false; // Trava para não bugar

    void Awake()
    {
        // Configura o Singleton
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
        // Começa o jogo!
        StartCoroutine(StartNextWaveWithDelay());
    }

    // Função pública que o Inimigo vai chamar quando morrer
    public void EnemyDied()
    {
        enemiesAlive--; // Subtrai um da contagem
        UpdateEnemiesLeftText();

        // Se todos os inimigos morreram e já não estamos spawnando a prox,
        // começa a próxima wave.
        if (enemiesAlive <= 0 && !isSpawningWave)
        {
            StartCoroutine(StartNextWaveWithDelay());
        }
    }

    // A Coroutine que controla o tempo de espera
    IEnumerator StartNextWaveWithDelay()
    {
        isSpawningWave = true; // Trava

        // Espera o tempo definido
        yield return new WaitForSeconds(timeBetweenWaves);

        // Inicia a próxima wave
        currentWave++;
        UpdateWaveText();

        // Calcula quantos inimigos devem nascer (ex: Wave 1 = 2, Wave 2 = 4, Wave 3 = 6...)
        int enemiesToSpawn = currentWave * baseEnemiesPerWave;
        enemiesAlive = enemiesToSpawn;

        UpdateEnemiesLeftText();

        // Loop para spawnar os inimigos
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
            // (Opcional) Adicionar um pequeno delay entre spawns
            // yield return new WaitForSeconds(0.5f); 
        }

        isSpawningWave = false; // Destrava
    }

    void SpawnEnemy()
    {
        // 1. Escolhe um ponto de spawn aleatório
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnIndex];

        // 2. Cria (Instantiate) o inimigo
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // (O inimigo agora está vivo e cuidará de si mesmo)
    }

    // --- Funções de UI ---

    void UpdateWaveText()
    {
        if (waveText != null)
        {
            waveText.text = $"WAVE: {currentWave}";
        }
    }

    void UpdateEnemiesLeftText()
    {
        if (enemiesLeftText != null)
        {
            enemiesLeftText.text = $"INIMIGOS: {enemiesAlive}";
        }
    }
}