using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<ulong> currentChosen = new NetworkVariable<ulong>();
    public NetworkVariable<bool> playing = new NetworkVariable<bool>(false);

    public const float waitTime = 10f;

    private const float roundTime = 30f;
    private float currentRoundTime = 0f;
    private int alivePlayers = 0;

    private AudioSource cameraAudio;
    public AudioClip twentySecondTimer;
    public AudioClip win;

    private UIManager uiManager;

    private void Awake()
    {
        cameraAudio = GameObject.Find("Main Camera").GetComponent<AudioSource>();
        uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();
    }

    //Randomly choose next player
    private void ChoosePlayer()
    {
        //Get all alive players
        List<PlayerManager> alive = new List<PlayerManager>();

        foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).gameObject;

            if (!playerObject.activeSelf)
            {
                continue;
            }

            alive.Add(playerObject.GetComponent<PlayerManager>());
        }

        //Randomly choose one
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

    //Returns true if tag is valid
    public bool Tag(Collider tagged, GameObject tagger)
    {
        //Sanity check, we should only be server at this point
        if (!IsServer)
        {
            return false;
        }

        ulong taggerID = tagger.GetComponent<NetworkObject>().OwnerClientId;
        ulong taggedID = tagged.GetComponent<NetworkObject>().OwnerClientId;
        
        if (taggerID != currentChosen.Value)
        {
            return false;
        }

        currentChosen.Value = taggedID;
        tagger.GetComponent<PlayerManager>().HighlightPlayerRPC(false);
        tagged.GetComponent<PlayerManager>().HighlightPlayerRPC(true);

        return true;
    }

    //Someone dies and the next round begins
    private void EndRound()
    {
        currentRoundTime = 0;

        var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(currentChosen.Value).gameObject;
        playerObject.GetComponent<PlayerManager>().SetPlayerStatusRPC(false);
        playerObject.GetComponent<PlayerManager>().HighlightPlayerRPC(false);

        --alivePlayers;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayTimerRPC()
    {
        cameraAudio.PlayOneShot(twentySecondTimer);
    }

    IEnumerator WaitForCurrentAudio(AudioClip clip)
    {
        while (cameraAudio.isPlaying)
        {
            yield return null;
        }

        cameraAudio.PlayOneShot(clip);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayWinRPC()
    {
        StartCoroutine(WaitForCurrentAudio(win));
    }

    IEnumerator WaitThenPresentStart()
    {
        yield return new WaitForSeconds(waitTime);
        uiManager.PresentStartGame();
    }

    private void RestartGame()
    {
        StartCoroutine(WaitThenPresentStart());
    }

    public void Update()
    {
        if (!IsServer || !playing.Value)
        {
            return;
        }

        if (currentRoundTime > roundTime)
        {
            EndRound();

            if (alivePlayers > 1)
            {
                ChoosePlayer();
            }
        }

        if (currentRoundTime >= 10f && !cameraAudio.isPlaying)
        {
            PlayTimerRPC();
        }

        //All players died, last one standing won
        if (alivePlayers == 1)
        {
            playing.Value = false;
            PlayWinRPC();
            RestartGame();
            return;
        }

        currentRoundTime += Time.deltaTime;
    }
}