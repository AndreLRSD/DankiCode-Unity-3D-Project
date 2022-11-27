using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CombatEnemy : MonoBehaviour
{
    [Header("Atributtes")]
    public float totalHealth;
    public float attackDamage;
    public float movementSpeed;
    public float lookRadius;
    public float colliderRadius = 2f;
    public float rotationSpeed;

    [Header("Components")]
    private Animator anim;
    private CapsuleCollider capsule;
    private NavMeshAgent nav;

    [Header("Other")]
    private Transform player;
    private bool walking;
    private bool attacking;
    private bool waitFor;
    private bool hiting;
    private bool playerAlive;

    [Header("WayPoints")]
    public List<Transform> wayPoints = new List<Transform>();
    public int currentPathIndex;
    public float pathDistance;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        capsule = GetComponent<CapsuleCollider>();
        nav = GetComponent<NavMeshAgent>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (totalHealth > 0)
        {
            float distance = Vector3.Distance(player.position, transform.position);

            if (distance <= lookRadius)
            {
                nav.isStopped = false;

                if (!attacking)
                {
                    nav.SetDestination(player.position);
                    anim.SetBool("Walk Forward", true);
                    walking = true;
                }
                if (distance <= nav.stoppingDistance)
                {
                    StartCoroutine("Attack");
                    LookTarget();
                }
                else
                {
                    attacking = false;
                }
            }
            else
            {
                //nav.isStopped = true;
                anim.SetBool("Walk Forward", false);
                walking = false;
                attacking = false;
                MoveToWayPoint();
            }
        }
    }

    void MoveToWayPoint()
    {
        if(wayPoints.Count > 0)
        {
            float distance = Vector3.Distance(wayPoints[currentPathIndex].position, transform.position);
            nav.destination = wayPoints[currentPathIndex].position;

            if(distance <= pathDistance)
            {
                currentPathIndex = Random.Range(0, wayPoints.Count);
            }

            anim.SetBool("Walk Forward", true);
            walking = true;
        }
    }


    IEnumerator Attack()
    {
        if(!waitFor && !hiting && !playerAlive)
        {
            waitFor = true;
            attacking = true;
            walking = false;
            anim.SetBool("Walk Forward", false);
            anim.SetBool("Bite Attack", true);

            yield return new WaitForSeconds(1.2f);

            GetPlayer();

           // yield return new WaitForSeconds(1f);
            waitFor = false;
        }

        if(playerAlive)
        {
            anim.SetBool("Walk Forward", false);
            anim.SetBool("Bite Attack", false);
            walking = false;
            attacking = false;
            nav.isStopped = true;
        }


    }

    void GetPlayer()
    {
 
        foreach (Collider c in Physics.OverlapSphere(transform.position + transform.forward * colliderRadius, colliderRadius))
        {
            if (c.gameObject.CompareTag("Player"))
            {
                c.gameObject.GetComponent<Player>().Gethit(attackDamage);
                playerAlive = c.gameObject.GetComponent<Player>().isDead;
            }
        }
    }

    public void Gethit(float damage)
    {
        totalHealth -= damage;

        if (totalHealth > 0)
        {
            StopCoroutine("Attack");
            anim.SetTrigger("Take Damage");
            hiting = true;
            StartCoroutine("RecoveryFromHit");
        }
        else 
        {
            anim.SetTrigger("Die");
        }
    }

    IEnumerator RecoveryFromHit()
    {
        yield return new WaitForSeconds(1f);
        anim.SetBool("Walk Forward", false);
        anim.SetBool("Bite Attack", false);
        hiting = false;
        waitFor = false;
    }

    void LookTarget()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
