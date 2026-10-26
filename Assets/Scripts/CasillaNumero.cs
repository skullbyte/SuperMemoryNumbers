using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Una casilla del tablero. Guarda el número que le toca en la ronda (0 = vacía) y si se muestra como "*",
/// y cambia de aspecto según el momento de la partida (visible, tapada, acertada, fallada...).
/// </summary>
public class CasillaNumero : MonoBehaviour
{
    public enum Aspecto { Vacia, Visible, Tapada, Acertada, Fallada, Pista, CorazonRoto, Solucion }

    [SerializeField] private Button boton;
    [SerializeField] private Image fondo;
    [SerializeField] private TextMeshProUGUI texto;
    [SerializeField] private Image icono;   // corazón roto

    [Header("Colores")]
    [SerializeField] private Color colorVacia = new Color32(0x24, 0x22, 0x2A, 0xFF);
    [SerializeField] private Color colorVisible = new Color32(0xE6, 0xE1, 0xE5, 0xFF);
    [SerializeField] private Color colorTapada = new Color32(0xD0, 0xBC, 0xFF, 0xFF);
    [SerializeField] private Color colorAcertada = new Color32(0x7F, 0xD8, 0x9B, 0xFF);
    [SerializeField] private Color colorFallada = new Color32(0xFF, 0x6F, 0x6A, 0xFF);
    [SerializeField] private Color colorPista = new Color32(0xFF, 0xD8, 0x6B, 0xFF);
    [SerializeField] private Color textoOscuro = new Color32(0x1D, 0x1B, 0x20, 0xFF);

    /// <summary>Número de la casilla en esta ronda (0 si no tiene).</summary>
    public int Valor { get; private set; }

    /// <summary>Si es true, al memorizar se ve "*" en lugar del número.</summary>
    public bool Estrella { get; private set; }

    /// <summary>Ya se ha pulsado bien en esta ronda.</summary>
    public bool Resuelta { get; private set; }

    /// <summary>Se lanza al pulsar la casilla.</summary>
    public event Action<CasillaNumero> AlPulsar;

    public Button Boton => boton;

    private void Awake()
    {
        boton.onClick.AddListener(() => AlPulsar?.Invoke(this));
    }

    // Asigna el número de la ronda y deja la casilla vacía hasta que se muestre
    public void Preparar(int valor, bool estrella)
    {
        Valor = valor;
        Estrella = estrella;
        Resuelta = false;
        MostrarAspecto(Aspecto.Vacia);
    }

    public void MarcarResuelta()
    {
        Resuelta = true;
        MostrarAspecto(Aspecto.Acertada);
    }

    // Cambia el color y el texto según el aspecto
    public void MostrarAspecto(Aspecto aspecto)
    {
        icono.enabled = aspecto == Aspecto.CorazonRoto;
        texto.color = textoOscuro;
        switch (aspecto)
        {
            case Aspecto.Vacia:
                fondo.color = colorVacia;
                texto.text = "";
                break;
            case Aspecto.Visible:
                fondo.color = Valor > 0 ? colorVisible : colorVacia;
                texto.text = Valor == 0 ? "" : Estrella ? "*" : Valor.ToString();
                break;
            case Aspecto.Tapada:
                // Las casillas sin número también se tapan: si no, se sabría dónde están los números
                fondo.color = colorTapada;
                texto.text = "";
                break;
            case Aspecto.Acertada:
                fondo.color = colorAcertada;
                texto.text = Valor.ToString();
                break;
            case Aspecto.Fallada:
                fondo.color = colorFallada;
                texto.text = Valor > 0 ? Valor.ToString() : "×";
                break;
            case Aspecto.Pista:
                fondo.color = colorPista;
                texto.text = Valor.ToString();
                break;
            case Aspecto.Solucion:
                // Al fallar se enseña el número de verdad, también el de las casillas "*"
                fondo.color = Valor > 0 ? colorVisible : colorVacia;
                texto.text = Valor > 0 ? Valor.ToString() : "";
                break;
            case Aspecto.CorazonRoto:
                // Sigue tapada (si tenía número hay que pulsarla más tarde), con el corazón roto encima
                fondo.color = colorTapada;
                texto.text = "";
                break;
        }
    }
}
