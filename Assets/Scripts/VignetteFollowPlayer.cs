using UnityEngine;
using UnityEngine.UI;

public class VignetteFollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;

    private Material vignetteMaterial;
    private Camera mainCamera;
    private static readonly int CenterProp = Shader.PropertyToID("_Center");

    private void Awake()
    {
        mainCamera = Camera.main;

        Image image = GetComponent<Image>();
        vignetteMaterial = Instantiate(image.material);
        image.material = vignetteMaterial;
    }

    private void LateUpdate()
    {
        if (player == null || mainCamera == null || vignetteMaterial == null) return;

        Vector3 screenPos = mainCamera.WorldToScreenPoint(player.position);
        Vector2 centerUV = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);

        vignetteMaterial.SetVector(CenterProp, new Vector4(centerUV.x, centerUV.y, 0f, 0f));
    }
}
