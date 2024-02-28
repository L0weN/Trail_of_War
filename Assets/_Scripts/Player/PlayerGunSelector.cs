using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerGunSelector : NetworkBehaviour
{
    [SerializeField] private Camera Camera;
    [SerializeField] private GunType Gun;
    [SerializeField] private Transform GunParent;
    [SerializeField] private List<GunScriptableObject> Guns;
    [SerializeField] private PlayerIK InverseKinematics;

    [Space]
    [Header("Runtime Filled")]
    public GunScriptableObject ActiveGun;

    private void Start()
    {
        SpawnGun();
    }

    private void SpawnGun()
    {
        GunScriptableObject gun = Guns.Find(gun => gun.Type == Gun);
        if (gun == null)
        {
            Debug.LogError($"Gun {Gun} not found in list");
            return;
        }

        if (IsOwner)
        {
            ActiveGun = gun;
            gun.Spawn(GunParent, this, Camera);
        }
        else
        {
            GameObject Gun = Instantiate(gun.ModelPrefab, GunParent);
            Gun.transform.localPosition = gun.SpawnPoint;
            Quaternion rot = Quaternion.Euler(gun.SpawnRotation.x, gun.SpawnRotation.y, gun.SpawnRotation.z);
            Gun.transform.localRotation = rot;
        }
        InverseKinematics.Setup(GunParent);
    }
}
