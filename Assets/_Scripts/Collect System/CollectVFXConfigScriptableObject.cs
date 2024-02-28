using UnityEngine;

[CreateAssetMenu(fileName = "Collectible VFX Config", menuName = "Collectibles/Collectible VFX Config", order = 2)]
public class CollectVFXConfigScriptableObject : ScriptableObject
{
    public GameObject spawnVFXPrefab;
    public GameObject collectVFXPrefab;

    public void SpawnVFX(Transform transform)
    {
        if (spawnVFXPrefab != null)
        {
            Instantiate(spawnVFXPrefab, transform.position, Quaternion.identity);
        }
    }

    public void CollectVFX(Transform transform)
    {
        if (collectVFXPrefab != null)
        {
            Instantiate(collectVFXPrefab, transform.position, Quaternion.identity);
        }
    }
}
