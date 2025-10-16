using UnityEngine;
using UnityEngine.UI;

public class UICrosshair : MonoBehaviour
{
    public Sprite crosshairSprite;

    void Start()
    {
        var canvas = new GameObject("CrosshairCanvas");
        canvas.transform.SetParent(transform, false);
        var c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();

        var crosshair = new GameObject("Crosshair");
        crosshair.transform.SetParent(canvas.transform, false);
        var image = crosshair.AddComponent<Image>();
        image.sprite = crosshairSprite;
        image.rectTransform.sizeDelta = new Vector2(32, 32); // Size of crosshair

        // Center the crosshair
        image.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        image.rectTransform.anchoredPosition = Vector2.zero;
    }
}