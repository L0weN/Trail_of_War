using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;


public class Collector : MonoBehaviour, ICollectible
{
    public CollectScriptableObject CollectibleData { get => Collectibles[Random.Range(0, Collectibles.Count)]; }

    public List<CollectScriptableObject> Collectibles;

    private void Start()
    {
        CollectibleData.Spawn(transform);
    }

    public void OnCollect(GameObject Player)
    {
        CollectibleData.Collect(Player);
        transform.DOScale(Vector3.zero, 1f).OnComplete(() => gameObject.SetActive(false));
    }
}
