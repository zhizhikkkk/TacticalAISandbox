using UnityEngine;
using Sirenix.OdinInspector;

public class UnitAnimator : MonoBehaviour
{
    [Title("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Unit unit;

    [Title("Settings")]
    [SerializeField] private float smoothTime = 0.5f;

    private float _currentAnimSpeed;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private float targetSpeed;
    private float _currentVelocity;

    private void Awake()
    {
        if (unit == null) { 
           
             unit = GetComponent<Unit>();
        }
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }
    private void Start()
    {
        var health = GetComponentInParent<Health>();
        if (health != null)
        {
            health.OnDeath += HandleDeath;
        }
    }
    private void Update()
    {
        if (animator == null || unit == null) return;
        if (unit.IsMoving) Debug.Log("<color=green>Аниматор видит движение!</color>");
        targetSpeed = unit.IsMoving ? 1f : 0f;

        _currentAnimSpeed = animator.GetFloat(SpeedHash);
        float newSpeed = Mathf.SmoothDamp(_currentAnimSpeed, targetSpeed, ref _currentVelocity, smoothTime);
        animator.SetFloat(SpeedHash, newSpeed);

    }

    private void HandleDeath()
    {
        if (animator != null)
        {
            animator.SetBool("IsShooting", false);
            animator.SetTrigger("Death");

            animator.transform.localPosition = Vector3.zero;
        }
        this.enabled = false;
    }
}

