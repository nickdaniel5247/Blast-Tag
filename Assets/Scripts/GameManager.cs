using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<ulong> currentChosen = new NetworkVariable<ulong>();
    public NetworkVariable<bool> playing = new NetworkVariable<bool>(false);

    public void StartGame()
    {
        if (!IsServer)
        {
            return;
        }

        //Make sure all players are enabled in case of previous rounds
        foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).gameObject;
            playerObject.GetComponent<PlayerManager>().SetPlayerStatusRPC(true);
        }

        int randomIdx = Random.Range(0, NetworkManager.Singleton.ConnectedClientsIds.Count);
        ulong chosenClientID = NetworkManager.Singleton.ConnectedClientsIds[randomIdx];

        currentChosen.Value = chosenClientID;
        playing.Value = true;
    }

    public void Tag(Collider collision, GameObject tagger)
    {
        //Sanity check, we should only be server at this point
        if (!IsServer)
        {
            return;
        }

        ulong taggerID = tagger.GetComponent<NetworkObject>().OwnerClientId;
        ulong taggedID = collision.GetComponent<NetworkObject>().OwnerClientId;
        
        if (taggerID != currentChosen.Value)
        {
            return;
        }

        currentChosen.Value = taggedID;
    }

    public void Update()
    {
        if (!playing.Value)
        {
            return;
        }
    }

    IEnumerator WaitToStartGame()
    {
        yield return new WaitForSeconds(5);
        StartGame();
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(WaitToStartGame());
    }
}
