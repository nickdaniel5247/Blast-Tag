using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    private GameManager gameManager;
    private Animator animator;

    private AudioSource footsteps;
    public float footstepThreshold = 0.05f;

    [Rpc(SendTo.Everyone)]
    public void SetPlayerStatusRPC(bool enabled)
    {
        gameObject.SetActive(enabled);
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

    void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            gameManager.Tag(collision, gameObject);
        }
    }
}