using UnityEngine;

namespace Lab8Attention
{
    public class RevealObjectAction : MonoBehaviour
    {
        public GameObject[] objectsToReveal;
        public Light revealLight;
        public string revealHint = "A hidden object has appeared.";

        public void Reveal()
        {
            for (int i = 0; i < objectsToReveal.Length; i++)
            {
                if (objectsToReveal[i] != null)
                {
                    objectsToReveal[i].SetActive(true);
                }
            }

            if (revealLight != null)
            {
                revealLight.enabled = true;
            }

            if (!string.IsNullOrEmpty(revealHint))
            {
                HintManager.Instance?.ShowHint(revealHint, 3f);
            }
        }
    }
}
