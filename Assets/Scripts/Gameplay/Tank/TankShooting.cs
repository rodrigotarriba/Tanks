﻿using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks
{
    public class TankShooting : MonoBehaviour
    {
        private const string FIRE_BUTTON = "Fire1";

        public Rigidbody shell;
        public Transform fireTransform;
        public Slider aimSlider;
        public AudioSource shootingAudio;
        public AudioClip chargingClip;
        public AudioClip fireClip;
        public float minLaunchForce = 15f;
        public float maxLaunchForce = 30f;
        public float maxChargeTime = 0.75f;

        private float currentLaunchForce;
        private float chargeSpeed;
        private bool fired;

        private PhotonView photonView;

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

                shootingAudio.clip = chargingClip;
                shootingAudio.Play();
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

        private void Fire()
        {
            fired = true;

            // TODO: Instantiate the projectile on all clients
            photonView.RPC(
                "Fire",
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
        private void Fire(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            Rigidbody shellInstance = Instantiate(shell, position, rotation);
            shellInstance.velocity = velocity;

            shootingAudio.clip = fireClip;
            shootingAudio.Play();
        }
    }



}