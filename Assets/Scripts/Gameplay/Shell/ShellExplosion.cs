using Photon.Pun;
using UnityEngine;
using Photon.Realtime;

namespace Tanks
{
    public class ShellExplosion : MonoBehaviour
    {
        public LayerMask tankMask;
        public LayerMask shieldMask;
        public ParticleSystem explosionParticles;
        public AudioSource explosionAudio;
        public float maxDamage = 100f;
        public float explosionForce = 1000f;
        public float maxLifeTime = 2f;
        public float explosionRadius = 5f;

        private void Start()
        {
            Destroy(gameObject, maxLifeTime);
        }


        private void OnTriggerEnter(Collider other)
        {
            explosionParticles.transform.parent = null;
            explosionParticles.Play();
            explosionAudio.Play();

            ParticleSystem.MainModule mainModule = explosionParticles.main;
            Destroy(explosionParticles.gameObject, mainModule.duration);
            Destroy(gameObject);

            

            TryDamageTanks();
        }

        //private void TryDamageTanks()
        //{
        //    //If we are not the masterclient, do not attempt to recognize hits
        //    if (!PhotonNetwork.IsMasterClient)
        //    {
        //        return;
        //    }


        //    Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, tankMask);
        //    for (int i = 0; i < colliders.Length; i++)
        //    {
        //        var tankManager = colliders[i].GetComponent<TankManager>();
        //        if (tankManager == null) continue;

        //        Rigidbody targetRigidbody = tankManager.GetComponent<Rigidbody>();


        //        tankManager.photonView.RPC(
        //            "OnHit",
        //            RpcTarget.All,
        //            explosionForce,
        //            transform.position,
        //            explosionRadius,
        //            CalculateDamage(targetRigidbody.position));
        //    }
            
        //}


        private void TryDamageTanks()
        {
            //If we are not the masterclient, do not attempt to recognize hits
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, tankMask);
            for (int i = 0; i < colliders.Length; i++)
            {

                var photonView = colliders[i].GetComponent<PhotonView>();
                if (photonView == null) continue;

                var tankManager = colliders[i].GetComponent<TankManager>();
                if (tankManager == null) continue;

                Collider[] shieldColliders = Physics.OverlapSphere(transform.position, .5f, shieldMask);
                for (int x = 0; x < shieldColliders.Length; x++)
                {
                    
                    //var shieldPhotonView = shieldColliders[x].GetComponent<PhotonView>();
                    if (shieldColliders[x].gameObject.GetComponent<Transform>().parent.parent.gameObject == tankManager.gameObject)
                    {
                        return;
                    }
                }

                Debug.Log("this one doesnt reach");
                Rigidbody targetRigidbody = tankManager.GetComponent<Rigidbody>();



               tankManager.photonView.RPC(
                    "OnHit",
                    //RpcTarget.All,
                    photonView.Owner, //we are only damaging the client that is receiving the hit, perhaps for resources allocation?
                    explosionForce,
                    transform.position,
                    explosionRadius,
                    CalculateDamage(targetRigidbody.position));
            }
            //But perhaps we dont sync the health until later. 

        }

        private float CalculateDamage(Vector3 targetPosition)
        {
            Vector3 explosionToTarget = targetPosition - transform.position;

            float explosionDistance = explosionToTarget.magnitude;
            float relativeDistance = (explosionRadius - explosionDistance) / explosionRadius;
            float damage = relativeDistance * maxDamage;

            damage = Mathf.Max(0f, damage);

            return damage;
        }
    }
}