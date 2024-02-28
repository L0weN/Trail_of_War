using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour, IDamageable
{
    private int _maxHealth = 100;
    private int _maxArmor = 50;
    [SerializeField] private NetworkVariable<int> _currentHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> _currentArmor = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public int MaxHealth { get => _maxHealth; private set => _maxHealth = value; }
    public int MaxArmor { get => _maxArmor; private set => _maxArmor = value; }
    public int CurrentHealth  { get => _currentHealth.Value; private set => _currentHealth.Value = value; }
    public int CurrentArmor { get => _currentArmor.Value; private set => _currentArmor.Value = value; }

    public event IDamageable.TakeDamageEvent OnTakeDamage;
    public event IDamageable.HealEvent OnHeal;
    public event IDamageable.ArmorUpEvent OnArmorUp;
    public event IDamageable.DeathEvent OnDeath;

    private void Start()
    {
        if (IsServer)
        {
            CurrentHealth = MaxHealth;
            CurrentArmor = MaxArmor;
        }
    }

    public void TakeDamage(int damage)
    {
        int damageTaken = Mathf.Clamp(damage, 0, CurrentHealth + CurrentArmor);
        
        if (CurrentArmor > 0)
        {
            CurrentArmor -= damageTaken;
            if (CurrentArmor < 0)
            {
                CurrentArmor = 0;
            }
        }
        else
        {
            CurrentHealth -= damageTaken;
        }

        if (damageTaken != 0)
        {
            OnTakeDamage?.Invoke(damageTaken);
        }

        if (CurrentHealth == 0 && damageTaken != 0)
        {
            OnDeath?.Invoke(transform.position);
        }
    }

    public void Heal(int heal)
    {
        CurrentHealth += heal;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

        if (heal != 0)
        {
            OnHeal?.Invoke(heal);
        }
    }

    public void ArmorUp(int armor)
    {
        CurrentArmor += armor;
        CurrentArmor = Mathf.Clamp(CurrentArmor, 0, MaxArmor);

        if (armor != 0)
        {
            OnArmorUp?.Invoke(armor);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealthServerRpc(int damage, ulong clientId)
    {
        var clientWithDamaged = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponentInChildren<IDamageable>();
        clientWithDamaged.TakeDamage(damage);

        NotifyHealthChangedClientRpc(damage, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        });
    }

    [ClientRpc]
    public void NotifyHealthChangedClientRpc(int damage, ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner) return;
        Debug.Log(damage + " ... " + clientRpcParams);
    }
}
