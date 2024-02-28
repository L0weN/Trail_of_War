using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject
{
    public ImpactType ImpactType;
    public GunType Type;
    public string Name;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;

    public DamageConfigScriptableObject DamageConfig;
    public AmmoConfigScriptableObject AmmoConfig;
    public ShootConfigurationScriptableObject ShootConfig;
    public TrailConfigScriptableObject TrailConfig;
    public AudioConfigScriptableObject AudioConfig;

    private MonoBehaviour ActiveMonoBehaviour;
    private GameObject Model;
    private Camera ActiveCamera;
    private AudioSource ShootingAudioSource;
    private float LastShootTime;
    private float InitialClickTime;
    private float StopShootingTime;
    private bool LastFrameWantedToShoot;
    private ParticleSystem ShootSystem;
    private ObjectPool<Bullet> BulletPool;
    private ObjectPool<TrailRenderer> TrailPool;

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour, Camera ActiveCamera = null)
    {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour;
        this.ActiveCamera = ActiveCamera;

        LastShootTime = 0;
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);
        if (!ShootConfig.IsHitscan)
        {
            BulletPool = new ObjectPool<Bullet>(CreateBullet);
        }

        Model = Instantiate(ModelPrefab, Parent);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);

        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
        ShootingAudioSource = Model.GetComponent<AudioSource>();
    }

    public void TryToShoot()
    {
        if (Time.time - LastShootTime - ShootConfig.FireRate > Time.deltaTime)
        {
            float lastDuration = Mathf.Clamp((StopShootingTime - InitialClickTime), 0, ShootConfig.MaxSpreadTime);
            float lerpTime = (ShootConfig.RecoilRecoverySpeed - (Time.time - StopShootingTime)) / ShootConfig.RecoilRecoverySpeed;

            InitialClickTime = Time.time - Mathf.Lerp(0, lastDuration, Mathf.Clamp01(lerpTime));
        }

        if (Time.time > ShootConfig.FireRate + LastShootTime)
        {
            LastShootTime = Time.time;
            if (AmmoConfig.CurrentClipAmmo == 0)
            {
                AudioConfig.PlayOutOfAmmoClip(ShootingAudioSource);
                return;
            }
            ShootSystem.Play();
            AudioConfig.PlayShootingClip(ShootingAudioSource, AmmoConfig.CurrentClipAmmo == 1);

            Vector3 spreadAmount = ShootConfig.GetSpread(Time.time - InitialClickTime);
            Model.transform.forward += Model.transform.TransformDirection(spreadAmount);

            Vector3 shootDirection = Vector3.zero;

            if (ShootConfig.ShootType == ShootType.FromGun)
            {
                shootDirection = ShootSystem.transform.forward;
            }
            else
            {
                shootDirection = ActiveCamera.transform.forward + ActiveCamera.transform.TransformDirection(shootDirection);
            }

            AmmoConfig.CurrentClipAmmo--;

            if (ShootConfig.IsHitscan)
            {
                DoHitscanShoot(shootDirection);
            }
            else
            {
                DoProjectileShoot(shootDirection);
            }

        }
    }

    private void DoHitscanShoot(Vector3 ShootDirection)
    {
        if (Physics.Raycast(GetRaycastOrigin(), ShootDirection, out RaycastHit hit, float.MaxValue, ShootConfig.HitMask))
        {
            ActiveMonoBehaviour.StartCoroutine(PlayTrail(ShootSystem.transform.position, hit.point, hit));
        }
        else
        {
            ActiveMonoBehaviour.StartCoroutine(PlayTrail(ShootSystem.transform.position, ShootSystem.transform.position + (ShootDirection * TrailConfig.MissDistance), new RaycastHit()));
        }
    }

    private void DoProjectileShoot(Vector3 ShootDirection)
    {
        Bullet bullet = BulletPool.Get();
        bullet.gameObject.SetActive(true);
        bullet.OnCollision += HandleBulletCollision;

        if (ShootConfig.ShootType == ShootType.FromCamera && Physics.Raycast(GetRaycastOrigin(), ShootDirection, out RaycastHit hit, float.MaxValue, ShootConfig.HitMask))
        {
            Vector3 directionToHit = (hit.point - ShootSystem.transform.position).normalized;
            Model.transform.forward = directionToHit;
            ShootDirection = directionToHit;
        }

        bullet.transform.position = ShootSystem.transform.position;
        bullet.Spawn(ShootDirection * ShootConfig.BulletSpawnForce);

        TrailRenderer trail = TrailPool.Get();
        if (trail != null)
        {
            trail.transform.SetParent(bullet.transform, false);
            trail.transform.localPosition = Vector3.zero;
            trail.emitting = true;
            trail.gameObject.SetActive(true);
        }
    }

    public bool CanReload()
    {
        return AmmoConfig.CanReload();
    }

    public void StartReloading()
    {
        AudioConfig.PlayReloadClip(ShootingAudioSource);
    }

    public void EndReload()
    {
        AmmoConfig.Reload();
    }

    public void UpdateCamera(Camera ActiveCamera)
    {
        this.ActiveCamera = ActiveCamera;
    }

    public void Tick(bool WantsToShoot)
    {
        Model.transform.localRotation = Quaternion.Lerp(Model.transform.localRotation, Quaternion.Euler(SpawnRotation), Time.deltaTime * ShootConfig.RecoilRecoverySpeed);

        if (WantsToShoot)
        {
            LastFrameWantedToShoot = true;
            TryToShoot();
        }
        else if (!WantsToShoot && LastFrameWantedToShoot)
        {
            StopShootingTime = Time.time;
            LastFrameWantedToShoot = false;
        }
    }

    public Vector3 GetRaycastOrigin()
    {
        Vector3 origin = ShootSystem.transform.position;

        if (ShootConfig.ShootType == ShootType.FromCamera)
        {
            origin = ActiveCamera.transform.position + ActiveCamera.transform.forward * Vector3.Distance(ActiveCamera.transform.position, ShootSystem.transform.position);
        }

        return origin;
    }

    public Vector3 GetGunForward()
    {
        return Model.transform.forward;
    }

    private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit)
    {
        TrailRenderer instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = StartPoint;
        yield return null;

        instance.emitting = true;

        float distance = Vector3.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(StartPoint, EndPoint, Mathf.Clamp01(1 - (remainingDistance / distance)));
            remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = EndPoint;

        if (Hit.collider != null)
        {
            var playerHit = Hit.transform.GetComponentInParent<NetworkObject>();
            if (playerHit != null)
            {
                var playerStats = playerHit.GetComponentInChildren<PlayerStats>();
                playerStats.UpdateHealthServerRpc(DamageConfig.GetDamage(distance), playerHit.OwnerClientId);
            }

            HandleBulletImpact(distance, EndPoint, Hit.normal, Hit.collider);
        }

        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        TrailPool.Release(instance);
    }

    private void HandleBulletCollision(Bullet Bullet, Collision Collision)
    {
        TrailRenderer trail = Bullet.GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            trail.transform.SetParent(null, true);
            ActiveMonoBehaviour.StartCoroutine(DelayedDisableTrail(trail));
        }

        Bullet.gameObject.SetActive(false);
        BulletPool.Release(Bullet);

        if (Collision != null)
        {
            ContactPoint contactPoint = Collision.GetContact(0);

            HandleBulletImpact(Vector3.Distance(contactPoint.point, Bullet.SpawnLocation), contactPoint.point, contactPoint.normal, contactPoint.otherCollider);
        }
    }

    private void HandleBulletImpact(float DistanceTraveled, Vector3 HitLocation, Vector3 HitNormal, Collider HitCollider)
    {
        SurfaceManager.Instance.HandleImpact(HitCollider.gameObject, HitLocation, HitNormal, ImpactType, 0);

        if (HitCollider.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(DamageConfig.GetDamage(DistanceTraveled));
        }
    }

    private IEnumerator DelayedDisableTrail(TrailRenderer Trail)
    {
        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        Trail.emitting = false;
        Trail.gameObject.SetActive(false);
        TrailPool.Release(Trail);
    }

    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = TrailConfig.Color;
        trail.material = TrailConfig.Material;
        trail.widthCurve = TrailConfig.WidthCurve;
        trail.time = TrailConfig.Duration;
        trail.minVertexDistance = TrailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }

    private Bullet CreateBullet()
    {
        return Instantiate(ShootConfig.BulletPrefab);
    }

    
}
