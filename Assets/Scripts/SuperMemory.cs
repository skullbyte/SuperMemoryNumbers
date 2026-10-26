using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Super Memory Numbers (versión placeholder).
/// En cada ronda se colocan los números 1..N en casillas al azar, se enseñan un momento y se tapan:
/// hay que pulsarlos en orden. Tras varios aciertos se añade un número (de 3 a 9) y, al superar el 9,
/// se pasa de fase: se vuelve a 3 números pero uno de ellos se enseña como "*" (uno más en cada fase).
/// Power-ups: Ojo (vuelve a enseñar los números), Luz (desvela el siguiente) y Corazón (perdona un fallo).
/// </summary>
public class SuperMemory : MonoBehaviour
{
    public enum EstadoJuego { Inicio, Memorizando, Jugando, Resultado, FinPartida }

    [Header("Tablero (array de casillas, cada una con su Text)")]
    [SerializeField] private CasillaNumero[] casillas;

    [Header("Reglas")]
    [SerializeField, Range(1, 9)] private int numerosIniciales = 3;
    [SerializeField, Range(3, 9)] private int numerosMaximos = 9;
    [SerializeField, Min(1)] private int aciertosParaSubir = 2;
    [SerializeField, Min(0.2f)] private float tiempoVisible = 1f;          // con los números iniciales
    [SerializeField, Min(0f)] private float tiempoPorNumeroExtra = 0.25f;  // se suma por cada número de más
    [SerializeField, Min(1)] private int vidasIniciales = 3;
    [SerializeField, Min(0)] private int usosPorFase = 1;                  // usos de cada power-up por fase

    [Header("Marcador")]
    [SerializeField] private TextMeshProUGUI textoFase;
    [SerializeField] private TextMeshProUGUI textoNumeros;
    [SerializeField] private TextMeshProUGUI textoPuntos;
    [SerializeField] private TextMeshProUGUI textoVidas;
    [SerializeField] private TextMeshProUGUI textoMensaje;

    [Header("Botones")]
    [SerializeField] private Button botonEmpezar;
    [SerializeField] private TextMeshProUGUI textoEmpezar;
    [SerializeField] private Button botonOjo;
    [SerializeField] private TextMeshProUGUI textoOjo;
    [SerializeField] private Button botonLuz;
    [SerializeField] private TextMeshProUGUI textoLuz;
    [SerializeField] private Button botonCorazon;
    [SerializeField] private TextMeshProUGUI textoCorazon;
    [SerializeField] private Image fondoCorazon;

    [Header("Colores")]
    [SerializeField] private Color colorPowerUp = new Color32(0x2B, 0x29, 0x30, 0xFF);
    [SerializeField] private Color colorCorazonActivo = new Color32(0xFF, 0x6F, 0x6A, 0xFF);

    private EstadoJuego estado = EstadoJuego.Inicio;
    private int fase;
    private int numeros;        // números que hay en el tablero en esta ronda
    private int siguiente;      // número que toca pulsar
    private int aciertos;       // rondas superadas desde que se añadió el último número
    private int puntos;
    private int vidas;
    private int usosOjo, usosLuz, usosCorazon;
    private bool corazonActivo;
    private Coroutine mostrando;

    public EstadoJuego Estado => estado;
    public int Fase => fase;
    public int Numeros => numeros;
    public int Siguiente => siguiente;
    public IReadOnlyList<CasillaNumero> Casillas => casillas;

    // Conecta los botones y deja el tablero vacío esperando a "Empezar"
    private void Start()
    {
        foreach (CasillaNumero c in casillas)
        {
            c.AlPulsar += PulsarCasilla;
            c.Preparar(0, false);
        }
        botonEmpezar.onClick.AddListener(NuevaPartida);
        botonOjo.onClick.AddListener(UsarOjo);
        botonLuz.onClick.AddListener(UsarLuz);
        botonCorazon.onClick.AddListener(UsarCorazon);
        textoMensaje.text = "Memoriza dónde están los números y púlsalos en orden";
        RecargarPowerUps();
        ActualizarMarcador();
    }

    // ---------- Partida y rondas ----------

