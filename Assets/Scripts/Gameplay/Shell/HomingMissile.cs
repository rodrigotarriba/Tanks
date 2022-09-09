using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Tanks;



public class HomingMissile : MonoBehaviour, IPunInstantiateMagicCallback
{
    [SerializeField]
    private Rigidbody missileRigidbody;
    [SerializeField]
    private float speed = 12;
    [SerializeField]
    private PhotonView photonView;
    [SerializeField]
    private ShellExplosion shellExplosion;

    private Rigidbody target;
    private int targetViewID;

    public void Awake()
    {
        photonView = GetComponent<PhotonView>();
        missileRigidbody = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log("it should be moving now");
        if (!photonView.IsMine)
        {
            return;
        }

        
        //Determine the target direction we are going
        var direction = (target.position - transform.position).normalized;
        
        //we empty the y direction since it does not have to go higher or lower
        direction.y = 0;

        //we use that direction as our transform forward
        transform.forward = direction;

        Vector3 movement = direction * speed * Time.deltaTime;
        missileRigidbody.MovePosition(missileRigidbody.position + movement);

        //while this is dynamic, it doesnt depend on client input anymore - Photon allows us to instantiate a prefab and pass the arguments with it. This is how we avoid an extra RPC


    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        //all the info using the photonnetwork.instantiate that was used in TankShooting
        object[] instantiateData = info.photonView.InstantiationData;

        //we created a data array in TankShooting, in which we added the PhotonView.ID component
        //we are retrieving the ID through the InstantiationData, which gives you back that array of data elements given to the PhotonNetwork.Instantiate
        targetViewID = (int)instantiateData[0];


        //We are finding the player with that targetView id
        //as a reminder, this is finding the photonview of the player that we selected through our raycast in TankShooting. 
        target = PhotonView.Find(targetViewID).GetComponent<Rigidbody>();

        //because the missile movement is dynamic and doesnt depend on input
        if (photonView.IsMine)
        {
            photonView.TransferOwnership(PhotonNetwork.MasterClient);
        }




    }
}
