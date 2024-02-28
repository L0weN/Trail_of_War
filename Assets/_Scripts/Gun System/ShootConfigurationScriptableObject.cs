using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Shoot Config", menuName = "Guns/Shoot Configuration", order = 2)]
public class ShootConfigurationScriptableObject : ScriptableObject
{
    public bool IsHitscan = true;
    public Bullet BulletPrefab;
    public float BulletSpawnForce = 1000f;

    public LayerMask HitMask;

    public float FireRate = 0.25f;

    public ShootType ShootType = ShootType.FromGun;

    public float RecoilRecoverySpeed = 1f;
    public float MaxSpreadTime = 1f;

    public BulletSpreadType SpreadType = BulletSpreadType.Simple;

    [Header("Simple Spread")]
    public Vector3 Spread = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("Texture-Based Spread")]
    [Range(0.001f, 5f)]
    public float SpreadMultiplier = 0.1f;
    public Texture2D SpreadTexture;

    public Vector3 GetSpread(float ShootTime = 0)
    {
        Vector3 spread = Vector3.zero;

        if(SpreadType == BulletSpreadType.Simple)
        {
            spread = Vector3.Lerp(
                Vector3.zero,
                new Vector3(Random.Range(-Spread.x, Spread.x), Random.Range(-Spread.y, Spread.y), Random.Range(-Spread.z, Spread.z)),
                Mathf.Clamp01(ShootTime / MaxSpreadTime));
        }
        else if (SpreadType == BulletSpreadType.TextureBased)
        {
            spread = GexTextureDirection(ShootTime);
            spread *= SpreadMultiplier;
        }

        return spread;
    }

    public Vector3 GexTextureDirection(float ShootTime)
    {
        Vector2 halfSize = new Vector2(SpreadTexture.width / 2f, SpreadTexture.height / 2f);
        int halfSquareExtents = Mathf.CeilToInt(Mathf.Lerp(1, halfSize.x, Mathf.Clamp01(ShootTime)));

        int minX = Mathf.FloorToInt(halfSize.x) - halfSquareExtents;
        int minY = Mathf.FloorToInt(halfSize.y) - halfSquareExtents;

        Color[] sampleColors = SpreadTexture.GetPixels(minX, minY, halfSquareExtents * 2, halfSquareExtents * 2);

        float[] colorsAsGrey = System.Array.ConvertAll(sampleColors, (color) => color.grayscale);
        float totalGreyValue = colorsAsGrey.Sum();

        float grey = Random.Range(0, totalGreyValue);
        int i = 0;
        for (; i < colorsAsGrey.Length; i++)
        {
            grey -= colorsAsGrey[i];
            if (grey <= 0)
            {
                break;
            }
        }

        int x = minX + i % (halfSquareExtents * 2);
        int y = minY + i / (halfSquareExtents * 2);

        Vector2 targetPosition = new Vector2(x, y);
        Vector2 direction = (targetPosition - halfSize) / halfSize.x;

        return direction;
    }
}
