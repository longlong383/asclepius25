using UnityEngine;

namespace MRTK.Tutorials.MultiUserCapabilities
{
    public class TableAnchor1 : MonoBehaviour
    {
        public static TableAnchor1 Instance;

        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                if (Instance == this) return;
                Destroy(Instance.gameObject);
                Instance = this;
            }
        }
    }
}
