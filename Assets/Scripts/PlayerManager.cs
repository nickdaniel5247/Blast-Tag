using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [Rpc(SendTo.Everyone)]
    public void SetPlayerStatusRPC(bool enabled)
    {
        gameObject.SetActive(enabled);
    }
}