    // Empieza desde la fase 1 con 3 números
    public void NuevaPartida()
    {
        fase = 1;
        numeros = numerosIniciales;
        aciertos = 0;
        puntos = 0;
        vidas = vidasIniciales;
        corazonActivo = false;
        RecargarPowerUps();
        NuevaRonda();
    }

    // Reparte los números 1..N en casillas al azar y los enseña durante un momento
    private void NuevaRonda()
    {
        List<int> posiciones = new List<int>();
        for (int i = 0; i < casillas.Length; i++) posiciones.Add(i);
        Barajar(posiciones);

        // A partir de la fase 2, (fase - 1) números se enseñan como "*" (nunca todos)
        int estrellas = Mathf.Min(fase - 1, numeros - 1);
        List<int> valoresEstrella = new List<int>();
        for (int v = 1; v <= numeros; v++) valoresEstrella.Add(v);
        Barajar(valoresEstrella);
        valoresEstrella.RemoveRange(estrellas, valoresEstrella.Count - estrellas);

        foreach (CasillaNumero c in casillas) c.Preparar(0, false);
        for (int v = 1; v <= numeros; v++)
            casillas[posiciones[v - 1]].Preparar(v, valoresEstrella.Contains(v));

        siguiente = 1;
        textoMensaje.text = estrellas > 0 ? "¡Memoriza! (* es un número que tendrás que deducir)" : "¡Memoriza!";
        ActualizarMarcador();
        Mostrar(TiempoVisible(), true);
    }

    private float TiempoVisible() => tiempoVisible + (numeros - numerosIniciales) * tiempoPorNumeroExtra;

    // Enseña los números que quedan por pulsar y los vuelve a tapar al acabar el tiempo
    private void Mostrar(float segundos, bool inicioRonda)
    {
        if (mostrando != null) StopCoroutine(mostrando);
        mostrando = StartCoroutine(MostrarDurante(segundos, inicioRonda));
    }

    private IEnumerator MostrarDurante(float segundos, bool inicioRonda)
    {
        estado = EstadoJuego.Memorizando;
        ActualizarMarcador();
        foreach (CasillaNumero c in casillas)
            if (!c.Resuelta) c.MostrarAspecto(CasillaNumero.Aspecto.Visible);

        yield return new WaitForSeconds(segundos);

        foreach (CasillaNumero c in casillas)
            if (!c.Resuelta) c.MostrarAspecto(CasillaNumero.Aspecto.Tapada);
        estado = EstadoJuego.Jugando;
        if (inicioRonda) textoMensaje.text = "Pulsa los números en orden: " + siguiente;
        ActualizarMarcador();
        mostrando = null;
    }

    // Comprueba si la casilla pulsada es la que tocaba
    private void PulsarCasilla(CasillaNumero casilla)
    {
        if (estado != EstadoJuego.Jugando || casilla.Resuelta) return;

        if (casilla.Valor == siguiente)
        {
            casilla.MarcarResuelta();
            siguiente++;
            if (siguiente > numeros) RondaSuperada();
            else textoMensaje.text = "¡Bien! Ahora el " + siguiente;
            ActualizarMarcador();
            return;
        }

        if (corazonActivo)
        {
            // El corazón perdona el fallo y deja un corazón roto en esa casilla
            corazonActivo = false;
            casilla.MostrarAspecto(CasillaNumero.Aspecto.CorazonRoto);
            textoMensaje.text = "¡El corazón te ha salvado! Sigue con el " + siguiente;
            ActualizarMarcador();
            return;
        }

        Fallo(casilla);
    }

    // Suma puntos y, cada X aciertos, añade un número o pasa de fase
    private void RondaSuperada()
    {
        puntos += numeros * 10 * fase;
        aciertos++;
        string mensaje = "¡Ronda superada!";
        if (aciertos >= aciertosParaSubir)
        {
            aciertos = 0;
            if (numeros < numerosMaximos)
            {
                numeros++;
                mensaje = "¡Ronda superada! Ahora con " + numeros + " números";
            }
            else
            {
                fase++;
                numeros = numerosIniciales;
                RecargarPowerUps();
                mensaje = "¡Fase " + fase + "! Aparecen los *";
            }
        }
        StartCoroutine(Pausa(mensaje, 1.2f, NuevaRonda));
    }

