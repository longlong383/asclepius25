using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Microsoft.MixedReality.Toolkit.UI;

public class positionalSetData : MonoBehaviourPun
{
    private Transform patientOriginalLocation;
    [SerializeField] private Transform patient;
    private Transform transformDifference;
    private BooleanSync booleanSync;
    [SerializeField] private Interactable setLocation, setView;

    // Start is called before the first frame update

    void Start()
    {
        patientOriginalLocation.position = patient.position;
        patientOriginalLocation.rotation = patient.rotation;
        patientOriginalLocation.localScale = patient.localScale;
        if (FindObjectOfType<BooleanSync>() != null)
        {
            booleanSync = FindObjectOfType<BooleanSync>();
        }
        else
        {
            Debug.LogError("Error retrieving boolean drawing condition. Gameplay will be affected");
        }
        setLocation.OnClick.AddListener(() => setNewLocation());
        setView.OnClick.AddListener(() => changeVisibility(setView.IsToggled));
    }

    private void changeVisibility(bool placeHolder)
    {
        patient.GetComponent<GameObject>().SetActive(placeHolder);
    }

    public void setNewLocation()
    {
        Vector3 positionDifference = patient.position - patientOriginalLocation.position;
        Quaternion rotationDifference = Quaternion.Inverse(patientOriginalLocation.rotation) * patient.rotation;
        Vector3 scaleDifference = patient.lossyScale.Divide(patientOriginalLocation.lossyScale); // Component-wise division
        Transform temp = null;
        temp.transform.position = positionDifference;
        temp.transform.rotation = rotationDifference;
        //this might need to be changed to lossyscale in the future 
        temp.transform.localScale = scaleDifference;
        if (FindObjectOfType<dataStreamer>() == null)
        {
            Debug.LogError("error finding datastreamer");
        }
        dataStreamer streamer = FindObjectOfType<dataStreamer>();
        streamer.setupTransformationChange(temp);
    }
}
