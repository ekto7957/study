using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform doorPart; // 문 부분(자식 오브젝트)
    public float openDistance = 2.0f; // 플레이어가 문을 열 수 있는 거리
    public float openSpeed = 5.0f; // 문 열리는 속도
    public float openAngle = 90f; // 열리는 각도 (90도)

    private Transform playerTransform; // 플레이어 위치 참조
    private Quaternion closedRotation; // 닫힌 상태의 회전
    private Quaternion targetRotation; // 열린 상태의 목표 회전
    private bool isOpening = false; // 문 열림 상태
    private bool isOpen = false; // 문이 완전히 열렸는지

    private void Start()
    {
        closedRotation = doorPart.localRotation;
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

    }

    void Update()
    {
        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(playerTransform.position, transform.position);

        if (distanceToPlayer <= openDistance && !isOpen)
        {
            OpenDoor();
        }
        else if (distanceToPlayer > openDistance && isOpen)
        {
            CloseDoor();
        }

        // 문 회전 보간
        if (isOpening)
        {
            doorPart.localRotation = Quaternion.Lerp(doorPart.localRotation, targetRotation, Time.deltaTime * openSpeed);
            // 목표 회전에 도달했는지 확인
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
            // 플레이어의 이동 방향 계산
            Vector3 playerMoveDirection = playerTransform.GetComponent<PlayerManager_Practice>().GetMoveDirection();
            Vector3 flatDirection = playerMoveDirection; // Y축 제외
            flatDirection.y = 0;
            flatDirection = flatDirection.normalized;

            // 문과 플레이어의 상대적 방향 계산
            Vector3 doorToPlayer = (playerTransform.position - transform.position).normalized;
            float dot = Vector3.Dot(flatDirection, doorToPlayer);

            // 플레이어가 문을 향해 오는지, 멀어지는지에 따라 회전 방향 결정
            float angle = (dot > 0) ? openAngle : -openAngle;

            // 목표 회전 설정 (Y축 기준)
            targetRotation = closedRotation * Quaternion.Euler(0, angle, 0);
            isOpening = true;
            isOpen = true;
        }
    }

    void CloseDoor()
    {
        if (!isOpening && isOpen)
        {
            targetRotation = closedRotation; // 닫힌 상태로 복귀
            isOpening = true;
            isOpen = false;
        }
    }
}

