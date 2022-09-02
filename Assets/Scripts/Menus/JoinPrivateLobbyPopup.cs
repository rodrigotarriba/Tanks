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
        [SerializeField] private TextMeshProUGUI invalidPrivateName;

        private string failedRoomName = string.Empty;
 
        private void Start()
        {
            enterButton.onClick.AddListener(OnEnterClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);

            invalidPrivateName.enabled = false;
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
            // TODO: Join target room
            PhotonNetwork.JoinRoom(lobbyNameInput.text);
            
        }
        public void privateRoomDoesntExist()
        {
            Debug.Log("private room doesnt exist");
            failedRoomName = lobbyNameInput.text;
            invalidPrivateName.enabled = true;
        }


        public void Update()
        {
            if(failedRoomName == lobbyNameInput.text)
            {
                return;
            }

            invalidPrivateName.enabled = false;
        }

    }
}