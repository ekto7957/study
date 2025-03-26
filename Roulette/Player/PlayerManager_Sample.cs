using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ParticleManager;

public enum WeaponMode
{
    Pistol,
    Shotgun,
    Rifle,
}

public class PlayerManager_Sample : MonoBehaviour
{
    public static PlayerManager_Sample Instance { get; private set; }


    private float moveSpeed = 5.0f; //�÷��̾� �̵� �ӵ�
    public float mouseSensitivity = 100.0f; // ���콺 ����
    public Transform cameraTransform; // ī�޶��� Transform
    public CharacterController characterController;
    public Transform playerHead; //�÷��̾� �Ӹ� ��ġ(1��Ī ��带 ���ؼ�)
    public float thirdPersonDistance = 3.0f; //3��Ī ��忡�� �÷��̾�� ī�޶��� �Ÿ�
    public Vector3 thirdPersonOffset = new Vector3(0f, 1.5f, 0f); //3��Ī ��忡�� ī�޶� ������
    public Transform playerLookObj; //�÷��̾� �þ� ��ġ

    public float zoomeDistance = 1.0f; //ī�޶� Ȯ��� ���� �Ÿ�(3��Ī ��忡�� ���)
    public float zoomSpeed = 5.0f; // Ȯ����Ұ� �Ǵ� �ӵ�
    public float defaultFov = 60.0f; //�⺻ ī�޶� �þ߰�
    public float zoomeFov = 30.0f; //Ȯ�� �� ī�޶� �þ߰�(1��Ī ��忡�� ���)

    private float currentDistance; //���� ī�޶���� �Ÿ�(3��Ī ���)
    private float targetDistance; //��ǥ ī�޶� �Ÿ�
    private float targetFov; //��ǥ FOV
    //private bool isZoomed = false; //Ȯ�� ���� Ȯ��
    private Coroutine zoomCoroutine; //�ڷ�ƾ�� ����Ͽ� Ȯ�� ��� ó��
    private Camera mainCamera; //ī�޶� ������Ʈ

    private float pitch = 0.0f; //���Ʒ� ȸ�� ��
    private float yaw = 0.0f; //�¿� ȸ�� ��
    private bool isFirstPerson = false; //1��Ī ��� ����
    private bool isRotaterAroundPlayer = false; //ī�޶� �÷��̾� ������ ȸ���ϴ��� ���� 

    //�߷� ���� ����
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
    private string currentAnimation = "Idle";

    public Transform aimTarget;

    private float weaponMaxDistance = 1000.0f;
    public LayerMask TargetLayerMask;

    public MultiAimConstraint multiAimConstraint;


    public Vector3 boxSize = new Vector3(1.0f, 1.0f, 1.0f);
    public float castDistance = 5.0f;
    public LayerMask itemLayer;
    public Transform itemGetPos;

    public GameObject crosshairObj;
    public GameObject m4IconImage;

    private bool isUseWeapon = false;
    private bool isGetM4Item = false;

    public ParticleSystem m4Effect;

    private float rifleFireDelay = 0.5f;

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

    private WeaponMode currentWeaponMode = WeaponMode.Rifle;
    private int ShotgunRayCount = 5;
    private float shotGunSpreadAngle = 10.0f;
    private float recoilStrength = 2.0f;
    private float maxRecoilAngle = 10.0f;
    private float currentRecoil = 0.0f;
    private float shakeDuration = 0.1f;
    private float shakeMagnitude = 0.1f;
    private Vector3 originalCameraPosition;
    private Coroutine cameraShakeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
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
        bulletText.text = $"{firebulletCount}/{savebulletCount}";
        bulletText.gameObject.SetActive(false);
        flashLightObj.SetActive(false);
        PauseObj.SetActive(false);

        RenderSettings.fog = true; // �Ȱ�ȿ�� Ȱ��ȭ
        // �Ȱ� �� ����
        RenderSettings.fogColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        // �Ȱ� ���� �Ÿ�
        RenderSettings.fogStartDistance = 10.0f;
        // �Ȱ� �� �Ÿ�
        RenderSettings.fogEndDistance = 100.0f;
        // �Ȱ� �е� ����
        RenderSettings.fogDensity = 0.5f;
        RenderSettings.fogMode = FogMode.ExponentialSquared;

