using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lab3FPS
{
    public class Gun : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private string weaponName = "Pistol";
        [SerializeField] private float damage = 25f;
        [SerializeField] private float fireDelay = 0.35f;
        [SerializeField] private float range = 80f;
        [SerializeField] private int magazineSize = 12;
        [SerializeField] private int reserveAmmo = 48;
        [SerializeField] private float reloadTime = 1.2f;
        [SerializeField] private bool automatic;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Recoil and Spread")]
        [SerializeField] private float recoilAmount = 1.5f;
        [SerializeField] private float spread = 0.012f;

        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private FPSController fpsController;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private ParticleSystem muzzleFlash;

        [Header("UI")]
        [SerializeField] private Text ammoText;
        [SerializeField] private Text weaponText;
        [SerializeField] private Text statusText;

        private int currentAmmo;
        private float nextFireTime;
        private bool isReloading;
        private Coroutine reloadRoutine;

        public string WeaponName => weaponName;
        public bool IsReloading => isReloading;

        private void Awake()
        {
            currentAmmo = magazineSize;
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            UpdateUI();
            SetStatus($"{weaponName} ready.");
        }

        private void Update()
        {
            bool firePressed = automatic ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");
            if (firePressed && Time.time >= nextFireTime && !isReloading)
            {
                Shoot();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                BeginReload();
            }
        }

        public void BeginReload()
        {
            if (isReloading || currentAmmo >= magazineSize || reserveAmmo <= 0)
            {
                return;
            }

            reloadRoutine = StartCoroutine(ReloadRoutine());
        }

        public void InterruptReload()
        {
            if (reloadRoutine != null)
            {
                StopCoroutine(reloadRoutine);
            }

            reloadRoutine = null;
            isReloading = false;
            UpdateUI();
        }

        public void AddAmmo(int amount)
        {
            reserveAmmo += amount;
            UpdateUI();
            SetStatus($"+{amount} ammo for {weaponName}.");
        }

        private void Shoot()
        {
            if (currentAmmo <= 0)
            {
                SetStatus($"{weaponName} empty. Press R to reload.");
                BeginReload();
                return;
            }

            currentAmmo--;
            nextFireTime = Time.time + fireDelay;

            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            if (fpsController != null)
            {
                fpsController.AddRecoil(recoilAmount);
            }

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 direction = ApplySpread(ray.direction);

            if (Physics.Raycast(ray.origin, direction, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                EnemyTarget target = hit.collider.GetComponentInParent<EnemyTarget>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                    SetStatus($"{weaponName} hit {target.TargetName} for {damage:0}.");
                }
                else
                {
                    SetStatus($"{weaponName} hit {hit.collider.name}.");
                }
            }
            else
            {
                SetStatus($"{weaponName} missed.");
            }

            Debug.DrawRay(ray.origin, direction * range, Color.red, 1f);
            UpdateUI();
        }

        private IEnumerator ReloadRoutine()
        {
            isReloading = true;
            UpdateUI();
            SetStatus($"Reloading {weaponName}...");

            yield return new WaitForSeconds(reloadTime);

            int neededAmmo = magazineSize - currentAmmo;
            int ammoToAdd = Mathf.Min(neededAmmo, reserveAmmo);
            currentAmmo += ammoToAdd;
            reserveAmmo -= ammoToAdd;

            isReloading = false;
            reloadRoutine = null;
            UpdateUI();
            SetStatus($"{weaponName} reloaded.");
        }

        private Vector3 ApplySpread(Vector3 baseDirection)
        {
            if (playerCamera == null)
            {
                return baseDirection;
            }

            Vector3 spreadDirection = baseDirection;
            spreadDirection += playerCamera.transform.right * Random.Range(-spread, spread);
            spreadDirection += playerCamera.transform.up * Random.Range(-spread, spread);
            return spreadDirection.normalized;
        }

        private void ResolveReferences()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (fpsController == null)
            {
                fpsController = GetComponentInParent<FPSController>();
            }
        }

        private void UpdateUI()
        {
            if (ammoText != null)
            {
                string reloadLabel = isReloading ? " (reloading)" : string.Empty;
                ammoText.text = $"Ammo: {currentAmmo}/{magazineSize} | Reserve: {reserveAmmo}{reloadLabel}";
            }

            if (weaponText != null)
            {
                weaponText.text = $"Weapon: {weaponName}";
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
    }
}
