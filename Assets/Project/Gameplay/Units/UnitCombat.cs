using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class UnitCombat : MonoBehaviour
{
    [Title("Settings")]
    [SerializeField] private float attackRange = 7f;
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float rotationSpeed = 15f;

    [Title("References")]
    [SerializeField] private Unit unit;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform muzzlePoint;
    

    private Health _currentTarget;
    private float _lastAttackTime;
    private Health _myHealth;
    private BulletPool _bulletPool;
    private static readonly int IsShootingHash = Animator.StringToHash("IsShooting");

    public bool HasTarget => _currentTarget != null && !_currentTarget.IsDead;
    public Health CurrentTarget => _currentTarget;

    [Inject]
    public void Construct(BulletPool bulletPool)
    {
        _bulletPool = bulletPool;
    }

    private void Awake()
    {
        if (unit == null) unit = GetComponent<Unit>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        _myHealth = GetComponent<Health>();
        if (_myHealth != null)
        {
            _myHealth.OnTakeDamage += (attacker) =>
            {
                if (_currentTarget == null && attacker != null)
                {
                    _currentTarget = attacker.GetComponentInParent<Health>();
                }
            };

            _myHealth.OnDeath += () =>
            {
                if (animator != null) animator.SetBool(IsShootingHash, false);
                this.enabled = false;
            };
        }
    }

    private void Update()
    {
        if (unit.IsMoving)
        {
            StopShooting();
            _currentTarget = null;
            return;
        }

        FindTarget();
        HandleCombat();
    }

    private void FindTarget()
    {
        if (HasTarget) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            if (hit.transform.root == transform.root) continue;

            if (hit.GetComponentInParent<Health>() is Health potentialTarget)
            {
                if (potentialTarget.Faction != unit.Faction && !potentialTarget.IsDead)
                {
                    _currentTarget = potentialTarget;
                    Debug.Log($"<color=cyan>{gameObject.name}</color> нашел цель: {potentialTarget.name}");
                    break;
                }
            }
        }
    }

    private void HandleCombat()
    {
        if (!HasTarget)
        {
            StopShooting();
            return;
        }

        float distance = Vector3.Distance(transform.position, _currentTarget.transform.position);

        if (distance <= attackRange)
        {
            RotateTowardsTarget();

            if (animator != null) animator.SetBool(IsShootingHash, true);

            if (Time.time >= _lastAttackTime + attackRate)
            {
                ExecuteAttack();
            }
        }
        else
        {
            StopShooting();
            _currentTarget = null;
        }
    }

    private void ExecuteAttack()
    {
        _lastAttackTime = Time.time;
        if (_currentTarget == null) return;
        
        _currentTarget.TakeDamage(damage, transform);
        Debug.Log($"_bulletPool: {_bulletPool}, muzzlePoint :{muzzlePoint}");
        if (_bulletPool != null && muzzlePoint != null) {
            Bullet bullet = _bulletPool.Get(muzzlePoint.position, muzzlePoint.rotation);
            bullet.Init(_currentTarget.transform, _bulletPool);
            bullet.gameObject.SetActive(true);
            Debug.Log($"Пуля выпущена!");
        }
        
    }

    private void RotateTowardsTarget()
    {
        Vector3 dir = (_currentTarget.transform.position - transform.position).normalized;
        dir.y = 0; 

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * 100f * Time.deltaTime);
        }
    }

    private void StopShooting()
    {
        if (animator != null) animator.SetBool(IsShootingHash, false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}