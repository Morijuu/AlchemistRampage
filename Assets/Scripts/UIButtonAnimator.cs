using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float hoverScale  = 1.12f;
    [SerializeField] private float pressScale  = 0.91f;
    [SerializeField] private float lerpSpeed   = 16f;

    private Vector3 baseScale;
    private float   targetScale = 1f;

    private void Awake()  => baseScale = transform.localScale;
    private void OnEnable() { targetScale = 1f; transform.localScale = baseScale; }

    private void Update()
    {
        Vector3 goal = baseScale * targetScale;
        transform.localScale = Vector3.Lerp(transform.localScale, goal, lerpSpeed * Time.unscaledDeltaTime);
    }

    public void OnPointerEnter(PointerEventData _) => targetScale = hoverScale;
    public void OnPointerExit (PointerEventData _) => targetScale = 1f;
    public void OnPointerDown (PointerEventData _) => targetScale = pressScale;
    public void OnPointerUp   (PointerEventData _) => targetScale = hoverScale;
}
