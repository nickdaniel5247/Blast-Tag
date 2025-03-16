using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<ulong> currentChosen = new NetworkVariable<ulong>();
    public NetworkVariable<bool> playing = new NetworkVariable<bool>(false);

    public float roundTime = 10f;
    private float currentRoundTime = 0f;

    //Synced as int to allow for less data transfers
    private NetworkVariable<int> currentRoundTimeSync = new NetworkVariable<int>();

    private int alivePlayers = 0;

    public void StartGame()
    {
        if (!IsServer)
        {
            return;
        }

        //Count all the players
        alivePlayers = 0;

        //Make sure all players are enabled in case of previous rounds
        foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).gameObject;
            playerObject.GetComponent<PlayerManager>().SetPlayerStatusRPC(true);
            ++alivePlayers;
        }

        int randomIdx = Random.Range(0, NetworkManager.Singleton.ConnectedClientsIds.Count);
        ulong chosenClientID = NetworkManager.Singleton.ConnectedClientsIds[randomIdx];

        //Reset these variables
        currentChosen.Value = chosenClientID;
        playing.Value = true;
        currentRoundTime = 0f;
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

    private void EndRound()
    {
        var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(currentChosen.Value).gameObject;
        playerObject.GetComponent<PlayerManager>().SetPlayerStatusRPC(false);

        --alivePlayers;
    }

    public void Update()
    {
        if (!IsServer || !playing.Value)
        {
            return;
        }

        if (currentRoundTime > roundTime)
        {
            currentRoundTime = 0;
            EndRound();
        }

        if (alivePlayers == 1)
        {
            playing.Value = false;
            StartCoroutine(WaitToStartGame());
            return;
        }

        currentRoundTime += Time.deltaTime;
        currentRoundTimeSync.Value = (int)currentRoundTime;
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
