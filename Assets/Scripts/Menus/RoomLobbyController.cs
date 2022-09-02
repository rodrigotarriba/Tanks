using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Linq;

namespace Tanks
{

    //<Make sure that the class changes to inherit from MonoBehaviour Pun Callbacks
    public class RoomLobbyController : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private PlayerLobbyEntry playerLobbyEntryPrefab;
        [SerializeField] private RectTransform entriesHolder;

        /// <summary>
        /// Create and Delete player entries, a playerLobbyEntry is a prefab with their name and a PlayerLobbyEntry script
        /// </summary>
        private Dictionary<Player, PlayerLobbyEntry> lobbyEntries;

        //Check if every player is ready - if all of them are true, it will turn true
        private bool IsEveryPlayerReady => lobbyEntries.Values.ToList().TrueForAll(entry => entry.IsPlayerReady);

        private void AddLobbyEntry(Player player)
        {
            var entry = Instantiate(playerLobbyEntryPrefab, entriesHolder);
            entry.Setup(player);

            // TODO: track created player lobby entries
            lobbyEntries.Add(player, entry);
        }

        private void Start()
        {
            LoadingGraphics.Disable();
            DestroyHolderChildren();

            closeButton.onClick.AddListener(OnCloseButtonClicked);
            startButton.onClick.AddListener(OnStartButtonClicked);
            startButton.gameObject.SetActive(false);

            //Make sure all levels from the other players automatically set the scene to thee same one of the master client
            PhotonNetwork.AutomaticallySyncScene = true;

            //Creating the dict with the size of our max players
            lobbyEntries = new Dictionary<Player, PlayerLobbyEntry>(PhotonNetwork.CurrentRoom.MaxPlayers); 
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                AddLobbyEntry(player);
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            //override the base function and just make sure we can add a new player 
            AddLobbyEntry(newPlayer);

            //Every time a new player is added to the lobby, we can update the start button, which will turn on if its ready to start the game.
            UpdateStartButton();
        }
        

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            //Destroy the prefab holding that player, needs to destroy the gameObject since the variable points to the script
            Destroy(lobbyEntries[otherPlayer].gameObject);

            //Remove player from the dictionary
            lobbyEntries.Remove(otherPlayer);

            UpdateStartButton();

        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            //base.OnPlayerPropertiesUpdate(targetPlayer, changedProps); //overriding the properties
            lobbyEntries[targetPlayer].UpdateVisuals();

            UpdateStartButton();
        }


        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            //base.OnMasterClientSwitched(newMasterClient); //removing base function
            UpdateStartButton(); //the start button will 

        }


        /// <summary>
        /// Show start button only to the master client and when all players are ready
        /// </summary>
        private void UpdateStartButton()
        {
            //We only activate the start button if we are the masterclient and if every other player is ready
            startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && IsEveryPlayerReady);

        }


        /// <summary>
        /// Load gameplay level for all clients
        /// </summary>
        private void OnStartButtonClicked()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("You Fool! Trying to start game while not MasterClient");
                return;
            }

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("Gameplay"); //we still need to code what it can and cant do - we are not allowing anyone to enter anymore.


        }



        /// <summary>
        /// Leave room and go to MainMenu
        /// </summary>
        private void OnCloseButtonClicked()
        {


            //foreach(KeyValuePair<Player, PlayerLobbyEntry> thePlayer in lobbyEntries)
            //{
            //    Destroy(thePlayer.Value);
            //    lobbyEntries.Remove(thePlayer.Key);
            //}

            PhotonNetwork.LeaveRoom();

            // TODO: Leave room
            SceneManager.LoadScene("MainMenu");

        }

        private void DestroyHolderChildren()
        {
            for (var i = entriesHolder.childCount - 1; i >= 0; i--) {
                Destroy(entriesHolder.GetChild(i).gameObject);
            }
        }
    }
}