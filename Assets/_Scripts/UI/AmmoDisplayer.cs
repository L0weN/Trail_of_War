using TMPro;
using Unity.Netcode;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class AmmoDisplayer : NetworkBehaviour
{
    private GameObject[] Player;
    [SerializeField] private PlayerGunSelector GunSelector;
    private TextMeshProUGUI AmmoText;

    private void Awake()
    {
        AmmoText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        UpdateAmmoText();
    }

    private void UpdateAmmoText()
    {
        AmmoText.SetText($"{GunSelector.ActiveGun.AmmoConfig.CurrentClipAmmo} / {GunSelector.ActiveGun.AmmoConfig.CurrentAmmo}");
    }

    public override void OnNetworkSpawn()
    {
        Player = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in Player)
        {
            if (player.GetComponentInParent<NetworkObject>().IsOwner)
            {
                GunSelector = player.GetComponent<PlayerGunSelector>();
            }
        }
    }
}
