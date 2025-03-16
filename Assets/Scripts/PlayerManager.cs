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
        if (!IsOwner)
        {
            return;
        }

        //All are disabled by defualt except our own to save unnecessary computation
        GetComponentInChildren<BoxCollider>().enabled = true;
    }

    void OnTriggerEnter(Collider collision)
    {
        //Only continue if we are chosen
        if (gameManager.currentChosen.Value != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            Debug.Log("SEND RPC");
        }
    }
}
