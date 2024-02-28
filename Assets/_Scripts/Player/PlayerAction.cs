using Unity.Netcode;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAction : NetworkBehaviour
{
    [SerializeField] private PlayerGunSelector GunSelector;
    [SerializeField] private bool AutoReload = true;
    [SerializeField] private Animator PlayerAnimator;
    private Inputs _input;

    private bool isReloading = false;

    private int _animIDReload;

    private void Start()
    {
        _input = GetComponent<Inputs>();
        AssignAnimationIDs();
    }

    private void Update()
    {
        if (!IsOwner) return;
        Shoot();
        Reload();
    }

    private void Shoot()
    {
        if (GunSelector.ActiveGun != null)
        {
            GunSelector.ActiveGun.Tick(_input.shoot);
        }
    }

    private void Reload()
    {
        if (GunSelector.ActiveGun == null) return;
        if (ShouldManualReload() || ShouldAutoReload())
        {
            GunSelector.ActiveGun.StartReloading();
            isReloading = true;
            PlayerAnimator.SetTrigger(_animIDReload);
        }
    }

    private void EndReload()
    {
        if (!IsOwner) return;
        GunSelector.ActiveGun.EndReload();
        isReloading = false;
    }

    private bool ShouldManualReload()
    {
        return !isReloading && _input.reload && GunSelector.ActiveGun.CanReload();
    }

    private bool ShouldAutoReload()
    {
        return !isReloading && AutoReload && GunSelector.ActiveGun.AmmoConfig.CurrentClipAmmo == 0 && GunSelector.ActiveGun.CanReload();
    }

    void AssignAnimationIDs()
    {
        _animIDReload = Animator.StringToHash("Reload");
    }
}
