using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ZombieManager : MonoBehaviour
{
    // enum으로 상태 정의 및 관리
    public EZombieState currentState;
    public Transform target;
    public float attacRange = 1.0f;
    public float attackDelay = 2.0f;
    //private float nextAttackTime = 1.0f;
    public Transform[] patrolPoints;// 순찰 경로지점
    public float moveSpeed = 2.0f;
    private int currentPoint = 0; // 현재 순찰 경로지점 인덱스
    private float trackingRange = 3.0f; // 추적 범위 설정
    private bool isAttack = false; // 공격상태
    private float evadeRange = 5.0f; // 도망 상태 회피거리
    private float zombieHP = 10.0f;
    private float distanceTotarget; // Target과의 거리 계산값
    private bool isWaiting = false;
    public float idleTime = 2.0f; // 각 상태 전환후 대기 시간
    private Coroutine stateRoutine;

    private Animator animator;
    private CharacterController characterController;
    private float lastLogTime = 0f; // 마지막으로 로그를 출력한 시간

    private float gravity = -9.81f; // 중력 값
    private Vector3 verticalVelocity;

    private void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        // 초기 상태 설정
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
            // 1초마다 거리 출력
            if (Time.time - lastLogTime >= 1f)
            {
                Debug.Log("좀비와 플레이어간의 거리 : " + distanceTotarget);
                lastLogTime = Time.time; // 마지막 로그 출력 시간 갱신
            }
        }

        //if (zombieHP <= 0)
        //{
        //    ChangeState(EZombieState.Die);
        //    return; //return이 좋긴한데 좀더 죽음에 대한 함수가 필요하다?
        //}



   


        //// 상태 머신
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

        Debug.Log(gameObject.name + " : 순찰중..");

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

        Debug.Log(gameObject.name + " : 좀비 추적중");





        while (currentState == EZombieState.Chase)
        {

            float distance = Vector3.Distance(transform.position, target.position);
            // 타겟을 향해 이동
            //transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(target.position);
            // 타겟을 바라봄

            animator.SetBool("IsRunning", true);

  
            if (distance < attacRange)
            {
                ChangeState(EZombieState.Attack);
            }
            if(distance > trackingRange * 1.5f)
            {
                ChangeState(EZombieState.Patrol);
                yield break;
                //기존 코루틴을 즉시 종료 충돌 방지
            }



            yield return null;
        }
      
    }

    private IEnumerator Attack()
    {


     
        animator.SetTrigger("Attack");
        Debug.Log(gameObject.name + " : 좀비가 공격해온다");
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
        Debug.Log(gameObject.name + " : 좀비 회피중  ");
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
        Debug.Log(gameObject.name + " : 좀비 피격당함");
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
        // 반복되는 케이스가 있다면 일단 코루틴을써서 중지시켜줘
        if (stateRoutine != null)
        {

            StopCoroutine(stateRoutine);    
        }

        animator.SetBool("IsRunning", false); // 추가
        animator.SetBool("IsWalking", false); // 추가

        // 새로 들어온 상태를 현재상태로 바꿔준후
        currentState = newstate;

        // 스위치문을 통해 상태마다 코루틴 호출, 코루틴이 뭐냐 그래서?
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
        Debug.Log(gameObject.name + " : 대기중");
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
        Debug.Log(gameObject.name + $" {damage} : 데미지 받음");
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