using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


namespace Tanks
{
    public class LobbyListController : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button createNewLobbyButton;
        [SerializeField] private Button joinPrivateLobbyButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private LobbyListEntry lobbyListEntryPrefab;
        [SerializeField] private RectTransform entriesHolder;

        [SerializeField] private GameObject createLobbyPopup;
        [SerializeField] private GameObject joinPrivateLobbyPopup;

        private Dictionary<string, LobbyListEntry> entries;

        private void OnNewLobbyButtonClicked()
        {
            createLobbyPopup.SetActive(true);
        }

        private void OnJoinPrivateLobbyButtonClicked()
        {
            joinPrivateLobbyPopup.SetActive(true);
        }

        private void OnCloseButtonClicked()
        {
            SceneManager.LoadScene("MainMenu");
        }

        
        // TODO: Create, Update and Remove room entries
        //We are going to delete a room
        private void DeleteRoomEntry(RoomInfo roomInfo)
        {
            Destroy(entries[roomInfo.Name].gameObject);
            entries.Remove(roomInfo.Name);
        }


        //Checks if a room is unlisted by verifying that is public, that has at least one player and that is open for players.
        private bool IsRoomUnlisted(RoomInfo roomInfo) => !roomInfo.IsVisible || roomInfo.PlayerCount == 0 || !roomInfo.IsOpen;

        //Create a new lobby entry in the list
        private void AddNewLobbyEntry(RoomInfo roomInfo)
        {
            var entry = Instantiate(lobbyListEntryPrefab, entriesHolder);
            entry.Setup(roomInfo);
            entries.Add(roomInfo.Name, entry);
        }


        //This method gives you a list of everyroom that was created or destroyed or updated
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            //base.OnRoomListUpdate(roomList);
            Debug.LogWarning("Room list updated");

            foreach(RoomInfo roomInfo in roomList)
            {
                //Destroy the prefab of the lobby if it was removed
                if (roomInfo.RemovedFromList)
                {
                    DeleteRoomEntry(roomInfo);
                    continue;
                }

                //checks if the room updated is unlisted due to the different reasons for being unlisted, and deletes the prefab if it existed before
                if (IsRoomUnlisted(roomInfo))
                {
                    if (entries.ContainsKey(roomInfo.Name))
                    {
                        DeleteRoomEntry(roomInfo);

                        continue;
                    }
                }

                //Check if the room already exist in the lobby entries, updates the configuration with the new data
                if (entries.ContainsKey(roomInfo.Name))
                {
                    entries[roomInfo.Name].Setup(roomInfo);

                }
                else
                {
                    AddNewLobbyEntry(roomInfo);
                }
            }
        }
        public override void OnJoinedRoom()
        {
            //base.OnJoinedRoom();
            SceneManager.LoadScene("RoomLobby"); //Since we are also using this method in the main menu controller, its not present in this scene
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            GetComponentInChildren<JoinPrivateLobbyPopup>().privateRoomDoesntExist();
        }

        private void Start()
        {
            LoadingGraphics.Disable();

            entries = new Dictionary<string, LobbyListEntry>();

            closeButton.onClick.AddListener(OnCloseButtonClicked);
            createNewLobbyButton.onClick.AddListener(OnNewLobbyButtonClicked);
            joinPrivateLobbyButton.onClick.AddListener(OnJoinPrivateLobbyButtonClicked);

            DestroyHolderChildren();

            createLobbyPopup.SetActive(false);
            joinPrivateLobbyPopup.SetActive(false);

            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }

        private void DestroyHolderChildren()
        {
            for (var i = entriesHolder.childCount - 1; i >= 0; i--) {
                Destroy(entriesHolder.GetChild(i).gameObject);
            }
        }
    }
}