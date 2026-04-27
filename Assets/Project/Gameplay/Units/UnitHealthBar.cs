using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class UnitHealthBar : MonoBehaviour
{
    [Title("References")]
    [SerializeField] private MeshRenderer fillRenderer;
    [SerializeField] private Transform barRoot;

    [Title("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.2f, 0f);
    [SerializeField] private Color fullColor = Color.green;
    [SerializeField] private Color emptyColor = Color.red;

    private Health _health;
    private Transform _cam;
    private MaterialPropertyBlock _mpb;
    private Transform _fillTransform;

    private static readonly int ColorProp = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        _cam = Camera.main.transform;
        _fillTransform = fillRenderer.transform;

        _health = GetComponentInParent<Health>();
        if (_health != null)
        {
            _health.OnTakeDamage += _ => UpdateBar();
            _health.OnDeath += () => gameObject.SetActive(false);
        }

    }
    private void Start()
    {
        UpdateBar(); 
    }

    private void LateUpdate()
    {
        barRoot.position = _health.transform.position + offset;
        barRoot.LookAt(barRoot.position + _cam.forward);
    }

    private void UpdateBar()
    {
        if (_health == null) return;

        float fraction = _health.CurrentHealth / _health.MaxHealth;

        Vector3 scale = _fillTransform.localScale;
        scale.x = Mathf.Clamp01(fraction);
        _fillTransform.localScale = scale;

        Vector3 pos = _fillTransform.localPosition;
        pos.x = (fraction - 1f) * 0.5f;
        _fillTransform.localPosition = pos;

        Color barColor = Color.Lerp(emptyColor, fullColor, fraction);
        fillRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(ColorProp, barColor);
        fillRenderer.SetPropertyBlock(_mpb);
    }
}