        if (mainCamera != null) // ī�޶��� clear flag�� solid color�� �����ϰ� ������ �Ȱ������� ����

        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = RenderSettings.fogColor;

        }
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
        if (Input.GetKeyDown(KeyCode.T))
        {
            ActionFlashLight();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPause = !isPause;

            if (isPause)
            {
                Pause();
            }
            else
            {
                ReGame();
            }
        }

        animator.speed = animationSpeed;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(currentAnimation) && stateInfo.normalizedTime >= 1.0f)
        {
            currentAnimation = "Attack";
            animator.Play(currentAnimation);
        }

        if (currentRecoil > 0)
        {
            currentRecoil -= recoilStrength * Time.deltaTime;
            currentRecoil = Mathf.Clamp(currentRecoil, 0, maxRecoilAngle);
            Quaternion currentRotation = Camera.main.transform.rotation;
            Quaternion recoliRotation = Quaternion.Euler(-currentRecoil, 0, 0);
            Camera.main.transform.rotation = currentRotation * recoliRotation; //ī�޶� �����ϴ� �ڵ带 �����Ѵ�.
        }
    }

    void FireShotgun()
    {
        for (int i = 0; i < ShotgunRayCount; i++)
        {
            RaycastHit hit;

            Vector3 origin = Camera.main.transform.position;
            Vector3 spreadDirection = GetSpreadDirection(Camera.main.transform.forward, shotGunSpreadAngle);
            Debug.DrawRay(origin, spreadDirection * castDistance, Color.green, 2.0f);
            if (Physics.Raycast(origin, spreadDirection, out hit, castDistance, TargetLayerMask))
            {
                Debug.Log("Shotgun Hit : " + hit.collider.name);
            }
        }
    }

    Vector3 GetSpreadDirection(Vector3 forwardDirection, float spreadAngle)
    {
        float spreadX = Random.Range(-spreadAngle, spreadAngle);
        float spreadY = Random.Range(-spreadAngle, spreadAngle);
        Vector3 spreadDirection = Quaternion.Euler(spreadX, spreadY, 0) * forwardDirection;
        return spreadDirection;
    }

    void ApplyRecoil()
    {
        Quaternion currentRotation = Camera.main.transform.rotation; //���� ī�޶� ���� ȸ���� ��������
        Quaternion recoilRotation = Quaternion.Euler(-currentRecoil, 0, 0); //�ݵ��� ����ؼ� X�� ���� ȸ���� �߰�
        Camera.main.transform.rotation = currentRotation * recoilRotation; //���� ȸ�� ���� �ݵ��� ���Ͽ� ���ο� ȸ��������
        currentRecoil += recoilStrength; //�ݵ� ���� ����
        currentRecoil = Mathf.Clamp(currentRecoil, 0, maxRecoilAngle); //�ݵ����� ����
    }

    void StartCameraShake()
    {
        if (cameraShakeCoroutine != null)
        {
            StopCoroutine(cameraShakeCoroutine);
        }
        cameraShakeCoroutine = StartCoroutine(CameraShake(shakeDuration, shakeMagnitude));
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        Vector3 originalPosition = Camera.main.transform.position;
        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1.0f, 1.0f) * magnitude;
            float offsetY = Random.Range(-1.0f, 1.0f) * magnitude;

            Camera.main.transform.position = originalPosition + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }
        Camera.main.transform.position = originalPosition;
    }

    public void ReGame()
    {
        audioSource.PlayOneShot(audioClipItemGet);
        PauseObj.SetActive(false);
        Time.timeScale = 1; //���� �ð� �簳
    }

    void Pause()
    {
        audioSource.PlayOneShot(audioClipItemGet);
        PauseObj.SetActive(true);
        Time.timeScale = 0; //���� �ð� ����
    }

    public void Exit()
    {
        audioSource.PlayOneShot(audioClipItemGet);
        PauseObj.SetActive(false);
        Time.timeScale = 1;
        Application.Quit();
    }

    void ActionFlashLight()
    {
        audioSource.PlayOneShot(audioClipFlashLightOn);
        isFlashLightOn = !isFlashLightOn;
        flashLightObj.SetActive(isFlashLightOn);
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
                hit.collider.gameObject.SetActive(false);
                audioSource.PlayOneShot(audioClipItemGet);
                m4IconImage.SetActive(true);
                isGetM4Item = true;
                bulletText.gameObject.SetActive(true);
            }
            else if (hit.collider.name == "ItemBullet")
            {
                hit.collider.gameObject.SetActive(false);
                audioSource.PlayOneShot(audioClipItemGet);
                savebulletCount += 30;
                if (savebulletCount >= 120)
                {
                    savebulletCount = 120;
                }
                bulletText.text = $"{firebulletCount}/{savebulletCount}";
                bulletText.gameObject.SetActive(true);
            }

        }
    }

    void MouseSet()
    {
        //���콺 �Է��� �޾� ī�޶�� �÷��̾� ȸ�� ó��
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
            Debug.Log(isFirstPerson ? "1��Ī ���" : "3��Ī ���");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            isRotaterAroundPlayer = !isRotaterAroundPlayer;
            Debug.Log(isRotaterAroundPlayer ? "ī�޶� ������ ȸ���մϴ�." : "�÷��̾ �þ߿� ���� ȸ���մϴ�.");
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
        if (Input.GetMouseButtonDown(0))
        {
            if (isAim && !isFire)
            {

                if (currentWeaponMode == WeaponMode.Pistol)
                {
                    recoilStrength = 10;
                }
                else if (currentWeaponMode == WeaponMode.Shotgun)
                {
                    recoilStrength = 40;
                    if (firebulletCount > 0)
                    {
                        firebulletCount -= 1;
                        bulletText.text = $"{firebulletCount}/{savebulletCount}";
                        bulletText.gameObject.SetActive(true);
                    }

                    FireShotgun();
                }
                else if (currentWeaponMode == WeaponMode.Rifle)
                {
                    recoilStrength = 20;
                }

                if (firebulletCount > 0)
                {
                    firebulletCount -= 1;
                    bulletText.text = $"{firebulletCount}/{savebulletCount}";
                    bulletText.gameObject.SetActive(true);
                }
                else
                {
                    //�Ѿ��� ���� �Ҹ� ���
                    return;
                }

                //Weapon Type MaxDistance Set
                weaponMaxDistance = 1000.0f;

                isFire = true;

                //Weapon Type FireDelay Data Fix
                StartCoroutine(FireWithDelay(rifleFireDelay));
                animator.SetTrigger("Fire");

                ApplyRecoil();
                StartCameraShake();

                Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
                RaycastHit[] hits = Physics.RaycastAll(ray, weaponMaxDistance, TargetLayerMask);

                if (hits.Length > 0)
                {
                    for (int i = 0; i < hits.Length && i < 2; i++)
                    {
                        ParticleManager.Instance.ParticlePlay(ParticleType.DamageExplosion, hits[i].point, Vector3.one);
                        audioSource.PlayOneShot(audioClipDamage);
                        hits[i].collider.GetComponent<ZombieManager>().TakeDamage(30.0f);
                    }

                }
                else
                {
                    Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 3.0f);
                }


            }
        }
        //if (Input.GetMouseButtonUp(0))
        //{
        //    isFire = false;
        //}
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
        animator.SetBool("IsRunning", isRunning);
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
            //ī�޶� �÷��̾� �����ʿ��� ȸ���ϵ��� ����
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            //ī�޶� �÷��̾��� �����ʿ��� ������ ��ġ�� �̵�
            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            //ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ����
            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else
        {
            //�÷��̾ ���� ȸ���ϴ� ���
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
        while (Mathf.Abs(currentDistance - targetDistance) > 0.01f) //���� �Ÿ����� ��ǥ �Ÿ��� �ε巴�� �̵�
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        currentDistance = targetDistance; // ��ǥ �Ÿ��� ������ �� ���� ����
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

    IEnumerator FireWithDelay(float fireDelay)
    {
        yield return new WaitForSeconds(fireDelay);
        isFire = false;
    }

    public void WeaponChangeSoundOn()
    {
        audioSource.PlayOneShot(audioClipWeaponChange);
    }

    public void WeaponFireSoundOn()
    {
        audioSource.PlayOneShot(audioClipFire);
        m4Effect.Play();
    }

    public void MovementSoundOn()
    {
        animationSpeed = 1;
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
        //else if( hit.collider.name == "Door")
        //{
        //    DoorContoller_sample doorManager = ColliderHit.collider.GetComponent<DoorController_sample>();
        //    if(doorManager != null)
        //    {
        //        if (doorManager.isOpen)
        //        {
        //            if (lastOpendForward)
        //            {
        //                doorManager.CloseForward(transform);
        //            }
        //            else
        //            {
        //                doorManager.CloseBackward(transform);
        //            }
        //        }
        //        else
        //        {
        //            if(doorManager.Open(transform))
        //            {
        //                lastOpendForward = doorManager.LastOpendForward;
        //            }

        //        }
        //        return;
        //    }
                
        //}

    }


    void DebugBox(Vector3 origin, Vector3 direction)
    {
        Vector3 endPoint = origin + direction * castDistance;

        Vector3[] corners = new Vector3[8];
        corners[0] = origin + new Vector3(-boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[1] = origin + new Vector3(boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[2] = origin + new Vector3(-boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[3] = origin + new Vector3(boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[4] = origin + new Vector3(-boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[5] = origin + new Vector3(boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[6] = origin + new Vector3(-boxSize.x, boxSize.y, boxSize.z) / 2;
        corners[7] = origin + new Vector3(boxSize.x, boxSize.y, boxSize.z) / 2;

        Debug.DrawLine(corners[0], corners[1], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[3], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[2], Color.green, 3.0f);
        Debug.DrawLine(corners[2], corners[0], Color.green, 3.0f);
        Debug.DrawLine(corners[4], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[5], corners[7], Color.green, 3.0f);
        Debug.DrawLine(corners[7], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[6], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[0], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[2], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[7], Color.green, 3.0f);
        Debug.DrawRay(origin, direction * castDistance, Color.green);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) // ���� �ε�� �� ȣ��Ǵ� �Լ�
    {

        //���� �ٱж����� �ν��Ͻ��� �����ִ°��� �ƴ϶� �ʱ�ȭ�� �ʿ��ϴ�. ������ �߰����� ����
        //���� ��ü �ڵ带 ������ �ʱ�ȭ�� �ؾ���
        Debug.Log("Scene Loaded : " + scene.name);
        
    }
}