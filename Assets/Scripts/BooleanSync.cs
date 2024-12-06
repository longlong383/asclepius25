using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class BooleanSync : MonoBehaviourPunCallbacks
{
    public const string BooleanKey = "isDrawing";
    public const string BooleanKey1 = "isConnected";

    //set default values
    void Start()
    {
        // Initialize both booleans to false at the beginning
        setIsDrawing(false);
        setIsConnected(false);
    }

    // Set the boolean property
    public void setIsDrawing(bool value)
    {
        Hashtable properties = new Hashtable
        {
            { BooleanKey, value }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    // Get the boolean property
    public bool returnIsDrawing()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(BooleanKey, out var value))
        {
            return (bool)value;
        }
        return false;
    }

    public void setIsConnected(bool value)
    {
        Hashtable properties = new Hashtable
        {
            { BooleanKey1, value }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    // Get the boolean property
    public bool returnIsConnected()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(BooleanKey1, out var value))
        {
            return (bool)value;
        }
        return false;
    }


}
