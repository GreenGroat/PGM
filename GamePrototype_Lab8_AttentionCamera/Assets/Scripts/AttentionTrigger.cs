using UnityEngine;

namespace Lab8Attention
{
    public class AttentionTrigger : MonoBehaviour
    {
        public CameraController cameraController;
        public Transform cameraPoint;
        public float cameraDuration = 2.5f;
        [TextArea] public string message;
        public bool destroyAfterTrigger = true;
        public bool playCue = true;

        private static AudioClip cueClip;
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = FindObjectOfType<AudioSource>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            CameraController controller = cameraController != null ? cameraController : FindObjectOfType<CameraController>();
            if (controller != null && cameraPoint != null)
            {
                controller.SetFixedCamera(cameraPoint, cameraDuration);
            }

            if (!string.IsNullOrEmpty(message))
            {
                HintManager.Instance?.ShowHint(message, cameraDuration);
            }

            if (playCue)
            {
                PlayCue();
            }

            if (destroyAfterTrigger)
            {
                Destroy(gameObject, 0.05f);
            }
        }

        private void PlayCue()
        {
            if (audioSource == null)
            {
                return;
            }

            if (cueClip == null)
            {
                cueClip = CreateCueClip();
            }

            audioSource.PlayOneShot(cueClip, 0.35f);
        }

        private static AudioClip CreateCueClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.18f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Sin((i / (float)sampleCount) * Mathf.PI);
                samples[i] = Mathf.Sin(t * 880f * Mathf.PI * 2f) * envelope * 0.5f;
            }

            AudioClip clip = AudioClip.Create("Attention Cue", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
