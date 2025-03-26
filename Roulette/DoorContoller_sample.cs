using UnityEngine;

public class DoorContoller_sample : MonoBehaviour
{
    public bool isOpen = false;
    private Animator animator;

    //���������� ���� �������� �����ִ����� ����
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
        //�÷��̾�� �������� ���͸� ���
        float dotProduct = Vector3.Dot(transform.forward, toPlayer);
        //���� ���ϴ� ����� �÷��̾��� ������ ��(��������)
        return dotProduct > 0; // dotProduct�� 0���� ũ�� �÷��̾ �� �տ� �ִ�.
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

