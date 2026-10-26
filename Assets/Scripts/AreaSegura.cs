using UnityEngine;

/// <summary>Ajusta el panel al área segura de la pantalla (notch, barra de gestos) en móviles.</summary>
[RequireComponent(typeof(RectTransform))]
public class AreaSegura : MonoBehaviour
{
    private RectTransform rt;
    private Rect ultimaArea;
    private Vector2Int ultimaPantalla;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        Aplicar();
    }

    private void Update()
    {
        if (Screen.safeArea != ultimaArea || ultimaPantalla.x != Screen.width || ultimaPantalla.y != Screen.height)
            Aplicar();
    }

    private void Aplicar()
    {
        ultimaArea = Screen.safeArea;
        ultimaPantalla = new Vector2Int(Screen.width, Screen.height);
        if (Screen.width == 0 || Screen.height == 0) return;

        Vector2 min = ultimaArea.position;
        Vector2 max = ultimaArea.position + ultimaArea.size;
        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
