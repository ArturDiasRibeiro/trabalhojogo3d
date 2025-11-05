using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Gun : MonoBehaviour
{
    [Header("General")]
    public bool useHitscan = true; // True = instant hit, False = projectile
    public Transform muzzlePoint;
    public ParticleSystem muzzleFlash;
    public GameObject hitEffectPrefab;
    public LayerMask hitMask; // Layers this weapon can hit

    [Header("Hitscan Settings")]
    public float damage = 25f;
    public float range = 100f;
    public float fireRate = 10f; // bullets per second
    public float spreadAngle = 1f; // degrees

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 80f;

    [Header("Ammo")]
    public int magazineSize = 30;
    public int reserveAmmo = 90;
    public float reloadTime = 2.1f;

    [Header("Recoil & Camera")]
    public float recoilAmount = 2f;
    public float recoilRecoverSpeed = 8f;
    public Transform cameraTransform; // Assign your player camera

    [Header("Audio")]
    public AudioClip fireSfx;
    public AudioClip reloadSfx;

    private AudioSource audioSource;
    private float lastFireTime = 0f;
    private int currentAmmo;
    private bool isReloading = false;
    private float currentRecoil = 0f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        currentAmmo = magazineSize;
    }

    void Update()
    {
        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.Mouse1) && Time.time - lastFireTime >= 1f / fireRate)
        {
            if (currentAmmo <= 0)
                StartCoroutine(Reload());
            else
                Fire();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentAmmo < magazineSize && reserveAmmo > 0)
                StartCoroutine(Reload());
        }

        // Simple recoil recovery
        if (currentRecoil > 0f)
        {
            currentRecoil = Mathf.MoveTowards(currentRecoil, 0f, recoilRecoverSpeed * Time.deltaTime);
            if (cameraTransform != null)
                cameraTransform.localEulerAngles = new Vector3(-currentRecoil, 0f, 0f);
        }
    }

    void Fire()
    {
        lastFireTime = Time.time;
        currentAmmo--;

        // Muzzle flash & sound
        if (muzzleFlash != null) muzzleFlash.Play();
        if (fireSfx != null) audioSource.PlayOneShot(fireSfx);

        // Apply recoil
        currentRecoil += recoilAmount;
        if (cameraTransform != null)
            cameraTransform.localEulerAngles = new Vector3(-currentRecoil, 0f, 0f);

        if (useHitscan)
            PerformHitscanShot();
        else
            SpawnProjectile();
    }

    void PerformHitscanShot()
    {
        Vector3 direction = GetShotDirection();
        if (Physics.Raycast(muzzlePoint.position, direction, out RaycastHit hit, range, hitMask))
        {
            // Apply damage
            var health = hit.collider.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage);

            // Hit effect
            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }

    void SpawnProjectile()
    {
        if (projectilePrefab == null) return;

        GameObject proj = Instantiate(projectilePrefab, muzzlePoint.position, Quaternion.LookRotation(muzzlePoint.forward));
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = muzzlePoint.forward * projectileSpeed;
    }

    Vector3 GetShotDirection()
    {
        float spreadRad = Mathf.Deg2Rad * spreadAngle;
        Vector2 random = Random.insideUnitCircle * Mathf.Tan(spreadRad);
        Vector3 direction = muzzlePoint.forward + muzzlePoint.up * random.y + muzzlePoint.right * random.x;
        return direction.normalized;
    }

    IEnumerator Reload()
    {
        if (isReloading) yield break;
        if (currentAmmo == magazineSize || reserveAmmo <= 0) yield break;

        isReloading = true;
        if (reloadSfx != null) audioSource.PlayOneShot(reloadSfx);
        yield return new WaitForSeconds(reloadTime);

        int needed = magazineSize - currentAmmo;
        int taken = Mathf.Min(needed, reserveAmmo);
        currentAmmo += taken;
        reserveAmmo -= taken;
        isReloading = false;
    }

    // For UI
    public int GetCurrentAmmo() => currentAmmo;
    public int GetReserveAmmo() => reserveAmmo;
}