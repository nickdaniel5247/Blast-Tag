using System.Linq;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    private GameManager gameManager;
    private Animator animator;
    private GameObject thirdPersonCamera;
    private GameObject spectatePos;

    private AudioSource footsteps;
    public float footstepThreshold = 0.05f;

    public AudioClip tagClip;

    [Rpc(SendTo.Everyone)]
    public void SetPlayerStatusRPC(bool alive)
    {
        gameObject.SetActive(alive);

        if (!IsOwner)
        {
            return;
        }

        //If we died, switch to spectator camera
        if (!alive)
        {
            thirdPersonCamera.SetActive(false);
            Camera.main.transform.position = spectatePos.transform.position;
            Camera.main.transform.rotation = spectatePos.transform.rotation;
        }
        else
        {
            thirdPersonCamera.SetActive(true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void HighlightPlayerRPC(bool enabled)
    {
        float value = enabled ? 0.01f : 0f;
        SkinnedMeshRenderer[] meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer meshRenderer in meshRenderers)
        {
            if (!meshRenderer.materials.Last().HasFloat("_Outline_thickness"))
            {
                continue;
            }

            meshRenderer.materials.Last().SetFloat("_Outline_thickness", value);
        }
    }

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        footsteps = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        thirdPersonCamera = GameObject.Find("Third Person Camera");
        spectatePos = GameObject.Find("Spectate Position");
    }

    private void Update()
    {
        Vector2 movementMagnitude;
        movementMagnitude.x = animator.GetFloat("Input X");
        movementMagnitude.y = animator.GetFloat("Input Y");

        if (movementMagnitude.magnitude > footstepThreshold)
        {
            if (!footsteps.isPlaying)
            {
                footsteps.Play();
            }
        }
        else 
        {
            footsteps.Stop();
        }
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

    [Rpc(SendTo.ClientsAndHost)]
    public void PlayTaggedSoundRPC()
    {
        AudioSource.PlayClipAtPoint(tagClip, transform.position);
    }

    //Attempt to tag other players when they collide with our detector
    void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            bool tagged = gameManager.Tag(collision, gameObject);

            if (tagged)
            {
                collision.GetComponent<PlayerManager>().PlayTaggedSoundRPC();
            }
        }
    }
}