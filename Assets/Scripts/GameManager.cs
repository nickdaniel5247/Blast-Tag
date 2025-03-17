using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<ulong> currentChosen = new NetworkVariable<ulong>();
    public NetworkVariable<bool> playing = new NetworkVariable<bool>(false);

    public float roundTime = 10f;
    private float currentRoundTime = 0f;

    //Synced as int to allow for less data transfers
    private NetworkVariable<int> currentRoundTimeSync = new NetworkVariable<int>();

    private int alivePlayers = 0;

    private void ChoosePlayer()
    {
        List<PlayerManager> alive = new List<PlayerManager>();

        foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).gameObject;
            alive.Add(playerObject.GetComponent<PlayerManager>());
        }

        int randomIdx = Random.Range(0, alive.Count);

        currentChosen.Value = alive[randomIdx].GetComponent<NetworkObject>().OwnerClientId;
        alive[randomIdx].HighlightPlayerRPC(true);
    }

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

        ChoosePlayer();

        //Reset these variables
        playing.Value = true;
        currentRoundTime = 0f;
    }

    public void Tag(Collider tagged, GameObject tagger)
    {
        //Sanity check, we should only be server at this point
        if (!IsServer)
        {
            return;
        }

        ulong taggerID = tagger.GetComponent<NetworkObject>().OwnerClientId;
        ulong taggedID = tagged.GetComponent<NetworkObject>().OwnerClientId;
        
        if (taggerID != currentChosen.Value)
        {
            return;
        }

        currentChosen.Value = taggedID;
        tagger.GetComponent<PlayerManager>().HighlightPlayerRPC(false);
        tagged.GetComponent<PlayerManager>().HighlightPlayerRPC(true);
    }

    private void EndRound()
    {
        var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(currentChosen.Value).gameObject;
        playerObject.GetComponent<PlayerManager>().SetPlayerStatusRPC(false);
        playerObject.GetComponent<PlayerManager>().HighlightPlayerRPC(false);

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
