using System;
using System.Collections;
using System.Security.Claims;
using UnityEngine;
using UnityEngine.Animations.Rigging;
//컨셉 : 룰렛 좀비 잡기
//1. 룰렛을 돌려서 나온 무기를 사용할수 있다.
//2. 무기를 다쓰면 다시 룰렛을 돌려야 한다. 
//3. 룰렛은 특정 조건을 만족해야 돌릴수 있다.

public class PlayerManager_Practice : MonoBehaviour
{
    // Movement
    public float walkSpeed = 5.0f;
    public float runSpeed = 20.0f;
    private float moveSpeed;
    public float mouseSensitivity = 250.0f;

    // camera
    public Transform cameraTransform;
    public Transform playerHead; // for frist-person view

    // camera zoom
    public float zoomDistance = 1.0f;
    public float zoomSpeed = 5.0f;
    public float defaultFov = 60.0f;
    public float zoomFov = 30.0f;
    private Camera mainCamera;
    private float currentDistance;
    private float targetDistance;
    private float targetFov;
    private Coroutine zoomCoroutine;

    // 1인칭 3인칭
    private bool isFirstPerson = true;

    // movement state tracking
    private float pitch = 0f;
    private float yaw = 0f;
    private bool isRotateAroundPlayer = false;
    private CharacterController characterController;
    private float horizontal;
    private float vertical;
    private bool isRunning = false;

    // Gravity
    public float gravity = -9.81f;
    private Vector3 velocity;
    private bool isGrounded;

    // Aim
    private bool isAim = false;

    // 무기 시스템 추가
    [Header("Weapon System")]
    public WeaponController weaponController;

    // Item
    GameObject nearObject;
    bool iDown;
    public bool[] hasWeapons;
    public GameObject[] weapons;
    GameObject equipWeapon;
    bool isSwap;

    // animation
    Animation anim;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = cameraTransform.GetComponent<Camera>();

        // 무기 컨트롤러 초기화
        if (weaponController != null)
        {
            weaponController.playerCamera = mainCamera;
        }

        // Lock cursor for FPS controls
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // handle mouse input for camera rotation
        MouseSet();

        // Toggle cameara modes
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    isRotateAroundPlayer = !isRotateAroundPlayer;
        //    Debug.Log(isRotateAroundPlayer ? "Cameara rotates around player" : "Player rotates with camera");
        //}

        Run();

        // apply movemnet based on camera mode
        FirstPersonMovement();
        AimSet();

        // iteam interaction
        GetInput();
        Interaction();

        ApplyGravity();