    // Pierde una vida, enseña dónde estaban los números y repite la ronda (o termina la partida)
    private void Fallo(CasillaNumero pulsada)
    {
        vidas--;
        foreach (CasillaNumero c in casillas)
            if (!c.Resuelta) c.MostrarAspecto(CasillaNumero.Aspecto.Solucion);
        pulsada.MostrarAspecto(CasillaNumero.Aspecto.Fallada);

        if (vidas <= 0)
        {
            estado = EstadoJuego.FinPartida;
            textoMensaje.text = "Fin de la partida: " + puntos + " puntos (fase " + fase + ")";
            ActualizarMarcador();
            return;
        }
        StartCoroutine(Pausa("¡Fallo! Tocaba el " + siguiente, 1.5f, NuevaRonda));
    }

    private IEnumerator Pausa(string mensaje, float segundos, System.Action despues)
    {
        estado = EstadoJuego.Resultado;
        textoMensaje.text = mensaje;
        ActualizarMarcador();
        yield return new WaitForSeconds(segundos);
        despues();
    }

    // ---------- Power-ups ----------

    private void RecargarPowerUps()
    {
        usosOjo = usosLuz = usosCorazon = usosPorFase;
    }

    // Ojo: vuelve a enseñar los números que quedan
    private void UsarOjo()
    {
        if (estado != EstadoJuego.Jugando || usosOjo <= 0) return;
        usosOjo--;
        textoMensaje.text = "Ojo: vuelve a mirarlos";
        Mostrar(TiempoVisible(), false);
    }

    // Luz: ilumina la casilla del número que toca
    private void UsarLuz()
    {
        if (estado != EstadoJuego.Jugando || usosLuz <= 0) return;
        usosLuz--;
        foreach (CasillaNumero c in casillas)
            if (c.Valor == siguiente) StartCoroutine(Destello(c));
        textoMensaje.text = "Luz: el " + siguiente + " está ahí";
        ActualizarMarcador();
    }

    private IEnumerator Destello(CasillaNumero casilla)
    {
        casilla.MostrarAspecto(CasillaNumero.Aspecto.Pista);
        yield return new WaitForSeconds(0.8f);
        if (!casilla.Resuelta && estado == EstadoJuego.Jugando)
            casilla.MostrarAspecto(CasillaNumero.Aspecto.Tapada);
    }

    // Corazón: el próximo fallo no cuenta (aunque sea en otra ronda)
    private void UsarCorazon()
    {
        if (estado != EstadoJuego.Jugando || usosCorazon <= 0 || corazonActivo) return;
        usosCorazon--;
        corazonActivo = true;
        textoMensaje.text = "Corazón activo: el próximo fallo no cuenta";
        ActualizarMarcador();
    }

    // ---------- Interfaz ----------

    private void ActualizarMarcador()
    {
        bool hayPartida = estado != EstadoJuego.Inicio;
        textoFase.text = hayPartida ? "Fase " + fase : "Fase -";
        textoNumeros.text = hayPartida ? numeros + " números" : "- números";
        textoPuntos.text = puntos + " pts";
        textoVidas.text = "Vidas " + (hayPartida ? vidas : vidasIniciales);

        bool jugando = estado == EstadoJuego.Jugando;
        botonOjo.interactable = jugando && usosOjo > 0;
        botonLuz.interactable = jugando && usosLuz > 0;
        botonCorazon.interactable = jugando && usosCorazon > 0 && !corazonActivo;
        textoOjo.text = "×" + usosOjo;
        textoLuz.text = "×" + usosLuz;
        textoCorazon.text = "×" + usosCorazon;
        fondoCorazon.color = corazonActivo ? colorCorazonActivo : colorPowerUp;

        bool puedeEmpezar = estado == EstadoJuego.Inicio || estado == EstadoJuego.FinPartida;
        botonEmpezar.interactable = puedeEmpezar;
        textoEmpezar.text = estado == EstadoJuego.FinPartida ? "Otra partida" : "Empezar";
    }

    // Fisher-Yates
    private static void Barajar(List<int> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (lista[i], lista[j]) = (lista[j], lista[i]);
        }
    }
}
