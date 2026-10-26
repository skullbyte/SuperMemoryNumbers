using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Prueba automática para revisar el juego sin tocarlo: solo se activa al lanzarlo con "-captura carpeta".
/// Juega solo (acierta rondas, usa los power-ups y falla a propósito) y guarda capturas de cada momento.
/// </summary>
public class CapturaAutomatica : MonoBehaviour
{
    [SerializeField] private SuperMemory juego;
    [SerializeField] private Button botonEmpezar;
    [SerializeField] private Button botonLuz;
    [SerializeField] private Button botonCorazon;

    private string carpeta;

    private IEnumerator Start()
    {
        string[] args = Environment.GetCommandLineArgs();
        int i = Array.IndexOf(args, "-captura");
        if (i < 0 || i + 1 >= args.Length) yield break;
        carpeta = args[i + 1];
        Directory.CreateDirectory(carpeta);

        yield return new WaitForSeconds(1f);
        yield return Capturar("1_inicio");

        botonEmpezar.onClick.Invoke();
        yield return new WaitForSeconds(0.3f);
        yield return Capturar("2_memorizando");
        yield return EsperarEstado(SuperMemory.EstadoJuego.Jugando);
        yield return Capturar("3_tapadas");

        // Luz: ilumina el 1
        botonLuz.onClick.Invoke();
        yield return new WaitForSeconds(0.2f);
        yield return Capturar("4_luz");
        yield return new WaitForSeconds(0.8f);

        // Corazón + fallo a propósito: deja el corazón roto y no quita vida
        botonCorazon.onClick.Invoke();
        Pulsar(CasillaIncorrecta());
        yield return new WaitForSeconds(0.2f);
        yield return Capturar("5_corazon_roto");

        // Acierta rondas hasta llegar a la fase 2 (aparecen los *)
        while (juego.Fase < 2)
        {
            yield return EsperarEstado(SuperMemory.EstadoJuego.Jugando);
            if (juego.Numeros == 6 && juego.Siguiente == 1) yield return Capturar("6_seis_numeros_tapadas");
            while (juego.Estado == SuperMemory.EstadoJuego.Jugando)
            {
                Pulsar(CasillaConValor(juego.Siguiente));
                yield return new WaitForSeconds(0.1f);
            }
            if (juego.Numeros == 9 && juego.Fase == 1) yield return Capturar("7_ronda_de_nueve");
        }
        yield return EsperarEstado(SuperMemory.EstadoJuego.Memorizando);
        yield return new WaitForSeconds(0.2f);
        yield return Capturar("8_fase2_estrella");

        // Fallo sin corazón: se pierde una vida y se enseñan los números
        yield return EsperarEstado(SuperMemory.EstadoJuego.Jugando);
        Pulsar(CasillaIncorrecta());
        yield return new WaitForSeconds(0.2f);
        yield return Capturar("9_fallo");

        Application.Quit();
    }

    private void Pulsar(CasillaNumero casilla) => casilla.Boton.onClick.Invoke();

    private CasillaNumero CasillaConValor(int valor)
    {
        foreach (CasillaNumero c in juego.Casillas)
            if (c.Valor == valor) return c;
        return null;
    }

    // Una casilla sin resolver que no sea la que toca
    private CasillaNumero CasillaIncorrecta()
    {
        foreach (CasillaNumero c in juego.Casillas)
            if (!c.Resuelta && c.Valor != juego.Siguiente) return c;
        return null;
    }

    private IEnumerator EsperarEstado(SuperMemory.EstadoJuego estado)
    {
        while (juego.Estado != estado) yield return null;
    }

    private IEnumerator Capturar(string nombre)
    {
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(carpeta, $"{nombre}_{Screen.width}x{Screen.height}.png"));
        yield return null;
    }
}
