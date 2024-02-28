using UnityEngine;
using System.Collections.Generic;

public interface ICollectible
{
    public CollectScriptableObject CollectibleData { get;}
    public void OnCollect(GameObject Player);
}
