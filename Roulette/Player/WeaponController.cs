using UnityEngine;
using System.Collections;
using TMPro;

public class WeaponController : MonoBehaviour
{
    // Weapon Properties
    public float damage = 30f;
    public float range = 100f;
    public float fireRate = 0.5f;
    public int magazineSize = 30;
    public int currentAmmo;
    public float reloadTime = 1.5f;

    // Weapon References
    public Camera playerCamera;
    public LayerMask targetLayer;
    public ParticleSystem muzzleFlash;
    public AudioSource weaponAudioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;

    // UI References
    public TextMeshProUGUI ammoText;

    // Weapon State
    private bool canShoot = true;
    private bool isReloading = false;
    private bool isAiming = false;

    void Start()
    {
        // Initialize ammo to full magazine
        currentAmmo = magazineSize;
        UpdateAmmoUI();
    }

    void Update()
    {
        // Check for shooting input
        if (Input.GetMouseButtonDown(0) && canShoot && !isReloading)
        {
            Shoot();
        }

        // Check for reload input
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
        }
    }

    public void SetAimStatus(bool aiming)
    {
        isAiming = aiming;
    }

    void Shoot()
    {
        // Check if we have ammo
        if (currentAmmo <= 0)
        {
            // Play empty magazine sound
            if (weaponAudioSource != null && emptySound != null)
                weaponAudioSource.PlayOneShot(emptySound);
            return;
        }

        // Reduce ammo count
        currentAmmo--;
        UpdateAmmoUI();

        // Play shooting effects
        if (weaponAudioSource != null && shootSound != null)
            weaponAudioSource.PlayOneShot(shootSound);

        if (muzzleFlash != null)
            muzzleFlash.Play();

        // Create ray from center of camera
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // 조준 중일 때 더 정확하게 (스프레드 감소)
        float spreadFactor = isAiming ? 0.01f : 0.05f;
        Vector3 spreadDirection = ray.direction;

        // 간단한 무기 스프레드 추가 (조준 중이 아닐 때 정확도 감소)
        if (!isAiming)
        {
            spreadDirection += new Vector3(
                Random.Range(-spreadFactor, spreadFactor),
                Random.Range(-spreadFactor, spreadFactor),
                Random.Range(-spreadFactor, spreadFactor)
            );
            ray.direction = spreadDirection.normalized;
        }

        // Perform raycast
        if (Physics.Raycast(ray, out hit, range, targetLayer))
        {
            Debug.Log("Hit: " + hit.transform.name);

            // Optional: Add damage to target
            IDamageable damageable = hit.transform.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            // Optional: Create impact effect
            // Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }

        // Prevent rapid firing
        StartCoroutine(FireRateLimit());
    }

    IEnumerator FireRateLimit()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    IEnumerator Reload()
    {
        isReloading = true;

        if (weaponAudioSource != null && reloadSound != null)
            weaponAudioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);

        // Reset ammo to full
        currentAmmo = magazineSize;
        UpdateAmmoUI();

        isReloading = false;
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo}/{magazineSize}";
        }
    }
}

// Optional Interface for Damage System
public interface IDamageable
{
    void TakeDamage(float damage);
}