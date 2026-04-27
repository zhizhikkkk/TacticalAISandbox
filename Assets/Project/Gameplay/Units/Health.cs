using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private UnitFaction faction ;
    [SerializeField] private float maxHealth = 100f;
    [ReadOnly, ShowInInspector] private float _currentHealth;

    public UnityAction OnDeath;

    public UnityAction<Transform> OnTakeDamage;


    public bool IsDead { get; private set; }
    public UnitFaction Faction => faction;
    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, Transform attacker)
    {
        if (IsDead) return;

        _currentHealth -= amount;

        OnTakeDamage?.Invoke(attacker);

        Debug.Log($"{gameObject.name}: take damage {amount} from {attacker.name}. Remaining HP:{_currentHealth}");
        if (_currentHealth <= 0)
        {
            Die();
        }  
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;

        OnDeath?.Invoke();

        if (TryGetComponent<Unit>(out var unit))
        {
            unit.ReleaseCell();
            unit.enabled = false;
        }

        if (TryGetComponent<Animator>(out var anim))
        {
            anim.applyRootMotion = false;
        }

        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);

        Destroy(gameObject, 5f);
    }

}
