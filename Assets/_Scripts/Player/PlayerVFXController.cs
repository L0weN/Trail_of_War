using DG.Tweening;
using UnityEngine;

public class PlayerVFXController : MonoBehaviour
{
    PlayerStats player;
    [SerializeField] private Transform VFXTransform;
    [Space]
    [Header("Effects")]
    [SerializeField] private GameObject playerHealEffect;
    [SerializeField] private GameObject playerArmorEffect;
    [SerializeField] private GameObject playerDeathEffect;

    
    void Start()
    {
        player = GetComponent<PlayerStats>();

        player.OnHeal += HealEffect;
        player.OnDeath += DeathEffect;
        player.OnArmorUp += ArmorEffect;
    }
    private void HealEffect(int heal)
    {
        InstantiateEffect(playerHealEffect);
    }
    private void ArmorEffect(int obj)
    {
        InstantiateEffect(playerArmorEffect);
    }

    private void DeathEffect(Vector3 Position)
    {
        InstantiateEffect(playerDeathEffect);
    }

    private GameObject InstantiateEffect(GameObject effect)
    {
        GameObject effectInstance = Instantiate(effect, VFXTransform);
        Destroy(effectInstance, 2f);
        return effectInstance;
    }


    private void OnDestroy()
    {
        player.OnHeal -= HealEffect;
        player.OnDeath -= DeathEffect;
        player.OnArmorUp -= ArmorEffect;
    }
}
