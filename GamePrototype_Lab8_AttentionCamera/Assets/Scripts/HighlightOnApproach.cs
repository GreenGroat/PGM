using UnityEngine;

namespace Lab8Attention
{
    [RequireComponent(typeof(Renderer))]
    public class HighlightOnApproach : MonoBehaviour
    {
        public float radius = 3f;
        public Material highlightMaterial;

        private Material originalMaterial;
        private Renderer objectRenderer;
        private Transform player;

        private void Start()
        {
            objectRenderer = GetComponent<Renderer>();
            originalMaterial = objectRenderer.material;

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        private void Update()
        {
            if (player == null || highlightMaterial == null)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, player.position);
            objectRenderer.material = distance <= radius ? highlightMaterial : originalMaterial;
        }
    }
}
