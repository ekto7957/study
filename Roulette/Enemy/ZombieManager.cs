using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ZombieManager : MonoBehaviour
{
    // enum���� ���� ���� �� ����
    public EZombieState currentState;
    public Transform target;
    public float attacRange = 1.0f;
    public float attackDelay = 2.0f;
    //private float nextAttackTime = 1.0f;
    public Transform[] patrolPoints;// ���� �������
    public float moveSpeed = 2.0f;
    private int currentPoint = 0; // ���� ���� ������� �ε���
    private float trackingRange = 3.0f; // ���� ���� ����
    private bool isAttack = false; // ���ݻ���
    private float evadeRange = 5.0f; // ���� ���� ȸ�ǰŸ�
    private float zombieHP = 10.0f;
    private float distanceTotarget; // Target���� �Ÿ� ��갪
    private bool isWaiting = false;
    public float idleTime = 2.0f; // �� ���� ��ȯ�� ��� �ð�
    private Coroutine stateRoutine;

    private Animator animator;
    private CharacterController characterController;
    private float lastLogTime = 0f; // ���������� �α׸� ����� �ð�

    private float gravity = -9.81f; // �߷� ��
    private Vector3 verticalVelocity;

    private void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        // �ʱ� ���� ����
        //ChangeState(EZombieState.Idle);
        currentState = EZombieState.Idle;

        if (currentState == EZombieState.Idle)
        {
            stateRoutine = StartCoroutine(Idle());
        }
        else if (currentState == EZombieState.Patrol)
        {
            stateRoutine = StartCoroutine(Patrol());
        }

    }

    private void Update()
    {
        if (target != null)
        {
            distanceTotarget = Vector3.Distance(transform.position, target.position);
            // 1�ʸ��� �Ÿ� ���
            if (Time.time - lastLogTime >= 1f)
            {
                Debug.Log("����� �÷��̾�� �Ÿ� : " + distanceTotarget);
                lastLogTime = Time.time; // ������ �α� ��� �ð� ����
            }
        }

        //if (zombieHP <= 0)
        //{
        //    ChangeState(EZombieState.Die);
        //    return; //return�� �����ѵ� ���� ������ ���� �Լ��� �ʿ��ϴ�?
        //}



   


        //// ���� �ӽ�
        //switch (currentState)
        //{

        //    case EZombieState.Idle:
        //        Idle();
        //        break;
        //    case EZombieState.Patrol:
        //        Patrol();
        //        break;
        //    case EZombieState.Chase:
        //        Chase();
        //        break;
        //    case EZombieState.Attack:
        //        Attack();
        //        break;
        //    case EZombieState.Evade:
        //        Evade();
        //        break;
        //    case EZombieState.Damage:
        //        Damage();
        //        break;
        //    case EZombieState.Die:
        //        Die();
        //        break;

        //}

    }

    private IEnumerator Patrol()
    {

        Debug.Log(gameObject.name + " : ������..");

        while (currentState == EZombieState.Patrol)
        {
            if (patrolPoints.Length > 0)
            {
                animator.SetBool("IsWalking", true);

                Transform targetPoint = patrolPoints[currentPoint];

                Vector3 direction = (targetPoint.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
                transform.LookAt(targetPoint.position);

                if (Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
                }

                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < attacRange)
                {
                    ChangeState(EZombieState.Attack);
                }

                else if (distance < trackingRange)
                {
                    ChangeState(EZombieState.Chase);
                    
                }
          

                
            }

            yield return null;
        }
    }
    private IEnumerator Chase()
    {

        Debug.Log(gameObject.name + " : ���� ������");





        while (currentState == EZombieState.Chase)
        {

            float distance = Vector3.Distance(transform.position, target.position);
            // Ÿ���� ���� �̵�
            //transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(target.position);
            // Ÿ���� �ٶ�

            animator.SetBool("IsRunning", true);

  
            if (distance < attacRange)
            {
                ChangeState(EZombieState.Attack);
            }
            if(distance > trackingRange * 1.5f)
            {
                ChangeState(EZombieState.Patrol);
                yield break;
                //���� �ڷ�ƾ�� ��� ���� �浹 ����
            }



            yield return null;
        }
      
    }

    private IEnumerator Attack()
    {


     
        animator.SetTrigger("Attack");
        Debug.Log(gameObject.name + " : ���� �����ؿ´�");
        transform.LookAt(target.position);

        yield return new WaitForSeconds(attackDelay);

        float distance = Vector3.Distance(transform.position,target.position);
        if (distance > attacRange)
        {

            ChangeState(EZombieState.Chase);
        }
        else
        {
            ChangeState(EZombieState.Attack);
        }
            
       
    }
    
    private IEnumerator Evade()
    {
        Debug.Log(gameObject.name + " : ���� ȸ����  ");
        animator.SetBool("IsWalking", true);
        Vector3 evadeDirection = (transform.position - target.position).normalized;
        float evadeTime = 3.0f;
        float timer = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(evadeDirection);
        transform.rotation = targetRotation;

        while(currentState == EZombieState.Evade && timer < evadeTime)
        {
            transform.position += evadeDirection * moveSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        ChangeState(EZombieState.Idle);
    }
    void Damage()
    {
        Debug.Log(gameObject.name + " : ���� �ǰݴ���");
    }


    private IEnumerator IdleWait() 
    { 
        isWaiting = true;
        yield return new WaitForSeconds(idleTime);
        isWaiting = false;
    }

    private IEnumerator DamageWait()
    {
        yield return new WaitForSeconds(1.0f);

        if (zombieHP <= 0)

        {
            ChangeState(EZombieState.Idle);

        }

        else if (zombieHP < 3.0f)
        {
            ChangeState(EZombieState.Evade);
        }

        else
        {
            ChangeState(EZombieState.Patrol);
        }

    }

    public void ChangeState(EZombieState newstate)
    {
        // �ݺ��Ǵ� ���̽��� �ִٸ� �ϴ� �ڷ�ƾ���Ἥ ����������
        if (stateRoutine != null)
        {

            StopCoroutine(stateRoutine);    
        }

        animator.SetBool("IsRunning", false); // �߰�
        animator.SetBool("IsWalking", false); // �߰�

        // ���� ���� ���¸� ������·� �ٲ�����
        currentState = newstate;

        // ����ġ���� ���� ���¸��� �ڷ�ƾ ȣ��, �ڷ�ƾ�� ���� �׷���?
        switch(currentState)
        {
            case EZombieState.Idle:
                stateRoutine = StartCoroutine(Idle()); 
                break;
            case EZombieState.Patrol:
                stateRoutine = StartCoroutine(Patrol());
                break;
            case EZombieState.Evade:
                stateRoutine = StartCoroutine(Evade());

                break;
            case EZombieState.Chase:
                stateRoutine = StartCoroutine(Chase());
                break;
            case EZombieState.Damage:
                stateRoutine = StartCoroutine(TakeDamage(1.0f));
                break;
                //case EZombieState.Chase:
                //    stateRoutine = StartCoroutine());
                //    break;
                //case EZombieState.Chase:
                //    stateRoutine = StartCoroutine());
                //    break;
        }

    }

    private IEnumerator Idle()
    {
        Debug.Log(gameObject.name + " : �����");
        animator.Play("Idle");

        while(currentState == EZombieState.Idle)
        {
            float distance = Vector3.Distance(transform.position , target.position);

            if(distance < trackingRange)
            {
                ChangeState(EZombieState.Chase);
            }
            else if(distance < attacRange)
            {
                ChangeState(EZombieState.Attack);
            }

            yield return null;


        }
    }

    public IEnumerator TakeDamage(float damage)
    {
        Debug.Log(gameObject.name + $" {damage} : ������ ����");
        animator.SetTrigger("Damage");
        zombieHP -= damage;

        if(zombieHP <= 0)
        {
            ChangeState(EZombieState.Die);

        }

        else
        {
            ChangeState(EZombieState.Chase);
        }

        yield return null;

    }

    private IEnumerator Die()
    {
        Debug.Log(gameObject.name + $" : Zombie Destroyed");
        animator.SetTrigger("Die");
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsWalking", false);
        yield return new WaitForSeconds(2.0f);
        gameObject.SetActive(false);
    }
}



//public enum EZombieState
//{
//    Patrol,
//    Chase,
//    Attack,
//    Evade,
//    Damage,
//    Idle,
//    Die

//}