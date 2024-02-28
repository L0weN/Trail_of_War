using UnityEngine;

[CreateAssetMenu(fileName = "Collect Config", menuName = "Collectibles/Collectibles", order = 1)]
public class CollectScriptableObject : ScriptableObject
{
    public CollectType Type;
    [Range(0, 25)] public int Value;
    public CollectAudioConfigScriptableObject AudioConfig;
    public CollectVFXConfigScriptableObject VFXConfig;
    public GameObject CollectiblePrefab;

    private GameObject Collectible;
    private AudioSource AudioSource;

    public void Spawn(Transform Parent)
    {
        if (CollectiblePrefab != null)
        {
            Collectible = Instantiate(CollectiblePrefab, Parent);
            AudioSource = Collectible.GetComponentInParent<AudioSource>();
            if (AudioConfig != null)
            {
                AudioConfig.PlaySpawnClip(AudioSource);
            }
        }
    }

    public void Collect(GameObject Player)
    {
        switch (Type)
        {
            case CollectType.Health:
                CollectHealth(Player);
                break;
            case CollectType.Armor:
                CollectArmor(Player);
                break;
            case CollectType.Ammo:
                CollectAmmo(Player);
                break;
            case CollectType.Gun:
                CollectGun(Player);
                break;
            default:
                break;
        }
        if (AudioConfig != null)
        {
            AudioConfig.PlayCollectClip(AudioSource);
        }
    }

    private void CollectHealth(GameObject Player)
    {
        Player.GetComponent<PlayerStats>().Heal(Value);
    }

    private void CollectArmor(GameObject Player)
    {
        Player.GetComponent<PlayerStats>().ArmorUp(Value);
    }

    private void CollectAmmo(GameObject Player)
    {
        Player.GetComponent<PlayerGunSelector>().ActiveGun.AmmoConfig.CurrentAmmo += Value;
    }

    private void CollectGun(GameObject Player)
    {
        Debug.Log("Collected Gun");
    }
}
