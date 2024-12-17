using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class handIndexAccess : MonoBehaviour
{
    public GameObject objectToAttach; // Assign this in the Inspector
    public Handedness handedness = Handedness.Right; // Set to Left or Right hand as needed
    // Start is called before the first frame update

    void Update()
    {
        //attaches annotation tracker to the index finger
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, handedness, out MixedRealityPose pose))
        {
            objectToAttach.transform.localPosition = pose.Position; // Optional: Adjust as needed
            objectToAttach.transform.localRotation = pose.Rotation; // Optional: Adjust as needed
            //Debug.Log("Object successfully attached as a child to the finger tip.");
        }
    }
}
