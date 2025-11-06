using StarterAssets;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Gun : MonoBehaviour
{
    // Variável para ler os inputs do Novo Sistema
    private StarterAssetsInputs starterInputs;

    [Header("General")]
    public bool useHitscan = true;
    public Transform muzzlePoint;
    public ParticleSystem muzzleFlash;
    public GameObject hitEffectPrefab;
    public LayerMask hitMask;

    [Header("Hitscan Settings")]
    public float damage = 25f;
    public float range = 100f;
    public float fireRate = 10f;
    public float spreadAngle = 1f;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // ATENÇÃO: Use o seu 'Player_Bullet_Prefab' aqui
    public float projectileSpeed = 80f;

    [Header("Ammo")]
    public int magazineSize = 30;
    public int reserveAmmo = 90;
    public float reloadTime = 2.1f;

    [Header("Recoil & Camera")]
    public float recoilAmount = 2f;
    public float recoilRecoverSpeed = 8f;
    public Transform cameraTransform;

    [Header("Audio")]
    public AudioClip fireSfx;
    public AudioClip reloadSfx;

    // --- Variáveis Privadas ---
    private AudioSource audioSource;
    private float lastFireTime = 0f;
    private int currentAmmo;
    private bool isReloading = false;
    private float currentRecoil = 0f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        currentAmmo = magazineSize;
        // Pega a referência do script de inputs que está no mesmo objeto
        starterInputs = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (isReloading) return;

        // --- LÓGICA DE DISPARO (Corrigida para o Novo Input) ---
        // Lê a variável 'fire' do StarterAssetsInputs
        if (starterInputs.fire && Time.time - lastFireTime >= 1f / fireRate)
        {
            if (currentAmmo <= 0)
                StartCoroutine(Reload());
            else
                Fire();
        }

        // --- LÓGICA DE RECARREGAR (BUG CORRIGIDO) ---
        // Lê a variável 'reload' do StarterAssetsInputs
        if (starterInputs.reload)
        {
            // Reseta a flag imediatamente para não recarregar em loop
            starterInputs.reload = false;

            if (currentAmmo < magazineSize && reserveAmmo > 0)
                StartCoroutine(Reload());
        }

        // Lógica de Recuo (igual)
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

        if (muzzleFlash != null) muzzleFlash.Play();
        if (fireSfx != null) audioSource.PlayOneShot(fireSfx);

        currentRecoil += recoilAmount;
        if (cameraTransform != null)
            cameraTransform.localEulerAngles = new Vector3(-currentRecoil, 0f, 0f);

        if (useHitscan)
            PerformHitscanShot();
        else
            SpawnProjectile();

        // Reseta a flag de 'fire' para permitir tiro semi-automático (um por clique)
        // Se quiser automático (segurar o botão), comente esta linha
        starterInputs.fire = false;
    }

    void PerformHitscanShot()
    {
        Vector3 direction = GetShotDirection();
        if (Physics.Raycast(muzzlePoint.position, direction, out RaycastHit hit, range, hitMask))
        {
            // Procura o script de vida do INIMIGO (EnemyHealth)
            var health = hit.collider.GetComponent<EnemyHealth>();
            if (health != null)
                health.TakeDamage(damage);

            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }

    // --- LÓGICA DE PROJÉTIL (BUG CORRIGIDO) ---
    void SpawnProjectile()
    {
        if (projectilePrefab == null) return;

        GameObject proj = Instantiate(projectilePrefab, muzzlePoint.position, Quaternion.LookRotation(muzzlePoint.forward));

        // Pega o script 'PlayerProjectile' que fizemos
        PlayerProjectile projScript = proj.GetComponent<PlayerProjectile>();
        if (projScript != null)
        {
            // "Diz" ao projétil qual o dano a dar e quem o atirou
            projScript.damage = this.damage;
            projScript.owner = this.gameObject;
        }
        else
        {
            Debug.LogError("O Prefab do Projétil NÃO TEM o script 'PlayerProjectile.cs'!");
        }

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

    // Funções para a UI (não precisam de mudança)
    public int GetCurrentAmmo() => currentAmmo;
    public int GetReserveAmmo() => reserveAmmo;
}