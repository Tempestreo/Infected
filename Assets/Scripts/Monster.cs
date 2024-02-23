using System.Collections;
using UnityEngine;
using UnityEngine.AI;                // navmesh kullanmak için gerekli olan kütüphaneusing UnityEngine;
public class Monster : MonoBehaviour
{
    //PostProcess

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] footsteps;
    // düþman AI geliþtirme------------------------------------------------------------------------------------------------------------------------------
    [SerializeField] AudioSource Scream;
    [SerializeField] GameObject ThePlayer;
    [SerializeField] GameObject JumpCam;

    Player player;

    //navmeshagent bölümü
    Rigidbody rb;
    Animator anim;
    public NavMeshAgent enemyAgent;
    public Transform playerTransform;             //yapayzeka düþmanýmýzýn oyuncumuzun konumunu belirlemesi için gereken transform
    public Transform MonsterHead;
    public LayerMask ground;             //yapayzeka düþmanýmýzýn ilerleyeceði zemin
    public LayerMask playerGround;

    //patrolling bölümü
    public Vector3 walkPoint;
    public float walkPointRange;
    public bool walkPointSet;

    //player detected
    public float sightRange, attackRange;
    public bool isInSightRange, isInAttackRange;

    //attack
    public float attackDelay;
    public bool isAttacking;

    //Health
    int fullHealth = 2;
    public int currentHealth;

    //animator controllers
    public bool isDied;
    private bool isMoving = false;


    //timer
    float time;
    float timer;
    float footsteptime;
    float footsteptimer;


    void Start()
    {

        anim = GetComponent<Animator>();
        enemyAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        player = FindObjectOfType<Player>();

        currentHealth = fullHealth;
        time = 20f;
        timer = Time.time;
        footsteptime = 0.8f;
        footsteptimer = Time.time;
        isDied = false;


    }

    void Update()
    {
        if (isDied == false)
        {
            isMoving = enemyAgent.remainingDistance > enemyAgent.stoppingDistance + 0.5f;

            if (isMoving)
            {
                anim.SetBool("isPatrolling", true);
            }
            else
            {
                anim.SetBool("isPatrolling", false);
            }
            timer += Time.deltaTime;
            footsteptimer += Time.deltaTime;

            isInSightRange = Physics.CheckSphere(MonsterHead.position, sightRange, playerGround);
            isInAttackRange = Physics.CheckSphere(MonsterHead.position, attackRange, playerGround);

            if (!isInSightRange && !isInAttackRange)
            {
                Patrolling();
            }
            else if (isInSightRange && !isInAttackRange)
            {
                DetectPlayer();
            }
            else if (isInSightRange && isInAttackRange)
            {
                AttackPlayer();
            }
            if (currentHealth <= 0)
            {
                isDied = true;
                anim.SetBool("isDied", isDied);
                Destroy(rb);
                Destroy(enemyAgent);
                Destroy(audioSource);
            }

        }
    }

    void Patrolling()
    {
        if (walkPointSet == false)
        {
            float randomZPos = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
            float randomXPos = UnityEngine.Random.Range(-walkPointRange, walkPointRange);

            walkPoint = new Vector3(transform.position.x + randomXPos, transform.position.y, transform.position.z + randomZPos);

            if (Physics.Raycast(walkPoint, -transform.up, 2f, ground))
            {
                walkPointSet = true;
            }
        }
        if (walkPointSet == true)
        {
            enemyAgent.SetDestination(walkPoint); 
            if (footsteptimer >= footsteptime)
            {
                footsteptimer = 0;
                audioSource.PlayOneShot(footsteps[Random.Range(0, 3)], 1);
            }
            if (timer >= time)
            {
                timer = 0;
                walkPointSet = false;
            }
        }
    }

    void DetectPlayer()
    {
        enemyAgent.SetDestination(playerTransform.position);
        transform.LookAt(playerTransform);
    }

    void AttackPlayer()
    {
        enemyAgent.SetDestination(transform.position);
        transform.LookAt(playerTransform);
        anim.SetBool("isInAttackRange", true);
        Scream.Play();
        JumpCam.SetActive(true);
        ThePlayer.SetActive(false);
        StartCoroutine(StopJumpscare());
        Destroy(this.gameObject, 2f);
    }
    IEnumerator StopJumpscare()
    {
        yield return new WaitForSeconds(1.5f);
        JumpCam.SetActive(false);
        ThePlayer.SetActive(true);
        player.isJumpscareEnded = true;
    }
    // gizmo çizimi yaparak checkSphere ve raycast gibi ýþýnlarý editör ekranýnda görünür yapabiliyoruz.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(MonsterHead.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(MonsterHead.position, sightRange);

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bullet")
        {
            currentHealth--;
        }
    }
}
