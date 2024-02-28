using UnityEngine;

[CreateAssetMenu(fileName = "Collectible Audio Config", menuName = "Collectibles/Collectible Audio Config", order = 5)]
public class CollectAudioConfigScriptableObject : ScriptableObject
{
    [Range(0, 1f)] public float Volume = 1f;
    public AudioClip SpawnClip;
    public AudioClip CollectClip;

    public void PlaySpawnClip(AudioSource AudioSource)
    {
        if (SpawnClip != null)
        {
            AudioSource.PlayOneShot(SpawnClip, Volume);
        }
    }

    public void PlayCollectClip(AudioSource AudioSource)
    {
        if (CollectClip != null)
        {
            AudioSource.PlayOneShot(CollectClip, Volume);
        }
    }
}
