using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class ZombieManager_Sample : MonoBehaviour
{
    public EZombieState currentState; // ���� ������ ����
    public Transform target; // �÷��̾�(Ÿ��) ����
    public float attackRange = 1.0f; // ���� ���� ����
    public float attackDelay = 2.0f; // ���� ������ �ð�
    private float nextAttackTime = 0.0f; // ���� ���� �ð� ����

    public Transform[] patrolPoints; // ���� ��� ������
    private int currentPoint = 0; // ���� �̵� ���� ���� ����� �ε���
    public float moveSpeed = 2.0f; // �̵� �ӵ�
    private float trackingRange = 4.0f; // �÷��̾ �����Ͽ� �����ϴ� ����
    private float evadeRange = 5.0f; // ���� ���°� Ȱ��ȭ�Ǵ� �Ÿ�

    private float zombieHp = 100.0f; // ���� ü��
    private float distanceToTarget; // Ÿ�ٰ��� �Ÿ� ����
    private Coroutine stateRoutine; // ���� ���� ���� ���� ������ �ڷ�ƾ

    private Animator animator; // ������ �ִϸ��̼��� �����ϴ� Animator

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
        ChangeState(currentState); // �ʱ� ���� ����
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
        // �÷��̾���� �Ÿ� ����
        distanceToTarget = Vector3.Distance(transform.position, PlayerManager_Sample.Instance.gameObject.transform.position);
    }

    // ������ ���¸� �����ϴ� �Լ�
    // ���� ���� ���� �ڷ�ƾ�� ������ ��, ���ο� ���� �ڷ�ƾ�� ����
    public void ChangeState(EZombieState newState)
    {
        // isJumpong logic

        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
            stateRoutine = null; // ���� �ڷ�ƾ ����
        }

        currentState = newState; // ���ο� ���� ����

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

    // ��� ���� (Idle)
    private IEnumerator Idle()
    {
        Debug.Log(gameObject.name + " : �����");
        animator.SetBool("IsWalk", false); // �̵� �ִϸ��̼� ����

        while (currentState == EZombieState.Idle)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            // �÷��̾� ���� �� ���� ���·� ����
            if (distance < trackingRange)
            {
                ChangeState(EZombieState.Chase);
            }
            // �÷��̾ ���� ���� ���� ���� ��� ���� ���·� ����
            else if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }

            yield return null;
        }
    }

    // ���� ���� (Patrol)
    private IEnumerator Patrol()
    {
        Debug.Log(gameObject.name + " : ������");

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

                // ��ġ�� ������ �°� �ٲ����

                if (agent.isOnOffMeshLink)
                {
                    StartCoroutine(JumpAcrrossLink());
                }

                if (Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
                }

                // �÷��̾� ���� �� ���� ���·� ����
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < trackingRange)
                {
                    ChangeState(EZombieState.Chase);
                }
            }
            yield return null;
        }
    }

    // ���� ���� (Chase)
    private IEnumerator Chase()
    {
        Debug.Log(gameObject.name + " : �÷��̾� ������");

        while (currentState == EZombieState.Chase)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            // �÷��̾�� �̵�
            Vector3 direction = (target.position - transform.position).normalized;
            agent.speed = moveSpeed;
            agent.isStopped = false;
            agent.destination = target.position;

            //transform.position += direction * moveSpeed * Time.deltaTime;
            //transform.LookAt(target.position);
            animator.SetBool("IsWalk", true);

            // ���� ���� ���� �����ϸ� ���� ���·� ����
            if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }

            // �÷��̾ �ʹ� �־����� ���� ���·� ����
            if (distance > trackingRange * 1.5f)
            {
                ChangeState(EZombieState.Patrol);
            }
            yield return null;
        }
    }
    // ���� ���� (Attack)
    private IEnumerator Attack()
    {
        Debug.Log(gameObject.name + " : ������");
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

    // ���� ���� (Evade)
    private IEnumerator Evade()
    {
        Debug.Log(gameObject.name + " : ������");
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

        // ���� �� �ٽ� ���� ���·� �̵�
        ChangeState(EZombieState.Patrol);
        yield break;
    }

    // ü���� ���ҽ�Ű�� �Լ�
    public void TakeDamage(float damage)
    {
        Debug.Log(gameObject.name + " : " + damage + " ������ ����");
        animator.SetTrigger("Damage");
        zombieHp -= damage;
        agent.isStopped = true;

        // ü���� 0 ���ϰ� �Ǹ� ��� ���·� ����
        if (zombieHp <= 0)
        {
            ChangeState(EZombieState.Die);
        }
        else
        {
            ChangeState(EZombieState.Chase);
        }
    }

    // ��� ���� (Die)
    private IEnumerator Die()
    {
        Debug.Log(gameObject.name + " : ���");
        animator.SetTrigger("Die");
        yield return new WaitForSeconds(2.0f);
        gameObject.SetActive(false);
    }

    private IEnumerator JumpAcrrossLink()
    {
        Debug.Log(gameObject.name + "���� ����");

        isJumping = true;

        agent.isStopped = true; // ������Ʈ ����

        float elapsedTime = 0;

        // NavMeshLink�� ���۰� �� ��ǥ ��������
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = linkData.startPos;
        Vector3 endPos = linkData.endPos;


        // ���� ��� ���(������ �׸��� ����)
        while (elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            Vector3 currentPostion = Vector3.Lerp(startPos, endPos, t);
            currentPostion.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;
            transform.position = currentPostion;

            elapsedTime += Time.deltaTime;
            yield return null;

        }

        //�������� ��ġ
        transform.position = endPos;
        // NavmeshAgent ����簳
        agent.CompleteOffMeshLink();
        agent.isStopped = false;
        isJumping = false;
    
    }
}

// ���� ���� ����
public enum EZombieState
{
    Patrol,  // ���� ����
    Chase,   // ���� ����
    Attack,  // ���� ����
    Evade,   // ���� ����
    Idle,    // ��� ����
    Die,     // ��� ����
    Damage
}