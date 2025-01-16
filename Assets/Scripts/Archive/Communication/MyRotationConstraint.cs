using UnityEngine;

public class MyRotationConstraint : MonoBehaviour
{
    void Update()
    {
        var rotation = transform.eulerAngles;
        rotation.z = 0; // Lock the Z-axis rotation to 0
        transform.eulerAngles = rotation;

    }
}
