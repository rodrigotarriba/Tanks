using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

namespace Tanks
{
    public class JoinPrivateLobbyPopup : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TMP_InputField lobbyNameInput;
        [SerializeField] private Button enterButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject invalidPrivateName;

        private string failedRoomName = string.Empty;
 
        private void Start()
        {
            enterButton.onClick.AddListener(OnEnterClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        public override void OnEnable()
        {
            lobbyNameInput.text = string.Empty;
            lobbyNameInput.Select();
            lobbyNameInput.ActivateInputField();
        }

        private void OnCloseButtonClicked()
        {
            gameObject.SetActive(false);
        }

        private void OnEnterClicked()
        {
            if (string.IsNullOrEmpty(lobbyNameInput.text)) return;
            LoadingGraphics.Enable();

            // TODO: Join target room

            PhotonNetwork.JoinRoom(lobbyNameInput.text);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            failedRoomName = lobbyNameInput.text;
            invalidPrivateName.SetActive(true);
        }

        public void Update()
        {
            if(failedRoomName == lobbyNameInput.text)
            {
                return;
            }

            invalidPrivateName.SetActive(false);
        }

    }
}