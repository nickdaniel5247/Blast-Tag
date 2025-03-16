using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    private GameManager gameManager;

    [Rpc(SendTo.Everyone)]
    public void SetPlayerStatusRPC(bool enabled)
    {
        gameObject.SetActive(enabled);
    }

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        //Detectors are disabled on clients as detection is server sided
        GetComponentInChildren<BoxCollider>().enabled = true;
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            gameManager.Tag(collision, gameObject);
        }
    }
}