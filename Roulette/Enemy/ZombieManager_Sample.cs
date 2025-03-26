using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class ZombieManager_Sample : MonoBehaviour
{
    public EZombieState currentState; // 현재 좀비의 상태
    public Transform target; // 플레이어(타겟) 정보
    public float attackRange = 1.0f; // 공격 가능 범위
    public float attackDelay = 2.0f; // 공격 딜레이 시간
    private float nextAttackTime = 0.0f; // 다음 공격 시간 관리

    public Transform[] patrolPoints; // 순찰 경로 지점들
    private int currentPoint = 0; // 현재 이동 중인 순찰 경로의 인덱스
    public float moveSpeed = 2.0f; // 이동 속도
    private float trackingRange = 4.0f; // 플레이어를 감지하여 추적하는 범위
    private float evadeRange = 5.0f; // 도망 상태가 활성화되는 거리

    private float zombieHp = 100.0f; // 좀비 체력
    private float distanceToTarget; // 타겟과의 거리 측정
    private Coroutine stateRoutine; // 현재 실행 중인 상태 관리용 코루틴

    private Animator animator; // 좀비의 애니메이션을 관리하는 Animator

    private NavMeshAgent agent;

    private AudioSource audioSource;
    public AudioClip ZombieAttackAudioClip;

    private bool isJumping = false;
    private Rigidbody rb;
    public float jumpHeight = 2.0f;
    public float jumpDuration = 1.0f;
    private NavMeshLinkData[] navMeshLinks;
    public PlayerManager_Sample PlayerManager_Sample;

    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        ChangeState(currentState); // 초기 상태 실행
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        if( rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;

        //navMeshLinks = FindObjectsByType<NavMeshLink>();
    }

    void Update()
    {
        // 플레이어와의 거리 측정
        distanceToTarget = Vector3.Distance(transform.position, PlayerManager_Sample.Instance.gameObject.transform.position);
    }

    // 좀비의 상태를 변경하는 함수
    // 기존 실행 중인 코루틴을 중지한 후, 새로운 상태 코루틴을 실행
    public void ChangeState(EZombieState newState)
    {
        // isJumpong logic

        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
            stateRoutine = null; // 기존 코루틴 정리
        }

        currentState = newState; // 새로운 상태 저장

        switch (currentState)
        {
            case EZombieState.Idle:
                stateRoutine = StartCoroutine(Idle());
                break;
            case EZombieState.Patrol:
                stateRoutine = StartCoroutine(Patrol());
                break;
            case EZombieState.Chase:
                stateRoutine = StartCoroutine(Chase());
                break;
            case EZombieState.Attack:
                stateRoutine = StartCoroutine(Attack());
                break;
            case EZombieState.Evade:
                stateRoutine = StartCoroutine(Evade());
                break;
            case EZombieState.Die:
                stateRoutine = StartCoroutine(Die());
                break;
        }
    }

    // 대기 상태 (Idle)
    private IEnumerator Idle()
    {
        Debug.Log(gameObject.name + " : 대기중");
        animator.SetBool("IsWalk", false); // 이동 애니메이션 중지

        while (currentState == EZombieState.Idle)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            // 플레이어 감지 시 추적 상태로 변경
            if (distance < trackingRange)
            {
                ChangeState(EZombieState.Chase);
            }
            // 플레이어가 공격 범위 내에 있을 경우 공격 상태로 변경
            else if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }

            yield return null;
        }
    }

    // 순찰 상태 (Patrol)
    private IEnumerator Patrol()
    {
        Debug.Log(gameObject.name + " : 순찰중");

        while (currentState == EZombieState.Patrol)
        {
            if (patrolPoints.Length > 0)
            {
                animator.SetBool("IsWalk", true);
                Transform targetPoint = patrolPoints[currentPoint];
                Vector3 direction = (targetPoint.position - transform.position).normalized;
                agent.speed = moveSpeed;
                agent.isStopped = false;
                agent.destination = target.position;
                //transform.position += direction * moveSpeed * Time.deltaTime;
                //transform.LookAt(targetPoint.transform);

                // 위치는 내꺼에 맞게 바꿔야함

                if (agent.isOnOffMeshLink)
                {
                    StartCoroutine(JumpAcrrossLink());
                }

                if (Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
                }

                // 플레이어 감지 시 추적 상태로 변경
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < trackingRange)
                {
                    ChangeState(EZombieState.Chase);
                }
            }
            yield return null;
        }
    }

    // 추적 상태 (Chase)
    private IEnumerator Chase()
    {
        Debug.Log(gameObject.name + " : 플레이어 추적중");

        while (currentState == EZombieState.Chase)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            // 플레이어에게 이동
            Vector3 direction = (target.position - transform.position).normalized;
            agent.speed = moveSpeed;
            agent.isStopped = false;
            agent.destination = target.position;

            //transform.position += direction * moveSpeed * Time.deltaTime;
            //transform.LookAt(target.position);
            animator.SetBool("IsWalk", true);

            // 공격 범위 내에 도달하면 공격 상태로 변경
            if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }

            // 플레이어가 너무 멀어지면 순찰 상태로 변경
            if (distance > trackingRange * 1.5f)
            {
                ChangeState(EZombieState.Patrol);
            }
            yield return null;
        }
    }
    // 공격 상태 (Attack)
    private IEnumerator Attack()
    {
        Debug.Log(gameObject.name + " : 공격중");
        audioSource.PlayOneShot(ZombieAttackAudioClip);
        //agent.speed = moveSpeed;
        agent.isStopped = true;
        agent.destination = target.position;
        //transform.LookAt(target.position);
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDelay);

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > attackRange)
        {
            ChangeState(EZombieState.Chase);
        }
    }

    // 도망 상태 (Evade)
    private IEnumerator Evade()
    {
        Debug.Log(gameObject.name + " : 도망중");
        animator.SetBool("IsWalk", true);

        Vector3 evadeDirection = (transform.position - target.position).normalized;
        float evadeTime = 3.0f;
        float timer = 0.0f;

        Quaternion targetRotation = Quaternion.LookRotation(evadeDirection);
        transform.rotation = targetRotation;

        while (currentState == EZombieState.Evade && timer < evadeTime)
        {
            //transform.position += evadeDirection * moveSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        // 도망 후 다시 순찰 상태로 이동
        ChangeState(EZombieState.Patrol);
        yield break;
    }

    // 체력을 감소시키는 함수
    public void TakeDamage(float damage)
    {
        Debug.Log(gameObject.name + " : " + damage + " 데미지 받음");
        animator.SetTrigger("Damage");
        zombieHp -= damage;
        agent.isStopped = true;

        // 체력이 0 이하가 되면 사망 상태로 변경
        if (zombieHp <= 0)
        {
            ChangeState(EZombieState.Die);
        }
        else
        {
            ChangeState(EZombieState.Chase);
        }
    }

    // 사망 상태 (Die)
    private IEnumerator Die()
    {
        Debug.Log(gameObject.name + " : 사망");
        animator.SetTrigger("Die");
        yield return new WaitForSeconds(2.0f);
        gameObject.SetActive(false);
    }

    private IEnumerator JumpAcrrossLink()
    {
        Debug.Log(gameObject.name + "좀비 점프");

        isJumping = true;

        agent.isStopped = true; // 에이전트 멈춤

        float elapsedTime = 0;

        // NavMeshLink의 시작과 끝 좌표 가져오기
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = linkData.startPos;
        Vector3 endPos = linkData.endPos;


        // 점프 경로 계산(포물선 그리며 점프)
        while (elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            Vector3 currentPostion = Vector3.Lerp(startPos, endPos, t);
            currentPostion.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;
            transform.position = currentPostion;

            elapsedTime += Time.deltaTime;
            yield return null;

        }

        //도착점에 위치
        transform.position = endPos;
        // NavmeshAgent 경로재개
        agent.CompleteOffMeshLink();
        agent.isStopped = false;
        isJumping = false;
    
    }
}

// 좀비 상태 정의
public enum EZombieState
{
    Patrol,  // 순찰 상태
    Chase,   // 추적 상태
    Attack,  // 공격 상태
    Evade,   // 도망 상태
    Idle,    // 대기 상태
    Die,     // 사망 상태
    Damage
}