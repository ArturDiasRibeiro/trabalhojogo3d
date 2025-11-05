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
        agent = GetComponent<NavMeshAgent>();
        //anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (patrolPoints == null || patrolPoints.Length == 0) state = State.Alert;
        GoToNextPatrolPoint();
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
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
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
            var proj = p.GetComponent<Projectile>();
            if (proj != null) proj.owner = gameObject;
        }

        //anim.SetTrigger("shoot");
    }
}