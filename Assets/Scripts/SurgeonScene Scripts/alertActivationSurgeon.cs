using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class alertActivationSurgeon : MonoBehaviour
{
    private BooleanSync booleanSync;

    [SerializeField] private Interactable emergency, general;
    // Start is called before the first frame update
    void Start()
    {
        if (FindObjectOfType<BooleanSync>() == null)
        {
            Debug.Log("Error retrieving booleanSync script");
        }
        else
        {
            booleanSync = FindObjectOfType<BooleanSync>();
        }

        emergency.OnClick.AddListener(() => emergencyActivation(emergency.IsToggled));
        general.OnClick.AddListener(() => generalActiviation(general.IsToggled));
        emergency.IsToggled = true;
        general.IsToggled = true;
    }

    private void emergencyActivation(bool placeHolder)
    {
        Debug.Log("Emergency toggle initial state: " + emergency.IsToggled);

        Debug.Log("alert bool before: " + booleanSync.returnAlertEmergency());
        Debug.Log("placeHolder before: " + placeHolder);
        booleanSync.setAlertEmergency(placeHolder);
        Debug.Log("emergency signal sent successfully");
        //yield return new WaitForSeconds(0.1f);
        Debug.Log("placeHolder after: " + placeHolder);
        Debug.Log("alert bool after: " + booleanSync.returnAlertEmergency());
    }

    private void generalActiviation(bool placeHolder)
    {
        Debug.Log("Emergency toggle initial state: " + general.IsToggled);

        Debug.Log("general bool before: " + booleanSync.returnAlertGeneral());
        Debug.Log("placeHolder before: " + placeHolder);
        booleanSync.setAlertGeneral(placeHolder);
        Debug.Log("general signal sent successfully");
        //yield return new WaitForSeconds(0.1f);
        Debug.Log("placeHolder after: " + placeHolder);
        Debug.Log("general bool after: " + booleanSync.returnAlertGeneral());
    }

}
