using UnityEngine;

public class ParticleSFX : MonoBehaviour
{
    public bool Repeating = true;
    public float RepeatTime = 2.0f;
    public float StartTime = 0.0f;
    public bool RandomVolume;
    public float minVolume = .4f;
    public float maxVolume = 1f;
    private AudioClip clip;

    [SerializeField] private AudioSource soundComponent;

    void Start()
    {
        //soundComponent = GetComponent<AudioSource>();
        clip = soundComponent.clip;
        if (RandomVolume == true)
        {
            soundComponent.volume = Random.Range(minVolume, maxVolume);
            //RepeatSound();
        }
        //if (Repeating == true)
        //{
        //    InvokeRepeating("RepeatSound", StartTime, RepeatTime);
        //}
    }

    public void PlaySFX()
    {
        RepeatSound();
    }

    void RepeatSound()
    {
        soundComponent.PlayOneShot(clip);
    }
}
