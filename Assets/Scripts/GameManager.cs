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

        int randomIdx = Random.Range(0, NetworkManager.Singleton.SpawnManager.GetConnectedPlayers().Count);
        currentChosen.Value = NetworkManager.Singleton.SpawnManager.GetConnectedPlayers()[randomIdx];

        playing.Value = true;
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
