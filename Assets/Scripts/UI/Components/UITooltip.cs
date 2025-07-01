

using TMPro;
using UnityEngine;

public class UITooltip : MonoBehaviour
{
    [field: SerializeField] public RectTransform RectTransform { get; private set; }
    [field: SerializeField][Range(0, 1)] public float HoverDelay { get; private set; } = 0.5f;
    [SerializeField] private TextMeshProUGUI text;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void SetText(string t)
    {
        text.SetText(t);
        Vector2 preferredSize = text.GetPreferredValues();
        RectTransform.sizeDelta = new Vector2(preferredSize.x + 50, RectTransform.sizeDelta.y);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}