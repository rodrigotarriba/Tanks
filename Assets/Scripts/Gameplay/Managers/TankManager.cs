using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

namespace Tanks
{
    public class TankManager : MonoBehaviour
    {
        private TeamConfig teamConfig;
        private TankMovement tankMovement;
        private TankShooting tankShooting;
        private TankHealth tankHealth;
        private GameObject canvasGameObject;
        private Player player;
        private PhotonView photonView;


        // TODO: Get player nickname
        public string ColoredPlayerName => $"<color=#{ColorUtility.ToHtmlStringRGB(teamConfig.color)}>Nickname</color>";
        public int Wins { get; set; }

        public void OnHit(float explosionForce, Vector3 explosionSource, float explosionRadius, float damage)
        {
            tankMovement.GotHit(explosionForce, explosionSource, explosionRadius);
            tankHealth.TakeDamage(damage);
        }

        public void Awake()
        {
            //Here he need to find, not only our photon view, but also our player!

            SetupComponents();

            //Get player
            //player = photonView.Owner;
            player = photonView.Owner;
            teamConfig = FindObjectOfType<GameManager>().RegisterTank(this, (int)player.CustomProperties["Team"]);

            SetupRenderers();
        }

        private void SetupComponents()
        {
            
            //here we need to grab the photon view
            photonView = GetComponent<PhotonView>(); //getting the photon view in this compoonent
            //Debug.Log($"{photonView.Owner}");


            tankShooting = GetComponent<TankShooting>();
            tankHealth = GetComponent<TankHealth>();
            tankMovement = GetComponent<TankMovement>();
            canvasGameObject = GetComponentInChildren<Canvas>().gameObject;
        }


        /// <summary>
        /// Changes all the children mesh renderers to the team configuration color
        /// </summary>
        private void SetupRenderers()
        {
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

            foreach (var meshRenderer in renderers)
                meshRenderer.material.color = teamConfig.color;
        }

        public void DisableControl()
        {
            tankMovement.enabled = false;
            tankShooting.enabled = false;

            canvasGameObject.SetActive(false);
        }

        public void EnableControl()
        {
            tankMovement.enabled = true;
            tankShooting.enabled = true;

            canvasGameObject.SetActive(true);
        }

        public void Reset()
        {
            transform.position = teamConfig.spawnPoint.position;
            transform.rotation = teamConfig.spawnPoint.rotation;

            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }
}