using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ajusta el tamaño de las celdas del GridLayoutGroup para que el tablero (columnas × filas)
/// quepa entero en su panel con casillas cuadradas, sea cual sea la orientación de la pantalla.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
public class RejillaCuadrada : MonoBehaviour
{
    [SerializeField, Min(1)] private int columnas = 3;
    [SerializeField, Min(1)] private int filas = 3;
    [SerializeField, Min(0)] private float ladoMaximo = 300;

    private GridLayoutGroup rejilla;
    private RectTransform rt;

    private void OnEnable()
    {
        rejilla = GetComponent<GridLayoutGroup>();
        rt = (RectTransform)transform;
        Ajustar();
    }

    private void OnRectTransformDimensionsChange() => Ajustar();

    private void Ajustar()
    {
        if (rejilla == null || rt == null) return;
        float anchoLibre = rt.rect.width - rejilla.padding.horizontal - rejilla.spacing.x * (columnas - 1);
        float altoLibre = rt.rect.height - rejilla.padding.vertical - rejilla.spacing.y * (filas - 1);
        float lado = Mathf.Min(anchoLibre / columnas, altoLibre / filas, ladoMaximo);
        if (lado <= 0) return;
        rejilla.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        rejilla.constraintCount = columnas;
        rejilla.cellSize = new Vector2(lado, lado);
    }
}
