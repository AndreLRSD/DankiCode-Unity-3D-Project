using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private CharacterController controller;
    public float speed;
    public float gravity;
    public float damage;
    public float totalHealth;

    private Animator anim;

    public float smoothRotTime;
    private float turnSmoothVelocity;

    public float colliderRadius;
    public List<Transform> enemyList = new List<Transform>();

    private Transform cam;
    private bool walking;
    private bool waitFor;
    private bool isHit;
    public bool isDead;

    Vector3 moveDirection;




    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;

    }

    // Update is called once per frame
    void Update()
    {
        if(!isDead)
        { 
            Move();
            GetMouseInput();
        }

    }

    void Move()
    {
        if (controller.isGrounded)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontal, 0f, vertical);

            if (direction.magnitude > 0)
            {
                if (!anim.GetBool("Attack"))
                {
                    float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                    float smothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref turnSmoothVelocity, smoothRotTime);

                    transform.rotation = Quaternion.Euler(0f, smothAngle, 0f);

                    moveDirection = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * speed;

                    anim.SetInteger("Transition", 1);
                    walking = true;
                }
                else
                {
                    anim.SetBool("Walk", false);
                    
                    moveDirection = Vector3.zero;
                }

            }
            else if(walking && !anim.GetBool("Attack"))
            {
                anim.SetBool("Walk", false);
                moveDirection = Vector3.zero;
                anim.SetInteger("Transition", 0);

                walking = false;
            }
        }
        moveDirection.y -= gravity * Time.deltaTime;

        controller.Move(moveDirection * Time.deltaTime);
    }

    void GetMouseInput()
    {
        if(controller.isGrounded)
        {
            if(Input.GetMouseButtonDown(0))
            {
                if(anim.GetBool("Walk"))
                {
                    anim.SetBool("Attack", false);
                    anim.SetInteger("Transition", 0);
                }

                if(!anim.GetBool("Walk"))
                {
                    StartCoroutine("Attack");
                }
            }
        }
    }

    IEnumerator Attack()
    {
        if (!waitFor && !isHit)
        {
            waitFor = true;
            anim.SetBool("Attack", true);
            anim.SetInteger("Transition", 2);
            yield return new WaitForSeconds(0.4f);

            GetEnemiesList();

            foreach (Transform e in enemyList)
            {
                CombatEnemy enemy = e.GetComponent<CombatEnemy>();

                if (enemy != null)
                {
                    enemy.Gethit(damage);
                }
            }

            yield return new WaitForSeconds(1f);
            anim.SetInteger("Transition", 0);
            anim.SetBool("Attack", false);
            waitFor = false;
        }
    }

    void GetEnemiesList()
    {
        enemyList.Clear();
        foreach(Collider c in Physics.OverlapSphere(transform.position + transform.forward * colliderRadius, colliderRadius))
        {
            if(c.gameObject.CompareTag("Enemy"))
            {
                enemyList.Add(c.transform);
            }
        }
    }

    public void Gethit(float damage)
    {
        totalHealth -= damage;

        if (totalHealth > 0)
        {
            StopCoroutine("Attack");
            anim.SetInteger("Transition", 3);
            isHit = true;
            StartCoroutine("RecoveryFromHit");
        }
        else
        {
            isDead = true;
            anim.SetTrigger("Die");
        }
    }

    IEnumerator RecoveryFromHit()
    {
        yield return new WaitForSeconds(1f);
        anim.SetInteger("Transition", 0);
        isHit = false;
        anim.SetBool("Attack", false);
      
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, colliderRadius);
    }
}
