using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

namespace Tanks
{
    public class LobbyListEntry : MonoBehaviour
    {
        [SerializeField] private Button enterButton;
        [SerializeField] private TMP_Text lobbyNameText;
        [SerializeField] private TMP_Text lobbyPlayerCountText;

        private RoomInfo roomInfo;

        private void OnEnterButtonClick()
        {
            // TODO: Join target room
            LoadingGraphics.Enable();
            PhotonNetwork.JoinRoom(roomInfo.Name);
        }

        /// <summary>
        /// Setting up the room info - store and update.
        /// </summary>
        public void Setup(RoomInfo info)
        {
            roomInfo = info;

            lobbyNameText.text = roomInfo.Name;
            lobbyPlayerCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
        }

        private void Start()
        {
            enterButton.onClick.AddListener(OnEnterButtonClick);
        }
    }
}