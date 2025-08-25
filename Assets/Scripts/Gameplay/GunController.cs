using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GunController : MonoBehaviour
{
    public Transform gunTip;
    public float bulletSpeed = 100f;
    public int maxAmmo = 30;
    private int currentAmmo;
    public int range = 100;
    public int damage = 34;
    public LayerMask shootable;
    public GameObject muzzleFlash;
    public Animator animator;
    public Camera playerCamera;
    public AudioSource gunAudioSource;
    public AudioClip fireClip;
    public GameObject impactPrefab;

    [Header("Aim Assist")]
    public float aimAssistRadius = 0.5f;
    public float spherecastStartOffset = 1f;
    public bool useOverlapSphereBackup = true;

    [Header("Gun Sway")]
    public Transform gun;
    public float swayAmount = 0.02f;
    public float swaySmooth = 8f;
    public float swayRotationAmount = 1f;
    public float swayRotationSmooth = 8f;
    public float swayMaxAmount = 0.06f;
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;
    private PlayerMovement playerMovement;

    public Recoil camShake;
    public Recoil recoil;
    public float timeBetweenShots = 0.7f;
    private bool canShoot = true;
    private bool shooting = false;
    private bool finished = false;
    public int aimFOV;
    public bool aiming;

    float nextShotTime=0f;


    private void Start()
    {
        currentAmmo = maxAmmo;
        initialLocalPos = gun.localPosition;
        initialLocalRot = gun.localRotation;
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    void Update()
    {
        Vector2 lookInput = Vector2.zero;
        if (playerMovement != null)
        {
            lookInput = playerMovement.lookInputs;
        }

        float swayX = Mathf.Clamp(-lookInput.x * swayAmount, -swayMaxAmount, swayMaxAmount);
        float swayY = Mathf.Clamp(-lookInput.y * swayAmount, -swayMaxAmount, swayMaxAmount);

        Vector3 targetPos = initialLocalPos + new Vector3(swayX, swayY, 0);
        gun.localPosition = Vector3.Lerp(gun.localPosition, targetPos, Time.deltaTime * swaySmooth);

        Quaternion targetRot = initialLocalRot * Quaternion.Euler(
            lookInput.y * swayRotationAmount,
            lookInput.x * swayRotationAmount,
            lookInput.x * swayRotationAmount * 0.5f
        );
        gun.localRotation = Quaternion.Slerp(gun.localRotation, targetRot, Time.deltaTime * swayRotationSmooth);

        if (shooting && Time.time >= nextShotTime && currentAmmo > 0)
        {
            Fire();
        }
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.started && currentAmmo > 0)
        {
            finished = false;
            shooting = true;
        }
        else if (context.canceled || currentAmmo == 0)
        {
            shooting = false;
            finished = true;
        }
    }

    public void Aim(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            aiming = true;
            animator.Play("Aim");
        }
        else if (context.canceled)
        {
            aiming = false;
            animator.Play("Idle");
        }
        recoil.aim = aiming;
        camShake.aim = aiming;

    }

    public void Reload()
    {
        canShoot = false;
        animator.Play("Reload");
        StartCoroutine(ReloadTimer());
    }

    IEnumerator ReloadTimer()
    {
        yield return new WaitForSeconds(3);
        currentAmmo = maxAmmo;
        canShoot = true;
    }

    private void Fire()
    {
        if (currentAmmo <= 0 || Time.time < nextShotTime) return;

        nextShotTime = Time.time + timeBetweenShots;
        currentAmmo--;

        PlayShootEffects();

        if (TryGetTarget(out RaycastHit hit, out Enemy enemy))
        {
            if (hit.collider != null)
            {
                SpawnImpact(hit);
            }

            if (enemy != null)
                enemy.TakeDamage(damage);
        }
    }

    private void PlayShootEffects()
    {
        animator.Play("Shoot");

        gunAudioSource.pitch = Random.Range(0.95f, 1.05f);
        gunAudioSource.PlayOneShot(fireClip);

        camShake.Fire();
        recoil.Fire();

        muzzleFlash.SetActive(true);
        Invoke(nameof(DisableMuzzleFlash), 0.05f);
    }

    private void DisableMuzzleFlash() => muzzleFlash.SetActive(false);

    private void SpawnImpact(RaycastHit hit)
    {
        var impact = Instantiate(impactPrefab, hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal));
        Destroy(impact, 3f);
    }

    private bool TryGetTarget(out RaycastHit hit, out Enemy enemy)
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, range, shootable, QueryTriggerInteraction.Ignore))
        {
            enemy = GetEnemyFromHit(hit.collider.gameObject);
            return true;
        }

        Vector3 sphereStart = playerCamera.transform.position + playerCamera.transform.forward * spherecastStartOffset;
        float adjustedRange = range - spherecastStartOffset;

        if (Physics.SphereCast(sphereStart, aimAssistRadius, playerCamera.transform.forward, out hit, adjustedRange, shootable, QueryTriggerInteraction.Ignore))
        {
            enemy = GetEnemyFromHit(hit.collider.gameObject);
            return true;
        }

        if (useOverlapSphereBackup)
        {
            enemy = FindClosestEnemyInLine(out Vector3 pos, out Vector3 normal);
            if (enemy != null)
            {
                Vector3 dirToEnemy = (enemy.transform.position - playerCamera.transform.position).normalized;
                if (Physics.Raycast(playerCamera.transform.position, dirToEnemy, out hit, range, shootable, QueryTriggerInteraction.Ignore))
                {
                    return true;
                }
                else
                {
                    hit = default(RaycastHit);
                    return true;
                }
            }
        }

        hit = default(RaycastHit);
        enemy = null;
        return false;
    }

    private Enemy GetEnemyFromHit(GameObject obj)
    {
        if (!obj) return null;
        return obj.GetComponentInParent<Enemy>();
    }

    private Enemy FindClosestEnemyInLine(out Vector3 position, out Vector3 normal)
    {
        Enemy closestEnemy = null;
        float closestDistance = Mathf.Infinity;
        float maxAngle = 30f;

        int checkPoints = 5;
        for (int i = 1; i <= checkPoints; i++)
        {
            float distance = (range / checkPoints) * i;
            Vector3 checkPos = playerCamera.transform.position + playerCamera.transform.forward * distance;

            Collider[] colliders = Physics.OverlapSphere(checkPos, aimAssistRadius, shootable);
            foreach (var col in colliders)
            {
                var enemy = GetEnemyFromHit(col.gameObject);
                if (enemy != null)
                {
                    Vector3 dirToEnemy = (enemy.transform.position - playerCamera.transform.position).normalized;
                    float angle = Vector3.Angle(playerCamera.transform.forward, dirToEnemy);

                    if (angle <= maxAngle)
                    {
                        float dist = Vector3.Distance(playerCamera.transform.position, enemy.transform.position);
                        if (dist < closestDistance)
                        {
                            closestEnemy = enemy;
                            closestDistance = dist;
                        }
                    }
                }
            }
        }

        if (closestEnemy != null)
        {
            position = closestEnemy.transform.position;
            normal = -(closestEnemy.transform.position - playerCamera.transform.position).normalized;
            return closestEnemy;
        }

        position = Vector3.zero;
        normal = Vector3.forward;
        return null;
    }

}
