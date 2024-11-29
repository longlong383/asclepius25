using UnityEngine;

namespace MRTK.Tutorials.MultiUserCapabilities
{
    public class TableAnchorAsParent1 : MonoBehaviour
    {
        private void Start()
        {
            if (TableAnchor1.Instance != null) transform.parent = TableAnchor1.Instance.transform;
        }
    }
}
