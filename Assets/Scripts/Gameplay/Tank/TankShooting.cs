using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks
{

    public class TankShooting : MonoBehaviour, IPunObservable
    {
        private const string FIRE_BUTTON = "Fire1";
        private const string HOMING_MISSILE_BUTTON = "Fire2";

        public Rigidbody shell;
        public Transform fireTransform;
        public Slider aimSlider;
        public AudioSource shootingAudio;
        public AudioClip chargingClip;
        public AudioClip fireClip;
        public float minLaunchForce = 15f;
        public float maxLaunchForce = 30f;
        public float maxChargeTime = 0.75f;

        //float that determines offset from tank to instantiate the homind missile
        public float homingMissileInstantiateOffset = 4f;

        private float currentLaunchForce;
        private float chargeSpeed;
        private bool fired;

        private PhotonView photonView;

        private bool GetClickPosition(out Vector3 clickPosition)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            var gotHit = Physics.Raycast(ray, out var hit, 1000, LayerMask.GetMask("Default"));

            //another terniary operation
            //returns the method gotHit, if its true, it will give you hit.point
            //if its false, it will give you Vector3.zero (zero)
            clickPosition = gotHit ? hit.point : Vector3.zero;

            return gotHit;
        }

        private void OnEnable()
        {
            currentLaunchForce = minLaunchForce;
            aimSlider.value = minLaunchForce;
        }

        private void Start()
        {
            photonView = GetComponent<PhotonView>();
            chargeSpeed = (maxLaunchForce - minLaunchForce) / maxChargeTime;
        }

        private void Update()
        {
            // TODO: Only allow owner of this tank to shoot

            //Guard clause, only allow owner of this tank to shoot
            if (!photonView.IsMine)
            {
                return;
            }

            
            //Instead of having the GEtBUtton functions here, these are put in the next functions.
            TryFireMissile();
            TryFireHomingMissile();

        }


        private void TryFireMissile()
        {
            aimSlider.value = minLaunchForce;

            if (currentLaunchForce >= maxLaunchForce && !fired)
            {
                currentLaunchForce = maxLaunchForce;
                Fire();
            }
            else if (Input.GetButtonDown(FIRE_BUTTON))
            {
                fired = false;
                currentLaunchForce = minLaunchForce;

                //Charging feedback sent to all clients
                photonView.RPC( "BeginCharging", RpcTarget.All );
            }
            else if (Input.GetButton(FIRE_BUTTON) && !fired)
            {
                currentLaunchForce += chargeSpeed * Time.deltaTime;

                aimSlider.value = currentLaunchForce;

            }
            else if (Input.GetButtonUp(FIRE_BUTTON) && !fired)
            {
                Fire();
            }
        }

        private void TryFireHomingMissile()
        {
            if (!Input.GetButtonDown(HOMING_MISSILE_BUTTON))
            {
                return;
            }

            if(!GetClickPosition(out var clickposition))
            {
                return;
            }

            Collider[] colliders = Physics.OverlapSphere(clickposition, 5, LayerMask.GetMask("Players"));
            foreach(var tankCollider in colliders)
            {
                if(tankCollider.gameObject == gameObject)
                {
                    continue;
                }

                var direction = (tankCollider.transform.position - transform.position).normalized;
                var position = transform.position + direction * homingMissileInstantiateOffset + Vector3.up;

                object[] data = { tankCollider.GetComponent<PhotonView>().ViewID };

                PhotonNetwork.Instantiate(
                    nameof(HomingMissile),
                    position,
                    Quaternion.LookRotation(transform.forward),
                    0,
                    data);

            }

        }

        private void Fire()
        {
            fired = true;

            // TODO: Instantiate the projectile on all clients
            photonView.RPC(
                "FireMissile",
                RpcTarget.All,
                fireTransform.position, //these parameters are within the Fire method
                fireTransform.rotation,
                currentLaunchForce * fireTransform.forward

                );



            //Rigidbody shellInstance = Instantiate(shell, fireTransform.position, fireTransform.rotation);
            //shellInstance.velocity = currentLaunchForce * fireTransform.forward;

            //shootingAudio.clip = fireClip;
            //shootingAudio.Play();

            currentLaunchForce = minLaunchForce;
        }

        [PunRPC]
        private void BeginCharging()
        {
            shootingAudio.clip = chargingClip;
            shootingAudio.Play();
        }

        [PunRPC]
        private void UpdateSlider(float newSliderValue)
        {
            aimSlider.value = newSliderValue;
        }

        [PunRPC]
        private void FireMissile(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            Rigidbody shellInstance = Instantiate(shell, position, rotation);
            shellInstance.velocity = velocity;

            shootingAudio.clip = fireClip;
            shootingAudio.Play();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(aimSlider.value);
                Debug.Log("Is sending");
            }

            if (stream.IsReading)
            {
                aimSlider.value = (float)stream.ReceiveNext();
            }
        }
    }



}