using UnityEngine;
using Sirenix.OdinInspector;

public class SelectionBoxVisualizer : MonoBehaviour
{
    [SerializeField, Required] private RectTransform selectionBoxVisual;
    private Canvas _canvas;

    private void Awake()
    {
        _canvas = selectionBoxVisual.GetComponentInParent<Canvas>();
        Hide();
    }

    public void Show(Vector2 startPos, Vector2 currentPos)
    {
        if (!selectionBoxVisual.gameObject.activeSelf)
            selectionBoxVisual.gameObject.SetActive(true);

        float minX = Mathf.Min(startPos.x, currentPos.x);
        float maxX = Mathf.Max(startPos.x, currentPos.x);
        float minY = Mathf.Min(startPos.y, currentPos.y);
        float maxY = Mathf.Max(startPos.y, currentPos.y);

        float scaleFactor = _canvas.scaleFactor;
        selectionBoxVisual.anchoredPosition = new Vector2(minX, minY) / scaleFactor;
        selectionBoxVisual.sizeDelta = new Vector2(maxX - minX, maxY - minY) / scaleFactor;
    }

    public void Hide() => selectionBoxVisual.gameObject.SetActive(false);
}