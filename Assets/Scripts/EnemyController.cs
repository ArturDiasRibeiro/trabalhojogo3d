using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
//[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float chaseDistance = 20f;
    public float loseSightDistance = 30f;
    public float shootDistance = 18f;
    public float fieldOfView = 120f; // degrees
    public float timeBetweenShots = 0.5f;

    public Transform eyes; // origin of vision / shooting
    public LayerMask visionMask; // what can block sight (walls)
    public Transform shootPoint; // where bullets originate
    public GameObject projectilePrefab;
    public float projectileSpeed = 60f;

    NavMeshAgent agent;
    //Animator anim;
    int currentPatrolIndex = 0;
    Transform player;
    float lastShotTime = 0f;

    enum State { Patrol, Alert, Chase, Shoot }
    State state = State.Patrol;

    void Start()
    {
        Debug.Log($"Inimigo {gameObject.name} iniciou o Start()");
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // --- INÍCIO DA NOVA LÓGICA (COM DEPURAÇÃO) ---

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.Log("Array de patrulha está vazio. A tentar encontrar 'PatrolHolder'...");
            GameObject patrolHolder = GameObject.FindWithTag("PatrolHolder");

            if (patrolHolder != null)
            {
                Debug.Log($"Encontrado objeto '{patrolHolder.name}' com a tag 'PatrolHolder'.");
                List<Transform> validPoints = new List<Transform>();

                // Percorre todos os filhos
                foreach (Transform child in patrolHolder.transform)
                {
                    if (child != null)
                    {
                        Debug.Log($"-- Encontrado filho: {child.name}");
                        validPoints.Add(child);
                    }
                    else
                    {
                        Debug.LogWarning("-- Encontrado um filho NULO. A ignorar.");
                    }
                }

                // Converte a lista segura de volta para o array
                patrolPoints = validPoints.ToArray();
                Debug.Log($"Array de patrulha preenchido com {patrolPoints.Length} pontos.");
            }
            else
            {
                Debug.LogError("FALHA AO ENCONTRAR 'PatrolHolder'. O inimigo não vai patrulhar.");
            }
        }

        // --- FIM DA NOVA LÓGICA ---

        // Checagem final:
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning($"Inimigo {gameObject.name} não tem pontos de patrulha! A entrar em modo Alerta.");
            state = State.Alert;
        }
        else
        {
            // SOMENTE se tivermos pontos, ele começa a patrulhar.
            Debug.Log("A iniciar patrulha...");
            GoToNextPatrolPoint();
        }
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool canSee = CanSeePlayer();

        switch (state)
        {
            case State.Patrol:
                //anim.SetBool("isMoving", true);
                if (agent.remainingDistance < 0.5f)
                    GoToNextPatrolPoint();

                if (canSee && dist <= chaseDistance) state = State.Chase;
                break;

            case State.Chase:
                agent.isStopped = false;
                agent.SetDestination(player.position);
                //anim.SetBool("isMoving", true);

                if (!canSee && dist > loseSightDistance)
                {
                    state = State.Patrol;
                    GoToNextPatrolPoint();
                }
                else if (canSee && dist <= shootDistance)
                {
                    state = State.Shoot;
                    agent.isStopped = true;
                }
                break;

            case State.Shoot:
                transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
                //anim.SetBool("isMoving", false);
                TryShoot();

                if (!canSee && dist > loseSightDistance)
                {
                    state = State.Patrol;
                    agent.isStopped = false;
                    GoToNextPatrolPoint();
                }
                else if (dist > shootDistance)
                {
                    state = State.Chase;
                    agent.isStopped = false;
                }
                break;

            case State.Alert:
                // idle or look around
                //anim.SetBool("isMoving", false);
                if (canSee) state = State.Chase;
                break;
        }

        // update animator speed parameter from agent velocity
        //anim.SetFloat("moveSpeed", agent.velocity.magnitude);

        // Cast a short forward ray from the eyes for debugging/visibility
        CastForwardRay();
    }

    void GoToNextPatrolPoint()
    {
        // 1. Verificação de segurança principal
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogError($"GoToNextPatrolPoint foi chamada, mas o array patrolPoints está nulo ou vazio! Inimigo {gameObject.name} vai parar.");
            state = State.Alert; // Pára de tentar patrulhar
            return;
        }

        // 2. Verificação de segurança do índice (isto não devia acontecer, mas vamos verificar)
        if (currentPatrolIndex < 0 || currentPatrolIndex >= patrolPoints.Length)
        {
            Debug.LogError($"Índice de patrulha ({currentPatrolIndex}) está FORA DOS LIMITES (Tamanho={patrolPoints.Length}). A redefinir para 0.");
            currentPatrolIndex = 0;
        }

        // 3. Verificação de segurança do elemento (ESTE é o nosso suspeito)
        if (patrolPoints[currentPatrolIndex] == null)
        {
            Debug.LogError($"O ponto de patrulha no índice {currentPatrolIndex} está NULO. A saltar para o próximo.");
            // Avança para o próximo índice e tenta novamente
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            return; // Sai da função por agora, vai tentar de novo no próximo Update
        }

        // 4. Se tudo passou, define o destino
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        // 5. Avança o índice para a próxima vez
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    bool CanSeePlayer()
    {
        if (eyes == null || player == null) return false;
        Vector3 dir = (player.position - eyes.position).normalized;
        
        float angle = Vector3.Angle(eyes.forward, dir);
        if (angle > fieldOfView * 0.5f) return false;
        RaycastHit hit;
        if (Physics.Raycast(eyes.position, dir, out hit, loseSightDistance, ~visionMask))
        {
            if (hit.transform.gameObject == player.gameObject) return true;
        }
        return false;
    }

    // Casts a 5-meter ray from the eyes forward and draws it for debugging. Logs hits.
    void CastForwardRay()
    {
        if (eyes == null) return;
        Vector3 origin = eyes.position;
        Vector3 direction = eyes.forward;
        float distance = 5f;

        // Visualize the ray in the scene view
        Debug.DrawRay(origin, direction * distance, Color.red);

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, distance, ~visionMask))
        {
            Debug.Log($"Forward ray hit: {hit.transform.name} (layer {hit.transform.gameObject.layer})");
        }
    }

    void TryShoot()
    {
        if (Time.time - lastShotTime < timeBetweenShots) return;
        lastShotTime = Time.time;

        if (projectilePrefab != null && shootPoint != null)
        {
            GameObject p = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            Rigidbody rb = p.GetComponent<Rigidbody>();
            if (rb) rb.velocity = (player.position - shootPoint.position).normalized * projectileSpeed;
            var proj = p.GetComponent<EnemyProjectile>();
            if (proj != null) proj.owner = gameObject;
        }

        //anim.SetTrigger("shoot");
    }
}