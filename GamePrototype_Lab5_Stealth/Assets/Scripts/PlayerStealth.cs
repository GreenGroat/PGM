using System;
using UnityEngine;

namespace Lab5Stealth
{
    public class PlayerStealth : MonoBehaviour
    {
        public static event Action<Vector3, float> NoiseEmitted;

        [Header("Noise")]
        public float crouchNoise = 0.08f;
        public float walkNoise = 0.35f;
        public float sprintNoise = 1f;
        public float airborneNoise = 0.2f;
        public float noiseRadiusMultiplier = 9f;
        public float emitInterval = 0.35f;

        public bool IsCrouching { get; private set; }
        public bool IsSprinting { get; private set; }
        public float CurrentNoise01 { get; private set; }
        public float LastNoiseRadius { get; private set; }

        private float nextEmitTime;

        public void SetCrouching(bool crouching)
        {
            IsCrouching = crouching;
        }

        public void SetSprinting(bool sprinting)
        {
            IsSprinting = sprinting;
        }

        public void UpdateMotion(bool moving, float speed, bool grounded)
        {
            float targetNoise = 0f;

            if (!grounded)
            {
                targetNoise = airborneNoise;
            }
            else if (moving)
            {
                targetNoise = IsCrouching ? crouchNoise : IsSprinting ? sprintNoise : walkNoise;
            }

            CurrentNoise01 = Mathf.MoveTowards(CurrentNoise01, targetNoise, Time.deltaTime * 4f);

            if (moving && grounded && CurrentNoise01 > 0.02f && Time.time >= nextEmitTime)
            {
                EmitNoise(CurrentNoise01);
                nextEmitTime = Time.time + emitInterval;
            }
        }

        public void EmitNoise(float intensity)
        {
            intensity = Mathf.Clamp01(intensity);
            LastNoiseRadius = intensity * noiseRadiusMultiplier;
            NoiseEmitted?.Invoke(transform.position, LastNoiseRadius);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.1f, 0.9f, 1f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(LastNoiseRadius, CurrentNoise01 * noiseRadiusMultiplier));
        }
    }
}
