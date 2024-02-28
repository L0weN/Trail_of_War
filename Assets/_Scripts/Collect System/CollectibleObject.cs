using UnityEngine;
using DG.Tweening;

public class CollectibleObject : MonoBehaviour
{
    private float moveDistance = 1f; 
    private float moveDuration = 1f; 
    private float rotateDuration = 1f;

    void Start()
    {
        Animate();
    }

    private void Animate()
    {
        transform.position = new Vector3(transform.position.x, .25f, transform.position.z);

        transform.DOMoveY(moveDistance, moveDuration)
            .SetLoops(-1, LoopType.Yoyo);

        transform.DORotate(new Vector3(0f, 360f, 0f), rotateDuration, RotateMode.FastBeyond360)
            .SetLoops(-1)
            .SetEase(Ease.InOutSine);
    }

    private void OnTriggerEnter(Collider other)
    {
        ICollectible collectible = GetComponentInParent<ICollectible>();
        if (collectible != null)
        {
            collectible.OnCollect(other.gameObject);
        }
        transform.DOKill();
        Destroy(gameObject);
    }
}
