using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Tanks
{
    public class MainMenuController : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button lobbyButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsController settingsPopup;
        private Action pendingAction;


        private void Start()
        {
            // TODO: Connect to photon server

            //Guard clause, only connect if it hasnt been connected yet
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                //Connects using the Photon App ID from the website and everything we already set up.
                PhotonNetwork.ConnectUsingSettings();

            }
            
             

            //Here is how they assign listeners to the buttons using script instead of the hierarchy

            //This is how she wrote it first, in essence it makes sense, but it seems that adding a listener that has another action as an argument might be hard
            //>>>playButton.onClick.AddListener(OnConnectionDependentActionClicked(JoinRandomRoom));
            //This is the one she did with a lambda function.
            playButton.onClick.AddListener(() => OnConnectionDependentActionClicked(JoinRandomRoom));

            lobbyButton.onClick.AddListener(GoToLobbyList);
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);

            settingsPopup.gameObject.SetActive(false);
            settingsPopup.Setup();

            if (!PlayerPrefs.HasKey("PlayerName"))
                PlayerPrefs.SetString("PlayerName", "Player #" + Random.Range(0, 9999));
        }
         
        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            Debug.Log("Succesfully connected to Master");

            //Here we invoke the pending action, only if its valid (hence the question mark)
            pendingAction?.Invoke();
        }




        private void OnSettingsButtonClicked()
        {
            settingsPopup.gameObject.SetActive(true);
        }

        public void JoinRandomRoom()
        {
            // TODO: Connect to a random room
            //Describe some room options here, pas those options to when we want to create a random room
            RoomOptions roomOptions = new RoomOptions { IsOpen = true, MaxPlayers = 4 };
            PhotonNetwork.JoinRandomOrCreateRoom(roomOptions : roomOptions);


        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            
            //Whenever a player has joined a room, we will load a specific scene in the game
            SceneManager.LoadScene("RoomLobby"); 
        }

        //private void OnConnectionDependentActionClicked()
        //{
        //    return;
        //}

        private void OnConnectionDependentActionClicked(Action action)
        {
            if (pendingAction != null)
            {
                return;
            }

            pendingAction = action;

            //Check if the photon network is connected and ready
            if (PhotonNetwork.IsConnectedAndReady)
            {
                action();
            }
        }

        private void GoToLobbyList()
        {
            SceneManager.LoadSceneAsync("LobbyList");
        }
    }
}