        // 무기 발사 로직 추가 - 무기 컨트롤러가 알아서 처리하도록 변경
        if (weaponController != null)
        {
            weaponController.SetAimStatus(isAim);
        }
    }

    private void MouseSet()
    {
        ////get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -60f, 60f); // 너무 위나 아래 보지 않게

        // 바닥에 있는지 확인
        isGrounded = characterController.isGrounded;

        // 바닥에 있을때 중력 초기화
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 땅에 닿으면 속도 초기화
        }
    }

    public void SetTargetDistance(float distance)
    {
        targetDistance = distance;
    }

    public void SetTargetFOV(float fov)
    {
        targetFov = fov;
    }

    // smoothly trasition camera distance for zooming
    IEnumerator ZoomCamera(float targetDistance)
    {
        while (Mathf.Abs(currentDistance - targetDistance) > 0.01f)
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        currentDistance = targetDistance;
        cameraTransform.localPosition = new Vector3(0, 0, -currentDistance);
    }

    IEnumerator ZoomFieldOfView(float targetFov)
    {
        while (Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        mainCamera.fieldOfView = targetFov;
    }

    void AimSet()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAim = true;

            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }

            if (isFirstPerson)
            {
                SetTargetFOV(zoomFov);
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            else
            {
                SetTargetDistance(zoomDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            isAim = false;

            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }

            if (isFirstPerson)
            {
                SetTargetFOV(defaultFov);
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            else
            {
                SetTargetDistance(0f);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }
    }

    void PlayerMovement()
    {
        FirstPersonMovement();
    }

    void FirstPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        // 플레이어 기준 이동방향 계산
        Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
        moveDirection.y = 0;

        // 입력이 없으면 이동하지 않음
        if (Mathf.Abs(horizontal) < 0.001f && Mathf.Abs(vertical) < 0.001f)
        {
            moveDirection = Vector3.zero; // 이동 멈춤
        }

        // 대각선 이동시 더 나가는것 방지
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        // 플레이어 머리에 카메라 배치
        cameraTransform.position = playerHead.position;
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0);

        // 카메라의 수평회전과 동일하게 플레이어 돌리기
        transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0);
    }

    void ApplyGravity()
    {
        //중력 적용
        velocity.y += gravity * Time.deltaTime; // 중력 가속도 적용
        characterController.Move(velocity * Time.deltaTime);
    }

    void Run()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        moveSpeed = isRunning ? runSpeed : walkSpeed;
    }

    public Vector3 GetMoveDirection()
    {
        Vector3 moveDirection = (cameraTransform.forward * vertical + cameraTransform.right * horizontal).normalized;
        moveDirection.y = 0; // Y축 제외
        return moveDirection;
    }

    void GetInput()
    {
        
        // 여기서 상호작용 키를 E로 설정
        iDown = Input.GetKeyDown(KeyCode.E);
    }

    // 무기 시스템 관련 메서드 추가
    public bool IsAiming()
    {
        return isAim;
    }


    // 룰렛 돌렸을때 나온 무기를 장착할수 있게 하는 로직
    void Swap(int weaponIndex) // weaponIndex를 받아와야 할듯
    {
        //int weaponIndex = -1
      
        if(equipWeapon != null)
        {
            equipWeapon.SetActive(false);
        }
        equipWeapon = weapons[weaponIndex];
        equipWeapon.SetActive(true);

        // 애니메이션 구현부분
        //anim.SetTrigger("doSwap");
        //isSwap = true;


    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Roulette"))
        {


            nearObject = other.gameObject;
       
            // 디버그 로그는 상호작용 시에만 표시하도록 Interaction() 메서드로 이동

        }

        else if (other.CompareTag("Weapon"))
        {
          
            nearObject = other.gameObject;
            // 무기 줍기
            // 디버그 로그는 상호작용 시에만 표시하도록 Interaction() 메서드로 이동

        }
    }

    void Interaction()
    {
        if (iDown && nearObject != null)
        {
            if (nearObject.CompareTag("Roulette"))
            {
                Item item = nearObject.GetComponent<Item>();
                if (item != null)
                {
                    int RouletteIndex = item.value;
                    hasWeapons[RouletteIndex] = true;
                    Debug.Log($"{nearObject.name} 획득! 값: {RouletteIndex}");

                    // 아이템 사라지게 하기
                    Destroy(nearObject);
                    nearObject = null;
                }
                else
                {
                    Debug.LogError("Item 컴포넌트가 없습니다!");
                }
            }
            else if (nearObject.CompareTag("Weapon"))
            {
                Item item = nearObject.GetComponent<Item>();
                if (item != null)
                {
                    int weaponIndex = item.value;
                    hasWeapons[weaponIndex] = true;
                    Debug.Log($"{nearObject.name} 획득! 값: {weaponIndex}");

                    // 아이템 사라지게 하기
                    Destroy(nearObject);
                    nearObject = null;
                }
                else
                {
                    Debug.LogError("Item 컴포넌트가 없습니다!");
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == nearObject)
        {
            nearObject = null;
            if (other.CompareTag("Roulette"))
            {
                Debug.Log("룰렛 벗어남");
            }
            else if (other.CompareTag("Weapon"))
            {
                Debug.Log("무기 벗어남");
            }
        }
    }
}