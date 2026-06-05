using UnityEngine;

namespace Lab8Attention
{
    [RequireComponent(typeof(Light))]
    public class PulseLight : MonoBehaviour
    {
        public float baseIntensity = 1.5f;
        public float pulseIntensity = 1f;
        public float pulseSpeed = 2f;

        private Light targetLight;

        private void Awake()
        {
            targetLight = GetComponent<Light>();
        }

        private void Update()
        {
            targetLight.intensity = baseIntensity + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
        }
    }
}
