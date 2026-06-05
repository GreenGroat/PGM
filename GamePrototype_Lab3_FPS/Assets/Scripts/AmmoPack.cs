using UnityEngine;

namespace Lab3FPS
{
    public class AmmoPack : MonoBehaviour
    {
        [SerializeField] private int ammoAmount = 30;
        [SerializeField] private bool addToAllWeapons;
        [SerializeField] private float rotationSpeed = 110f;

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            WeaponSwitcher weaponSwitcher = other.GetComponentInChildren<WeaponSwitcher>();
            if (weaponSwitcher == null)
            {
                return;
            }

            if (addToAllWeapons)
            {
                weaponSwitcher.AddAmmoToAllWeapons(ammoAmount);
                Destroy(gameObject);
                return;
            }

            if (weaponSwitcher.AddAmmoToActiveWeapon(ammoAmount))
            {
                Destroy(gameObject);
            }
        }
    }
}
