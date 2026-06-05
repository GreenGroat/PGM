using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lab3FPS
{
    public class WeaponSwitcher : MonoBehaviour
    {
        [SerializeField] private GameObject[] weapons;
        [SerializeField] private KeyCode[] switchKeys = { KeyCode.Alpha1, KeyCode.Alpha2 };
        [SerializeField] private float switchDelay = 0.2f;
        [SerializeField] private Text statusText;

        private int currentWeaponIndex = -1;
        private bool isSwitching;

        public Gun ActiveGun
        {
            get
            {
                if (currentWeaponIndex < 0 || currentWeaponIndex >= weapons.Length)
                {
                    return null;
                }

                return weapons[currentWeaponIndex].GetComponent<Gun>();
            }
        }

        private void Start()
        {
            SelectWeaponInstant(0);
        }

        private void Update()
        {
            if (isSwitching || weapons == null || weapons.Length == 0)
            {
                return;
            }

            for (int i = 0; i < switchKeys.Length && i < weapons.Length; i++)
            {
                if (Input.GetKeyDown(switchKeys[i]))
                {
                    StartCoroutine(SwitchWeaponRoutine(i));
                    return;
                }
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                int nextIndex = scroll > 0f
                    ? (currentWeaponIndex + 1) % weapons.Length
                    : (currentWeaponIndex - 1 + weapons.Length) % weapons.Length;

                StartCoroutine(SwitchWeaponRoutine(nextIndex));
            }
        }

        public bool AddAmmoToActiveWeapon(int amount)
        {
            Gun gun = ActiveGun;
            if (gun == null)
            {
                return false;
            }

            gun.AddAmmo(amount);
            return true;
        }

        public void AddAmmoToAllWeapons(int amount)
        {
            foreach (GameObject weapon in weapons)
            {
                if (weapon != null && weapon.TryGetComponent(out Gun gun))
                {
                    gun.AddAmmo(amount);
                }
            }
        }

        public void SetWeaponsEnabled(bool enabled)
        {
            if (weapons == null || currentWeaponIndex < 0 || currentWeaponIndex >= weapons.Length)
            {
                return;
            }

            foreach (GameObject weapon in weapons)
            {
                if (weapon != null)
                {
                    weapon.SetActive(enabled && weapon == weapons[currentWeaponIndex]);
                }
            }
        }

        private IEnumerator SwitchWeaponRoutine(int index)
        {
            if (index < 0 || index >= weapons.Length || index == currentWeaponIndex)
            {
                yield break;
            }

            isSwitching = true;
            ActiveGun?.InterruptReload();

            if (currentWeaponIndex >= 0 && weapons[currentWeaponIndex] != null)
            {
                weapons[currentWeaponIndex].SetActive(false);
            }

            SetStatus("Switching weapon...");
            yield return new WaitForSeconds(switchDelay);

            SelectWeaponInstant(index);
            isSwitching = false;
        }

        private void SelectWeaponInstant(int index)
        {
            if (weapons == null || weapons.Length == 0)
            {
                return;
            }

            index = Mathf.Clamp(index, 0, weapons.Length - 1);

            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                {
                    weapons[i].SetActive(i == index);
                }
            }

            currentWeaponIndex = index;
            Gun activeGun = ActiveGun;
            SetStatus(activeGun != null ? $"Selected {activeGun.WeaponName}." : "Weapon selected.");
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
