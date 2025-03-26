using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;   



public class PlayerManager : MonoBehaviour
{
    private float moveSpeed = 5.0f; //플레이어 이동 속도
    public float mouseSensitivity = 100.0f; // 마우스 감도
    public Transform cameraTransform; // 카메라의 Transform
    public CharacterController characterController;
    public Transform playerHead; //플레이어 머리 위치(1인칭 모드를 위해서)
    public float thirdPersonDistance = 3.0f; //3인칭 모드에서 플레이어와 카메라의 거리
    public Vector3 thirdPersonOffset = new Vector3(0f, 1.5f, 0f); //3인칭 모드에서 카메라 오프셋
    public Transform playerLookObj; //플레이어 시야 위치

    public float zoomeDistance = 1.0f; //카메라가 확대될 때의 거리(3인칭 모드에서 사용)
    public float zoomSpeed = 5.0f; // 확대축소가 되는 속도
    public float defaultFov = 60.0f; //기본 카메라 시야각
    public float zoomeFov = 30.0f; //확대 시 카메라 시야각(1인칭 모드에서 사용)

    private float currentDistance; //현재 카메라와의 거리(3인칭 모드)
    private float targetDistance; //목표 카메라 거리
    private float targetFov; //목표 FOV
    //private bool isZoomed = false; //확대 여부 확인
    private Coroutine zoomCoroutine; //코루틴을 사용하여 확대 축소 처리
    private Camera mainCamera; //카메라 컴포넌트

    private float pitch = 0.0f; //위아래 회전 값
    private float yaw = 0.0f; //좌우 회전 값
    private bool isFirstPerson = false; //1인칭 모드 여부
    private bool isRotaterAroundPlayer = false; //카메라가 플레이어 주위를 회전하는지 여부 

    //중력 관련 변수
    public float gravity = -9.81f;
    public float jumpHeight = 2.0f;
    private Vector3 velocity;
    private bool isGround;

    private Animator animator;
    private float horizontal;
    private float vertical;
    private bool isRunning = false;
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;
    private bool isAim = false;
    private bool isFire = false;
    private bool isOperate = false;

    public AudioClip audioClipFire;
    public AudioClip audioClipItemGet;
    private AudioSource audioSource;
    public AudioClip audioClipWeaponChange;
    public GameObject RifleM4Obj;
    private int animationSpeed = 1;
    private string currentAnimation;

    public Transform aimTarget;

    private float weaponMaxDistance = 100.0f;
    public LayerMask TargetLayerMask;

    public MultiAimConstraint multiAimConstraint;


    public Vector3 boxSize = new Vector3(1.0f, 1.0f, 1.0f);
    public float castDistance = 5.0f;
    public LayerMask itemLayer;
    public Transform itemGetPos;

    public GameObject crosshairObj;
    public GameObject m4IconImage;

    bool isUseWeapon = false;
    bool isGetM4Item = false;

    public ParticleSystem m4Effect;

    private float rifleFireDelay = 0.5f;
    private float lastFireTime = 0f;
    private float fireRate = 0.5f; // 발사 간격

    public ParticleSystem DamageParticleSystem;
    public AudioClip audioClipDamage;
    public AudioClip audioClipFlashLightOn;

    public Text bulletText;
    private int firebulletCount = 30;
    private int savebulletCount = 0;

    public GameObject flashLightObj;
    private bool isFlashLightOn = false;
    private int playerHp = 100;

