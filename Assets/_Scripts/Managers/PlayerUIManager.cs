using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    PlayerStats stats;
    PlayerGunSelector gun;

    [SerializeField] private TextMeshProUGUI AmmoText;
    [SerializeField] private Slider HealthSlider;
    [SerializeField] private Slider ArmorSlider;

    private void Start()
    {
        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in player)
        {
            if (p.GetComponentInParent<NetworkObject>().IsOwner)
            {
                stats = p.GetComponent<PlayerStats>();
                gun = p.GetComponent<PlayerGunSelector>();
            }
        }

        HealthSlider.maxValue = stats.MaxHealth;
        HealthSlider.value = stats.CurrentHealth;
        ArmorSlider.maxValue = stats.MaxArmor;
        ArmorSlider.value = stats.CurrentArmor;

        stats.OnTakeDamage += Damage;
        stats.OnHeal += Heal;
        stats.OnArmorUp += Armor;
    }

    private void Damage(int damage)
    {
        if (ArmorSlider.value > 0)
        {
            ArmorSlider.value -= damage;
        }
        else
        {
            HealthSlider.value -= damage;
        }
    }

    private void Heal(int heal)
    {
        HealthSlider.value += heal;
    }

    private void Armor(int armor)
    {
        ArmorSlider.value += armor;
    }

    private void Update()
    {
        if (gun.ActiveGun == null) return;
        UpdateAmmoText();
    }

    void UpdateAmmoText()
    {
        AmmoText.text = $"{gun.ActiveGun.AmmoConfig.CurrentClipAmmo} / {gun.ActiveGun.AmmoConfig.CurrentAmmo}";
    }

    
}
