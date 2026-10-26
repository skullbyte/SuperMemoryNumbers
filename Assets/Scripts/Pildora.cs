using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mantiene los extremos del botón como semicírculos perfectos sea cual sea su tamaño:
/// ajusta el multiplicador de píxeles del sprite 9-slice para que el borde mida justo la mitad del alto.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class Pildora : MonoBehaviour
{
    private Image imagen;
    private RectTransform rt;

    private void OnEnable()
    {
        imagen = GetComponent<Image>();
        rt = (RectTransform)transform;
        Ajustar();
    }

    private void OnRectTransformDimensionsChange() => Ajustar();

    private void Ajustar()
    {
        if (imagen == null || imagen.sprite == null || rt == null) return;
        float mitadAlto = rt.rect.height / 2f;
        if (mitadAlto <= 0) return;
        float bordePx = imagen.sprite.border.y; // borde inferior del 9-slice (= radio del sprite)
        float unidadesPorPx = imagen.sprite.pixelsPerUnit / 100f;
        imagen.pixelsPerUnitMultiplier = bordePx / unidadesPorPx / mitadAlto;
    }
}
