using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform doorPart; // �� �κ�(�ڽ� ������Ʈ)
    public float openDistance = 2.0f; // �÷��̾ ���� �� �� �ִ� �Ÿ�
    public float openSpeed = 5.0f; // �� ������ �ӵ�
    public float openAngle = 90f; // ������ ���� (90��)

    private Transform playerTransform; // �÷��̾� ��ġ ����
    private Quaternion closedRotation; // ���� ������ ȸ��
    private Quaternion targetRotation; // ���� ������ ��ǥ ȸ��
    private bool isOpening = false; // �� ���� ����
    private bool isOpen = false; // ���� ������ ���ȴ���

    private void Start()
    {
        closedRotation = doorPart.localRotation;
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

    }

    void Update()
    {
        // �÷��̾���� �Ÿ� ���
        float distanceToPlayer = Vector3.Distance(playerTransform.position, transform.position);

        if (distanceToPlayer <= openDistance && !isOpen)
        {
            OpenDoor();
        }
        else if (distanceToPlayer > openDistance && isOpen)
        {
            CloseDoor();
        }

        // �� ȸ�� ����
        if (isOpening)
        {
            doorPart.localRotation = Quaternion.Lerp(doorPart.localRotation, targetRotation, Time.deltaTime * openSpeed);
            // ��ǥ ȸ���� �����ߴ��� Ȯ��
            if (Quaternion.Angle(doorPart.localRotation, targetRotation) < 1f)
            {
                isOpening = false;
            }
        }
    }

    void OpenDoor()
    {
        if (!isOpening && !isOpen)
        {
            // �÷��̾��� �̵� ���� ���
            Vector3 playerMoveDirection = playerTransform.GetComponent<PlayerManager_Practice>().GetMoveDirection();
            Vector3 flatDirection = playerMoveDirection; // Y�� ����
            flatDirection.y = 0;
            flatDirection = flatDirection.normalized;

            // ���� �÷��̾��� ����� ���� ���
            Vector3 doorToPlayer = (playerTransform.position - transform.position).normalized;
            float dot = Vector3.Dot(flatDirection, doorToPlayer);

            // �÷��̾ ���� ���� ������, �־��������� ���� ȸ�� ���� ����
            float angle = (dot > 0) ? openAngle : -openAngle;

            // ��ǥ ȸ�� ���� (Y�� ����)
            targetRotation = closedRotation * Quaternion.Euler(0, angle, 0);
            isOpening = true;
            isOpen = true;
        }
    }

    void CloseDoor()
    {
        if (!isOpening && isOpen)
        {
            targetRotation = closedRotation; // ���� ���·� ����
            isOpening = true;
            isOpen = false;
        }
    }
}

