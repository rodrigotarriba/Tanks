using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tanks
{
    public class TankMovement : MonoBehaviour
    {
        private const string MOVEMENT_AXIS_NAME = "Vertical";
        private const string TURN_AXIS_NAME = "Horizontal";

        public float regularSpeed = 12f;
        public float turboSpeed = 24f;
        public float turnSpeed = 180f;
        public AudioSource movementAudio;
        public AudioClip engineIdling;
        public AudioClip engineDriving;
		public float pitchRange = 0.2f;

        private Rigidbody tankRigidbody;
        private float movementInputValue;
        private float turnInputValue;
        private float originalPitch;
        private ParticleSystem[] particleSystems;
        private PhotonView photonView;


        //Turbo state factors
        public ParticleSystem thrusterParticles;
        public float turboMaxTime;
        public float coolingMaxTime;
        private bool isTurboOn = false;
        private bool isCoolingDown = false;
        private float speed;
        private bool turboInput = false;


        public void GotHit(float explosionForce, Vector3 explosionSource, float explosionRadius)
        {
            tankRigidbody.AddExplosionForce(explosionForce, explosionSource, explosionRadius);
        }

        private void Awake()
        {
            //Get a reference to the photon view attached to this component, which will allow us to manipulate it directly using the commands that we are sending 
            photonView = GetComponent<PhotonView>();


            tankRigidbody = GetComponent<Rigidbody>();

            tankRigidbody.isKinematic = false;
        }

        private void OnEnable()
        {
            tankRigidbody.isKinematic = false;

            movementInputValue = 0f;
            turnInputValue = 0f;

            particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (var system in particleSystems) system.Play();
        }

        private void OnDisable()
        {
            tankRigidbody.isKinematic = true;

            foreach (var system in particleSystems) system.Stop();
        }

        private void Start()
        {
            originalPitch = movementAudio.pitch;
        }

        private void Update()
        {
            //Guard clause, only allow owner of this tank to move it
            if (!photonView.IsMine)
            {
                return;
            }

            movementInputValue = Input.GetAxis (MOVEMENT_AXIS_NAME);
            turnInputValue = Input.GetAxis (TURN_AXIS_NAME);

            if (Input.GetKeyDown(KeyCode.T))
            {
                TurboCheck();
            }

            EngineAudio();
        }

        private void EngineAudio()
        {
            // If there is no input (the tank is stationary)...
            if (Mathf.Abs (movementInputValue) < 0.1f && Mathf.Abs (turnInputValue) < 0.1f)
            {
                // ... and if the audio source is currently playing the driving clip...
                if (movementAudio.clip == engineDriving)
                {
                    // ... change the clip to idling and play it.
                    movementAudio.clip = engineIdling;
                    movementAudio.pitch = Random.Range (originalPitch - pitchRange, originalPitch + pitchRange);
                    movementAudio.Play();
                }
            }
            else
            {
                // Otherwise if the tank is moving and if the idling clip is currently playing...
                if (movementAudio.clip == engineIdling)
                {
                    // ... change the clip to driving and play.
                    movementAudio.clip = engineDriving;
                    movementAudio.pitch = Random.Range(originalPitch - pitchRange, originalPitch + pitchRange);
                    movementAudio.Play();
                }
            }
        }

        private void FixedUpdate()
        {
            //Guard clause, only allow owner of this tank to move it
            if (!photonView.IsMine)
            {
                return;
            }


            Move();
            Turn();
        }

        // TODO: Synchronize position and rotation across clients

        private void Move()
        {
            if (isTurboOn) speed = turboSpeed;
            else speed = regularSpeed;
            
            Vector3 movement = transform.forward * movementInputValue * speed * Time.deltaTime;
            tankRigidbody.MovePosition(tankRigidbody.position + movement);
        }

        private void Turn()
        {
            float turn = turnInputValue * turnSpeed * Time.deltaTime;
            Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);

            tankRigidbody.MoveRotation(tankRigidbody.rotation * turnRotation);
        }

        private void TurboCheck()
        {
            if(isTurboOn || isCoolingDown)
            {
                return;
            }

            StartCoroutine(TurboMode());

            //check if turbo is off and cooldown is off

                   //turbo coroutine
                        //isturboon is true
                        //particles are on
                        //isturboon = false, particles off + speed regular after turbotimer
                        //cooldown on
                        //after cooldown timer, cooldown off   
        }



        private IEnumerator TurboMode()
        {
            yield return null;

            isTurboOn = true;
            thrusterParticles.gameObject.SetActive(true);

            photonView.RPC(
                "TurnThrustersOn",
                RpcTarget.All,
                true
                );

            var turboTimer = turboMaxTime;

            while(turboTimer >= 0f)
            {
                yield return new WaitForEndOfFrame();
            }

            

            //Command all clients to turn off the particles
            photonView.RPC(
                "TurnThrustersOn",
                RpcTarget.All,
                true
                );

            //turn off turbo mode, turn on cooling mode
            isTurboOn = false;
            isCoolingDown = true;
            var coolingTimer = coolingMaxTime;

            while(coolingTimer >= 0f)
            {
                yield return new WaitForEndOfFrame();
            }

            isCoolingDown = false;
           
        }


        [PunRPC]
        public void TurnThrustersOn(bool thrustersState)
        {
            thrusterParticles.gameObject.SetActive(thrustersState);
        }

    }
}