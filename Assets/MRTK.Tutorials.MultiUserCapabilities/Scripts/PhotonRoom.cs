using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MRTK.Tutorials.MultiUserCapabilities
{
    public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
    {
        public static PhotonRoom Room;

        [SerializeField] private GameObject photonUserPrefab = default;
        [SerializeField] private GameObject annotationTrackingObject = default;
        [SerializeField] private GameObject connectionIndicator = default;
        [SerializeField] private Transform startingLocation = default;
        [SerializeField] private Transform indicatorLocation = default;

        // private PhotonView pv;
        private Player[] photonPlayers;
        private int playersInRoom;
        private int myNumberInRoom;

        // private GameObject module;
        // private Vector3 moduleLocation = Vector3.zero;

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            photonPlayers = PhotonNetwork.PlayerList;
            playersInRoom++;
        }

        private void Awake()
        {
            if (Room == null)
            {
                Room = this;
            }
            else
            {
                if (Room != this)
                {
                    Destroy(Room.gameObject);
                    Room = this;
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void Start()
        {
            // pv = GetComponent<PhotonView>();

            // Allow prefabs not in a Resources folder
            if (PhotonNetwork.PrefabPool is DefaultPool pool)
            {
                if (photonUserPrefab != null) pool.ResourceCache.Add(photonUserPrefab.name, photonUserPrefab);

                if (annotationTrackingObject != null) pool.ResourceCache.Add(annotationTrackingObject.name, annotationTrackingObject);
                if (connectionIndicator != null) pool.ResourceCache.Add(connectionIndicator.name, connectionIndicator);
            }
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            photonPlayers = PhotonNetwork.PlayerList;
            playersInRoom = photonPlayers.Length;
            myNumberInRoom = playersInRoom;
            PhotonNetwork.NickName = myNumberInRoom.ToString();

            StartGame();
        }

        private void StartGame()
        {
            CreatPlayer();

            if (!PhotonNetwork.IsMasterClient) return;

            if (TableAnchor.Instance != null) CreatePhantom();
            if (TableAnchor1.Instance != null) CreateIndicator();
        }

        private void CreatPlayer()
        {
            var player = PhotonNetwork.Instantiate(photonUserPrefab.name, new Vector3(0f, 0.2f, 0f), Quaternion.identity);
        }

        private void CreatePhantom()
        {
            var position = startingLocation.position;
            var positionOnTopOfSurface = new Vector3(position.x, position.y,
                position.z);


            var go = PhotonNetwork.Instantiate(annotationTrackingObject.name, positionOnTopOfSurface,
                startingLocation.rotation);
        }
        private void CreateIndicator()
        {
            var position1 = indicatorLocation.position;
            var positionOnTopOfSurface1 = new Vector3(position1.x, position1.y,
                position1.z);

            var go1 = PhotonNetwork.Instantiate(connectionIndicator.name, positionOnTopOfSurface1,
                indicatorLocation.rotation);
        }

        // private void CreateMainLunarModule()
        // {
        //     module = PhotonNetwork.Instantiate(roverExplorerPrefab.name, Vector3.zero, Quaternion.identity);
        //     pv.RPC("Rpc_SetModuleParent", RpcTarget.AllBuffered);
        // }
        //
        // [PunRPC]
        // private void Rpc_SetModuleParent()
        // {
        //     Debug.Log("Rpc_SetModuleParent- RPC Called");
        //     module.transform.parent = TableAnchor.Instance.transform;
        //     module.transform.localPosition = moduleLocation;
        // }
    }
}
