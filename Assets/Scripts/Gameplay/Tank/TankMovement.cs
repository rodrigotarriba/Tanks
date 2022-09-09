using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tanks
{
    public class TankMovement : MonoBehaviour, IPunObservable
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

        //shield rotation
        public Transform shieldAnchor;
        public float shieldRotation;
        public float shieldLoopSpeed = 3f;

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

            //Default tank rotation
            shieldRotation = -90f;
            shieldAnchor.rotation = Quaternion.Euler(new Vector3(0f, shieldRotation, 0f));
        }

        private void OnEnable()
        {
            tankRigidbody.isKinematic = false;

            movementInputValue = 0f;
            turnInputValue = 0f;

            particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (var system in particleSystems) system.Play();

            thrusterParticles.gameObject.SetActive(false);
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
            shieldAnchor.rotation = Quaternion.Euler(new Vector3(0f, shieldRotation, 0f));
            
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

            if (Input.GetKey(KeyCode.X))
            {
                shieldRotation += 360/shieldLoopSpeed * Time.deltaTime;
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

            photonView.RPC(
                "TurnThrustersOn",
                RpcTarget.All,
                true
                );

            var turboTimer = turboMaxTime;

            while(turboTimer >= 0f)
            {
                turboTimer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            

            //Command all clients to turn off the particles
            photonView.RPC(
                "TurnThrustersOn",
                RpcTarget.All,
                false
                );

            //turn off turbo mode, turn on cooling mode
            isTurboOn = false;
            isCoolingDown = true;
            var coolingTimer = coolingMaxTime;

            while(coolingTimer >= 0f)
            {
                coolingTimer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            isCoolingDown = false;
           
        }


        [PunRPC]
        private void TurnThrustersOn(bool thrustersState)
        {
            Debug.Log("turn thrusters on activated");
            thrusterParticles.gameObject.SetActive(thrustersState);

            if (thrustersState)
            {
                thrusterParticles.Play();
            }
            else
            {
                thrusterParticles.Stop();
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(shieldRotation); 
            }

            if (stream.IsReading)
            {
                shieldRotation = (float)stream.ReceiveNext();
            }
        }


    }
}

//FROM NAKISAS'SOLUTION - TERNIARY OPERATORS THAT CHECK STATUS
//private bool CanUseTurbo => remainingTurboCooldown <= 0;
//private bool IsTurboActive => remainingTurboDuration > 0;
////Define the current speed depending on whether IsTurboActive returns true, if so, current speed equals turbo speed, if not, equals speed.
//private float CurrentSpeed => IsTurboActive ? turboSpeed : speed;

//[PunRPC]
//private void Turbo()
//{
//    remainingTurboDuration = turboDuration;
//    turboParticles.Play();
//}

//private void UpdateTurbo()
//{
//    // decrease remaining turbo duration
//    remainingTurboDuration -= Time.deltaTime;

//    // check to see if the turbo is active and if the particles are playing, if they are stop the particles, turbo has stopped
//    if (!IsTurboActive && turboParticles.isPlaying)
//    {
//        turboParticles.Stop();
//    }
//}

//private void TryUseTurbo()
//{
//    // decrease cooldown
//    remainingTurboCooldown -= Time.deltaTime;

//    // if we can't use the turbo and the turbo button isn't down, don't do anything
//    if (!CanUseTurbo || !Input.GetButtonDown(TURBO_BUTTON))
//    {
//        return;
//    }

//    // update the turbo cooldown
//    remainingTurboCooldown = turboCooldown;

//    // call the Turbo RPC on all clients
//    photonView.RPC("Turbo", RpcTarget.All);
