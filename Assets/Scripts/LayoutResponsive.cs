using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reparte la pantalla según la orientación:
/// vertical → tablero arriba, controles abajo; horizontal → tablero a la izquierda, controles a la derecha.
/// También ajusta el CanvasScaler para que el lado corto mida siempre 1080 unidades.
/// </summary>
[ExecuteAlways]
public class LayoutResponsive : MonoBehaviour
{
    [SerializeField] private CanvasScaler scaler;
    [SerializeField] private RectTransform panelSuperior;
    [SerializeField] private RectTransform panelInferior;
    [SerializeField, Range(0.2f, 0.6f)] private float altoBotonesVertical = 0.42f;
    [SerializeField, Range(0.2f, 0.6f)] private float anchoBotonesHorizontal = 0.4f;

    private bool? verticalAplicado;

    private void OnEnable() => Aplicar(true);

    private void Update() => Aplicar(false);

    private void Aplicar(bool forzar)
    {
        bool vertical = Screen.height >= Screen.width;
        if (!forzar && verticalAplicado == vertical) return;
        verticalAplicado = vertical;

        if (scaler != null)
        {
            scaler.referenceResolution = vertical ? new Vector2(1080, 1920) : new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (vertical)
        {
            Colocar(panelSuperior, new Vector2(0, altoBotonesVertical), Vector2.one);
            Colocar(panelInferior, Vector2.zero, new Vector2(1, altoBotonesVertical));
        }
        else
        {
            Colocar(panelSuperior, Vector2.zero, new Vector2(1 - anchoBotonesHorizontal, 1));
            Colocar(panelInferior, new Vector2(1 - anchoBotonesHorizontal, 0), Vector2.one);
        }
    }

    private static void Colocar(RectTransform rt, Vector2 min, Vector2 max)
    {
        if (rt == null) return;
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
