using UnityEngine;

public class DoorContoller_sample : MonoBehaviour
{
    public bool isOpen = false;
    private Animator animator;

    //마지막으로 문이 정방향을 열려있는지를 추적
    public bool LastOpendForward { get; private set; } = true;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsPlayerInfront(Transform player)
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer = toPlayer.normalized;
        //플레이어와 문사이의 벡터를 계산
        float dotProduct = Vector3.Dot(transform.forward, toPlayer);
        //문이 향하는 방향과 플레이어의 방향을 비교(내적연산)
        return dotProduct > 0; // dotProduct가 0보다 크면 플레이어가 문 앞에 있다.
    }

    public bool Open(Transform player)
    {
        if (!isOpen)
        {

            isOpen = true;
            if (IsPlayerInfront(player))
            {
                animator.SetTrigger("OpenForward");
                LastOpendForward = true;
                
            }
            else
            {
                animator.SetTrigger("OpenBackward");
                LastOpendForward = false;
                
            }
            return true;
        }
        return false;
    }

    public void CloseForward(Transform player)
    {
        if (isOpen)
        {
            if (isOpen)
            {
                isOpen = false;
                animator.SetTrigger("CloseBackward");
            }
        }
    }

    public void CloseBackward()
    {
        if (isOpen)
        {
            isOpen = false;
            animator.SetTrigger("CloseForward");
        }
    }
}

