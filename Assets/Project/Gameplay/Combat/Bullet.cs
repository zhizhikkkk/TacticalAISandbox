using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private TrailRenderer trail;

    private Transform _target;
    private BulletPool _pool;
    private float _lifeTime;
    private const float MaxLifeTime = 3f;

    public void Init(Transform target, BulletPool pool)
    {
        _target = target;
        _pool = pool;
        _lifeTime = 0f;

        if (trail != null) trail.Clear();
    }

    private void Update()
    {

        if (_target == null || _target.gameObject == null)
        {
            ReturnToPool();
            return;
        }

        _lifeTime += Time.deltaTime;
        if (_lifeTime >= MaxLifeTime)
        {
            ReturnToPool();
            return;
        }

        Vector3 dir = (_target.position - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, _target.position, speed * Time.deltaTime);

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        if((transform.position - _target.position).sqrMagnitude < 0.15f * 0.15f)
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        _target = null;

        _pool?.Return(this);
    }
}