    public GameObject PauseObj;
    private bool isPause = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = thirdPersonDistance;
        targetDistance = thirdPersonDistance;
        targetFov = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        RifleM4Obj.SetActive(false);
        crosshairObj.SetActive(false);
        m4IconImage.SetActive(false);
    }

    void Update()
    {
        MouseSet();
        CameraSet();
        PlayerMovement();
        AimSet();
        WeaponFire();
        Run();
        WeaponChange();
        AnimationSet();
        Operate();

        animator.speed = animationSpeed;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName(currentAnimation) && stateInfo.normalizedTime >= 1.0f)
        {
            currentAnimation = "Idle"; 
            animator.Play(currentAnimation);
        }
    }


    void UpdateAimTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        aimTarget.position = ray.GetPoint(10.0f);
    }

    void Operate()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (Input.GetKeyDown(KeyCode.E) && !stateInfo.IsName("PickUp"))
        {
            animator.SetTrigger("Operate");
            ItemBoxCast();
        }
    }

    public void ItemBoxCast()
    {
        Vector3 origin = itemGetPos.position;
        Vector3 direction = itemGetPos.forward;
        RaycastHit[] hits;
        hits = Physics.BoxCastAll(origin, boxSize / 2, direction, Quaternion.identity, castDistance, itemLayer);
        DebugBox(origin, direction);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.name == "ItemM4")
            {
                Debug.Log("아이템 줍기");
                hit.collider.gameObject.SetActive(false);
                audioSource.PlayOneShot(audioClipItemGet);
                m4IconImage.SetActive(true);
                isGetM4Item = true;
            }

        }
    }

    void MouseSet()
    {
        //마우스 입력을 받아 카메라와 플레이어 회전 처리
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -45f, 45f);

        isGround = characterController.isGrounded;

        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void CameraSet()
    {

        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1인칭 모드" : "3인칭 모드");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            isRotaterAroundPlayer = !isRotaterAroundPlayer;
            Debug.Log(isRotaterAroundPlayer ? "카메라가 주위를 회전합니다." : "플레이어가 시야에 따라서 회전합니다.");
        }
    }

    void PlayerMovement()
    {
        if (isFirstPerson)
        {
            FirstPersonMovement();
        }
        else
        {
            ThirdPersonMovement();
        }
    }

    void WeaponChange()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && isGetM4Item)
        {
            animator.SetTrigger("IsWeaponChange");
            RifleM4Obj.SetActive(true);
            isUseWeapon = true;
        }
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

    void WeaponFire()
    {
        if (Input.GetMouseButton(0) && isAim && Time.time - lastFireTime >= fireRate)
        {

           
            //Weapon Type MaxDistance Set
            weaponMaxDistance = 1000.0f;
            lastFireTime = Time.time;
            //isFire = true;
            //StartCoroutine(FireWithDelay(0.5f));
            animator.SetTrigger("Fire");

            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            RaycastHit[] hits = Physics.RaycastAll(ray, weaponMaxDistance, TargetLayerMask);

            if (hits.Length > 0)
            {
                for (int i = 0; i < hits.Length && i < 2; i++)
                {
                    Debug.Log("충돌 : " + hits[i].collider.name);
                    hits[i].collider.GetComponent<ZombieManager>().TakeDamage(30.0f);
                    Debug.DrawLine(ray.origin, hits[i].point, Color.red, 3.0f);
                }

            }
            else
            {
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 3.0f);
            }

            StartCoroutine(FireWithDelay(fireRate));
            
        }


        //if (Input.GetMouseButtonUp(0))
        //{
        //    isFire = false;
        //}

        // 코루틴 써서 다시 isFire가 false가 될수 있게 수정

    }

    void AimSet()
    {
        if (Input.GetMouseButtonDown(1) && isGetM4Item && isUseWeapon)
        {
            isAim = true;
            multiAimConstraint.data.offset = new Vector3(-30, 0, 0);
            crosshairObj.SetActive(true);
            //animator.SetBool("IsAim", isAim);
            animator.SetLayerWeight(1, 1);

            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }

            if (isFirstPerson)
            {
                SetTargetFOV(zoomeFov);
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            else
            {
                SetTargetDistance(zoomeDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }

        if (Input.GetMouseButtonUp(1) && isGetM4Item && isUseWeapon)
        {
            isAim = false;
            crosshairObj.SetActive(false);
            multiAimConstraint.data.offset = new Vector3(0, 0, 0);
            //animator.SetBool("IsAim", isAim);
            animator.SetLayerWeight(1, 0);
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
                SetTargetDistance(thirdPersonDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }
    }

    void AnimationSet()
    {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetBool("isRunning", isRunning);
    }

    void FirstPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
        moveDirection.y = 0;
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        cameraTransform.position = playerHead.position;
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0);
        transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0);
    }

    void ThirdPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        UpdateCameraPosition();
    }


    void UpdateCameraPosition()
    {
        if (isRotaterAroundPlayer)
        {
            //카메라가 플레이어 오른쪽에서 회전하도록 설정
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            //카메라를 플레이어의 오른쪽에서 고정된 위치로 이동
            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            //카메라가 플레이어의 위치를 따라가도록 설정
            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else
        {
            //플레이어가 직접 회전하는 모드
            transform.rotation = Quaternion.Euler(0f, yaw, 0);
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            cameraTransform.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
            cameraTransform.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));

            UpdateAimTarget();
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

    IEnumerator ZoomCamera(float targetDistance)
    {
        while (Mathf.Abs(currentDistance - targetDistance) > 0.01f) //현재 거리에서 목표 거리로 부드럽게 이동
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        currentDistance = targetDistance; // 목표 거리에 도달한 후 값을 고정
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

    IEnumerator FireWithDelay(float delay) 
    {
        yield return new WaitForSeconds(delay); // 지정시간만큼 대기
        isFire = false;
    }

    public void WeaponChangeSoundOn()
    {
        audioSource.PlayOneShot(audioClipWeaponChange);
    }

    public void WeaponFireSoundOn()
    {
        audioSource.PlayOneShot(audioClipFire);
    }

    public void MovementSoundOn()
    {
        animationSpeed = 1;
    }

    //예시
    public void FootStepSoundon()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10.0f))
        {

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerDamage"))
        {
            animator.SetTrigger("Damage");
            audioSource.PlayOneShot(audioClipDamage);
            GetComponent<CharacterController>().enabled = false;
            transform.position = Vector3.zero;
            GetComponent<CharacterController>().enabled = true;
            playerHp -= 30;
        }
        else if (other.CompareTag("Attack"))
        {
            animator.SetTrigger("Damage");
            audioSource.PlayOneShot(audioClipDamage);
            playerHp -= 30;
        }
        else if (other.CompareTag("Item"))
        {
            other.gameObject.transform.SetParent(null);

        }


    }

    void DebugBox(Vector3 origin, Vector3 direction)
    {
        Vector3 endPoint = origin + direction * castDistance;

        Vector3[] corners = new Vector3[8];
        corners[0] = origin + new Vector3(-boxSize.x, - boxSize.y, -boxSize.z) / 2;
        corners[1] = origin + new Vector3(boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[2] = origin + new Vector3(-boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[3] = origin + new Vector3(boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[4] = origin + new Vector3(-boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[5] = origin + new Vector3(boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[6] = origin + new Vector3(-boxSize.x, boxSize.y, boxSize.z) / 2;
        corners[7] = origin + new Vector3(boxSize.x, boxSize.y, boxSize.z) / 2;

        // 아래 면의 4개 모서리
        Debug.DrawLine(corners[0], corners[1], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[5], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[4], corners[0], Color.green, 3.0f);

        // 윗 면의 4개 모서리
        Debug.DrawLine(corners[2], corners[3], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[7], Color.green, 3.0f);
        Debug.DrawLine(corners[7], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[6], corners[2], Color.green, 3.0f);

        // 옆면을 연결하는 4개의 수직선
        Debug.DrawLine(corners[0], corners[2], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[3], Color.green, 3.0f);
        Debug.DrawLine(corners[5], corners[7], Color.green, 3.0f);
        Debug.DrawLine(corners[4], corners[6], Color.green, 3.0f);


        Debug.DrawRay(origin, direction * castDistance, Color.green);

    }
}
