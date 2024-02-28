using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerNetworkManager : NetworkBehaviour
{
    [SerializeField] private GameObject Camera;
    
    public override void OnNetworkSpawn()
    {
        if (IsOwner) return;
        Destroy(Camera);
        Destroy(GetComponentInChildren<PlayerInput>());
    }
}
