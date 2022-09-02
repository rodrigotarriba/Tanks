using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using ExitGames.Client.Photon;

namespace Tanks
{
    public class PlayerLobbyEntry : MonoBehaviour
    {
        [SerializeField] private Button readyButton;
        [SerializeField] private GameObject readyText;
        [SerializeField] private Button waitingButton;
        [SerializeField] private GameObject waitingText;

        [SerializeField] private TMP_Text playerName;
        [SerializeField] private Button changeTeamButton;
        [SerializeField] private Image teamHolder;
        [SerializeField] private List<Sprite> teamBackgrounds;

        //we create a player variable that determines which player this is
        private Player player;

        /// <summary>
        /// We are updating the player team property, setting and getting a hashtable
        /// </summary>
        public int PlayerTeam 
        { 
            get => player.CustomProperties.ContainsKey("Team") ? (int)player.CustomProperties["Team"] : 0;
            set
            {
                //we are going to create a new photon hashtable
                ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable { { "Team", value } };
                player.SetCustomProperties(hash);
            }
        
        }  // TODO: Update player team to other clients



        /// <summary>
        /// We are updating the player ready status to other clients
        /// </summary>
        public bool IsPlayerReady 
        {
            get => player.CustomProperties.ContainsKey("IsReady") && (bool)player.CustomProperties["IsReady"];
            set
            {
                ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable { { "IsReady", value } };
                player.SetCustomProperties(hash);
            }
        
        } 

        /// <summary>
        /// Get if this entry belongs to the local player
        /// </summary>
        private bool IsLocalPlayer => Equals(player, PhotonNetwork.LocalPlayer); 


        //Each player is identified with a player id number
        public void Setup(Player entryPlayer)
        {
            // TODO: Store and update player information
            player = entryPlayer;


            //If we are the local player, designate the playerTeam int variable
            if (IsLocalPlayer)
            {
                PlayerTeam = (player.ActorNumber - 1) % PhotonNetwork.CurrentRoom.MaxPlayers;
                //doing operations to see f there are maximum players
            }

            //assing the photon nickname in server to the playername text that shows in unity
            playerName.text = player.NickName;

            if (!IsLocalPlayer)
                Destroy(changeTeamButton);

            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            teamHolder.sprite = teamBackgrounds[PlayerTeam];

            waitingText.SetActive(!IsPlayerReady);
            readyText.SetActive(IsPlayerReady);
        }

        private void Start()
        {
            waitingButton.onClick.AddListener(() => OnReadyButtonClick(true));
            readyButton.onClick.AddListener(() => OnReadyButtonClick(false));
            changeTeamButton.onClick.AddListener(OnChangeTeamButtonClicked);

            waitingButton.gameObject.SetActive(IsLocalPlayer);
            readyButton.gameObject.SetActive(false);
        }

        private void OnChangeTeamButtonClicked()
        {
            // TODO: Change player team
            //Because maybe we want to change teams, we create a player team.
            PlayerTeam = (PlayerTeam + 1) % PhotonNetwork.CurrentRoom.MaxPlayers;

        }

        private void OnReadyButtonClick(bool isReady)
        {
            waitingButton.gameObject.SetActive(!isReady);
            waitingText.SetActive(!isReady);
            readyButton.gameObject.SetActive(isReady);
            readyText.SetActive(isReady);

            IsPlayerReady = isReady;
        }
    }
}