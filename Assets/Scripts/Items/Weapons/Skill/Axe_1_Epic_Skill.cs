using UnityEngine;

public class Axe_1_Epic_Skill : MonoBehaviour
{
    [SerializeField] ParticleSystem _skillEffect;

    float _duration;
    float _range = 6;
    float _skillStartTime;
    bool _skillStart = false;

    private void Start()
    {
        _duration = 1.35f;
    }

    public void OnSkillStart()
    {
        _skillEffect.Play();
        _skillStartTime = Time.time;
        _skillStart = true;
    }

    private void Update()
    {
        if (_skillStart && Time.time - _skillStartTime >= _duration)
        {
            CheckEnemyInRange();
        }
    }

    void CheckEnemyInRange()
    {
        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, _range, LayerData.Enemy);
        if (colliders.Length == 0) return;

        foreach(Collider collider in colliders)
        {
            if (!collider.GetComponent<Enemy>().IsSpawning)
                collider.GetComponent<HealthSystem>().TakeDamage(200, DamageType.Magic);
        }
        _skillStart = false;
    }

    private void OnParticleSystemStopped()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        _skillEffect.Stop();
        ParticleObjectPool.Instance.ReturnToPull(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _range);
    }
}
