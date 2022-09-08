using UnityEngine;
using UnityEngine.UI;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Tanks
{
    public class TankHealth : MonoBehaviour, IPunObservable, IOnEventCallback
    {
        //When this constant dies, this integer, its the integer that we are going to send when we send that event. we send the integer to the photon view and then raise the event.
        public const int TANK_DIED_PHOTON_EVENT = 0;

        
        public float startingHealth = 100f;
        public Slider slider;
        public Image fillImage;
        public Color fullHealthColor = Color.green;
        public Color zeroHealthColor = Color.red;
        public GameObject explosionPrefab;

        private AudioSource explosionAudio;
        private ParticleSystem explosionParticles;
        private float currentHealth;
        private bool dead;

        private PhotonView photonView;

        private void Awake()
        {
            explosionParticles = Instantiate(explosionPrefab).GetComponent<ParticleSystem>();
            explosionAudio = explosionParticles.GetComponent<AudioSource>();

            explosionParticles.gameObject.SetActive(false);

            //adding a reference to the photon view
            photonView = GetComponent<PhotonView>();
        }

        private void OnEnable()
        {
            //Here we need to add a callback to the Photon Network
            PhotonNetwork.AddCallbackTarget(this);
            
            currentHealth = startingHealth;
            dead = false;

            SetHealthUI();
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            SetHealthUI();

            //before we die we also need to ceheck if this photon view is ours
            if (currentHealth <= 0f && !dead && photonView.IsMine)
                OnDeath();
        }

        private void SetHealthUI()
        {
            slider.value = currentHealth;

            fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor, currentHealth / startingHealth);
        }

        private void OnDeath()
        {


            // TODO: Notify server that this tank died
            //receiver group would be everyone
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(TANK_DIED_PHOTON_EVENT, photonView.Owner, raiseEventOptions, SendOptions.SendReliable);

        }

        //Synchronize health across clients
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            
            //is writing is true is my client is adding data to the stream
            if (stream.IsWriting)
            {
                stream.SendNext(currentHealth);
                
            }
            else //isWriting is false if this is a receiver of data.
            {
                var newHealth = (float)stream.ReceiveNext();
                TakeDamage(currentHealth - newHealth);

            }
        }

        public void OnEvent(EventData photonEvent)
        {
            if(photonEvent.Code != TANK_DIED_PHOTON_EVENT)
            {
                return;
            }

            var player = (Player)photonEvent.CustomData;
            //Checking if the player of this evenbt is the photonViewer oiwner
            if(!Equals(photonView.Owner, player))
            {
                return;
            }
            
            //This is called when the tank actually dies, this is when we want to do all the fancy pancy particle stuff
            explosionParticles.transform.position = transform.position;
            explosionParticles.gameObject.SetActive(true);
            explosionParticles.Play();
            explosionAudio.Play();


            dead = true;
            gameObject.SetActive(false);

        }
    }
}