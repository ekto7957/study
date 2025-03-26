using System;
using System.Collections;
using System.Security.Claims;
using UnityEngine;
using UnityEngine.Animations.Rigging;
//���� : �귿 ���� ���
//1. �귿�� ������ ���� ���⸦ ����Ҽ� �ִ�.
//2. ���⸦ �پ��� �ٽ� �귿�� ������ �Ѵ�. 
//3. �귿�� Ư�� ������ �����ؾ� ������ �ִ�.

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

    // 1��Ī 3��Ī
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

    // ���� �ý��� �߰�
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

        // ���� ��Ʈ�ѷ� �ʱ�ȭ
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

        // ���� �߻� ���� �߰� - ���� ��Ʈ�ѷ��� �˾Ƽ� ó���ϵ��� ����
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
        pitch = Mathf.Clamp(pitch, -60f, 60f); // �ʹ� ���� �Ʒ� ���� �ʰ�

        // �ٴڿ� �ִ��� Ȯ��
        isGrounded = characterController.isGrounded;

        // �ٴڿ� ������ �߷� �ʱ�ȭ
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // ���� ������ �ӵ� �ʱ�ȭ
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

        // �÷��̾� ���� �̵����� ���
        Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
        moveDirection.y = 0;

        // �Է��� ������ �̵����� ����
        if (Mathf.Abs(horizontal) < 0.001f && Mathf.Abs(vertical) < 0.001f)
        {
            moveDirection = Vector3.zero; // �̵� ����
        }

        // �밢�� �̵��� �� �����°� ����
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        // �÷��̾� �Ӹ��� ī�޶� ��ġ
        cameraTransform.position = playerHead.position;
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0);

        // ī�޶��� ����ȸ���� �����ϰ� �÷��̾� ������
        transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0);
    }

    void ApplyGravity()
    {
        //�߷� ����
        velocity.y += gravity * Time.deltaTime; // �߷� ���ӵ� ����
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
        moveDirection.y = 0; // Y�� ����
        return moveDirection;
    }

    void GetInput()
    {
        
        // ���⼭ ��ȣ�ۿ� Ű�� E�� ����
        iDown = Input.GetKeyDown(KeyCode.E);
    }

    // ���� �ý��� ���� �޼��� �߰�
    public bool IsAiming()
    {
        return isAim;
    }


    // �귿 �������� ���� ���⸦ �����Ҽ� �ְ� �ϴ� ����
    void Swap(int weaponIndex) // weaponIndex�� �޾ƿ;� �ҵ�
    {
        //int weaponIndex = -1
      
        if(equipWeapon != null)
        {
            equipWeapon.SetActive(false);
        }
        equipWeapon = weapons[weaponIndex];
        equipWeapon.SetActive(true);

        // �ִϸ��̼� �����κ�
        //anim.SetTrigger("doSwap");
        //isSwap = true;


    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Roulette"))
        {


            nearObject = other.gameObject;
       
            // ����� �α״� ��ȣ�ۿ� �ÿ��� ǥ���ϵ��� Interaction() �޼���� �̵�

        }

        else if (other.CompareTag("Weapon"))
        {
          
            nearObject = other.gameObject;
            // ���� �ݱ�
            // ����� �α״� ��ȣ�ۿ� �ÿ��� ǥ���ϵ��� Interaction() �޼���� �̵�

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
                    Debug.Log($"{nearObject.name} ȹ��! ��: {RouletteIndex}");

                    // ������ ������� �ϱ�
                    Destroy(nearObject);
                    nearObject = null;
                }
                else
                {
                    Debug.LogError("Item ������Ʈ�� �����ϴ�!");
                }
            }
            else if (nearObject.CompareTag("Weapon"))
            {
                Item item = nearObject.GetComponent<Item>();
                if (item != null)
                {
                    int weaponIndex = item.value;
                    hasWeapons[weaponIndex] = true;
                    Debug.Log($"{nearObject.name} ȹ��! ��: {weaponIndex}");

                    // ������ ������� �ϱ�
                    Destroy(nearObject);
                    nearObject = null;
                }
                else
                {
                    Debug.LogError("Item ������Ʈ�� �����ϴ�!");
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
                Debug.Log("�귿 ���");
            }
            else if (other.CompareTag("Weapon"))
            {
                Debug.Log("���� ���");
            }
        }
    }
}