using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Monta la escena de Super Memory Numbers desde cero (menú Super Memory).
/// 1) Preparar: importa TextMeshPro y genera los sprites (casilla, píldora e iconos de los power-ups).
/// 2) Construir escena: crea la fuente y la interfaz completa (tablero 3×3, marcador, power-ups y botón Empezar).
/// 3) Compilar para Linux: build de prueba para lanzar la captura automática.
/// </summary>
public static class ConstructorSuperMemory
{
    private const string RutaPildora = "Assets/Sprites/Pildora.png";
    private const string RutaCasilla = "Assets/Sprites/Casilla.png";
    private const string RutaOjo = "Assets/Sprites/Ojo.png";
    private const string RutaLuz = "Assets/Sprites/Luz.png";
    private const string RutaCorazon = "Assets/Sprites/Corazon.png";
    private const string RutaCorazonRoto = "Assets/Sprites/CorazonRoto.png";
    private const string RutaTtf = "Assets/Fonts/NotoSans-Regular.ttf";
    private const string RutaFuente = "Assets/Fonts/NotoSans SDF.asset";
    private const string RutaEscena = "Assets/Scenes/SuperMemory.unity";

    private static readonly Color Fondo = new Color32(0x14, 0x12, 0x18, 0xFF);
    private static readonly Color Gris = new Color32(0x2B, 0x29, 0x30, 0xFF);
    private static readonly Color TextoClaro = new Color32(0xE6, 0xE1, 0xE5, 0xFF);
    private static readonly Color TextoSecundario = new Color32(0x93, 0x8F, 0x99, 0xFF);
    private static readonly Color Lavanda = new Color32(0xD0, 0xBC, 0xFF, 0xFF);
    private static readonly Color TextoLavanda = new Color32(0x38, 0x1E, 0x72, 0xFF);
    private static readonly Color Rojo = new Color32(0xFF, 0x6F, 0x6A, 0xFF);
    private static readonly Color Amarillo = new Color32(0xFF, 0xD8, 0x6B, 0xFF);

    [MenuItem("Super Memory/1. Preparar (TextMeshPro y sprites)")]
    public static void Preparar()
    {
        if (!File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
        {
            string paquete = Path.GetFullPath("Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage");
            AssetDatabase.ImportPackage(paquete, false);
        }
        Directory.CreateDirectory("Assets/Sprites");
        CrearSpriteRedondeado(RutaPildora, 512, 256f, 255);
        CrearSpriteRedondeado(RutaCasilla, 256, 48f, 60);
        CrearIcono(RutaOjo, Ojo);
        CrearIcono(RutaLuz, Bombilla);
        CrearIcono(RutaCorazon, (x, y) => Corazon(x, y));
        CrearIcono(RutaCorazonRoto, CorazonRoto);
        AssetDatabase.Refresh();
        Debug.Log("[SuperMemory] Preparado");
    }

    [MenuItem("Super Memory/2. Construir escena")]
    public static void ConstruirEscena()
    {
        TMP_FontAsset fuente = CrearFuente();
        Sprite pildora = AssetDatabase.LoadAssetAtPath<Sprite>(RutaPildora);
        Sprite casillaSprite = AssetDatabase.LoadAssetAtPath<Sprite>(RutaCasilla);

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camara = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
        Camera cam = camara.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Fondo;
        cam.orthographic = true;

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        // Canvas que escala con la pantalla
        var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        var canvas = (RectTransform)canvasGo.transform;

        Estirar(Hijo("Fondo", canvas)).gameObject.AddComponent<Image>().color = Fondo;

        RectTransform area = Estirar(Hijo("AreaSegura", canvas));
        area.gameObject.AddComponent<AreaSegura>();
        RectTransform superior = Estirar(Hijo("PanelTablero", area));
        RectTransform inferior = Estirar(Hijo("PanelControles", area));
        LayoutResponsive responsive = area.gameObject.AddComponent<LayoutResponsive>();
        Asignar(responsive, "scaler", scaler);
        Asignar(responsive, "panelSuperior", superior);
        Asignar(responsive, "panelInferior", inferior);

        // Tablero 3×3: el array de casillas del juego
        RectTransform tablero = Estirar(Hijo("Tablero", superior));
        GridLayoutGroup rejilla = tablero.gameObject.AddComponent<GridLayoutGroup>();
        rejilla.padding = new RectOffset(40, 40, 60, 30);
        rejilla.spacing = new Vector2(24, 24);
        rejilla.childAlignment = TextAnchor.MiddleCenter;
        tablero.gameObject.AddComponent<RejillaCuadrada>();

        Sprite corazonRoto = AssetDatabase.LoadAssetAtPath<Sprite>(RutaCorazonRoto);
        var casillas = new CasillaNumero[9];
        for (int i = 0; i < casillas.Length; i++)
            casillas[i] = CrearCasilla("Casilla" + (i + 1), tablero, fuente, casillaSprite, corazonRoto);

        // Controles: marcador, mensaje, power-ups y Empezar
        VerticalLayoutGroup vInf = inferior.gameObject.AddComponent<VerticalLayoutGroup>();
        vInf.padding = new RectOffset(60, 60, 30, 70);
        vInf.spacing = 26;
        vInf.childAlignment = TextAnchor.LowerCenter;
        Controlar(vInf);

        RectTransform marcador = Hijo("Marcador", inferior);
        HorizontalLayoutGroup hMarcador = marcador.gameObject.AddComponent<HorizontalLayoutGroup>();
        hMarcador.spacing = 20;
        Controlar(hMarcador);
        marcador.gameObject.AddComponent<LayoutElement>().preferredHeight = 70;
        TextMeshProUGUI textoFase = Texto("Fase", marcador, fuente, "Fase -", 44, TextoClaro);
        TextMeshProUGUI textoNumeros = Texto("Numeros", marcador, fuente, "- números", 44, TextoSecundario);
        TextMeshProUGUI textoPuntos = Texto("Puntos", marcador, fuente, "0 pts", 44, TextoClaro);
        TextMeshProUGUI textoVidas = Texto("Vidas", marcador, fuente, "Vidas 3", 44, Rojo);
        // En horizontal la columna es estrecha: los textos del marcador se encogen para no pisarse
        foreach (TextMeshProUGUI t in new[] { textoFase, textoNumeros, textoPuntos, textoVidas })
        {
            t.enableAutoSizing = true;
            t.fontSizeMin = 22;
            t.fontSizeMax = 44;
        }

        TextMeshProUGUI mensaje = Texto("Mensaje", inferior, fuente, "", 48, TextoClaro);
        mensaje.textWrappingMode = TextWrappingModes.Normal;
        mensaje.enableAutoSizing = true;
        mensaje.fontSizeMin = 30;
        mensaje.fontSizeMax = 50;
        LayoutElement leMensaje = mensaje.gameObject.AddComponent<LayoutElement>();
        leMensaje.flexibleHeight = 1;
        leMensaje.minHeight = 110;

        RectTransform powerUps = Hijo("PowerUps", inferior);
        HorizontalLayoutGroup hPower = powerUps.gameObject.AddComponent<HorizontalLayoutGroup>();
        hPower.spacing = 24;
        Controlar(hPower);
        hPower.childForceExpandHeight = true;
        LayoutElement lePower = powerUps.gameObject.AddComponent<LayoutElement>();
        lePower.preferredHeight = 130;
        lePower.minHeight = 90;
        lePower.flexibleHeight = 0;   // que no se estire con el espacio sobrante
        Button ojo = BotonPowerUp("BotonOjo", powerUps, fuente, pildora, RutaOjo, Lavanda, out TextMeshProUGUI textoOjo);
        Button luz = BotonPowerUp("BotonLuz", powerUps, fuente, pildora, RutaLuz, Amarillo, out TextMeshProUGUI textoLuz);
        Button corazon = BotonPowerUp("BotonCorazon", powerUps, fuente, pildora, RutaCorazon, Rojo, out TextMeshProUGUI textoCorazon);

        Button empezar = Boton("BotonEmpezar", inferior, fuente, pildora, "Empezar", Lavanda, TextoLavanda, 170);

        // Lógica
        SuperMemory juego = canvasGo.AddComponent<SuperMemory>();
        var so = new SerializedObject(juego);
        SerializedProperty lista = so.FindProperty("casillas");
        lista.arraySize = casillas.Length;
        for (int i = 0; i < casillas.Length; i++)
            lista.GetArrayElementAtIndex(i).objectReferenceValue = casillas[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        Asignar(juego, "textoFase", textoFase);
        Asignar(juego, "textoNumeros", textoNumeros);
        Asignar(juego, "textoPuntos", textoPuntos);
        Asignar(juego, "textoVidas", textoVidas);
        Asignar(juego, "textoMensaje", mensaje);
        Asignar(juego, "botonEmpezar", empezar);
        Asignar(juego, "textoEmpezar", empezar.GetComponentInChildren<TextMeshProUGUI>());
        Asignar(juego, "botonOjo", ojo);
        Asignar(juego, "textoOjo", textoOjo);
        Asignar(juego, "botonLuz", luz);
        Asignar(juego, "textoLuz", textoLuz);
        Asignar(juego, "botonCorazon", corazon);
        Asignar(juego, "textoCorazon", textoCorazon);
        Asignar(juego, "fondoCorazon", corazon.GetComponent<Image>());

        CapturaAutomatica captura = canvasGo.AddComponent<CapturaAutomatica>();
        Asignar(captura, "juego", juego);
        Asignar(captura, "botonEmpezar", empezar);
        Asignar(captura, "botonLuz", luz);
        Asignar(captura, "botonCorazon", corazon);

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), RutaEscena);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(RutaEscena, true) };
        PlayerSettings.productName = "Super Memory Numbers";
        PlayerSettings.companyName = "Jamie Tanner";
        PlayerSettings.runInBackground = true;
        AssetDatabase.SaveAssets();
        Debug.Log("[SuperMemory] Escena construida en " + RutaEscena);
    }

    [MenuItem("Super Memory/3. Compilar para Linux (prueba)")]
    public static void CompilarLinux()
    {
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        var opciones = new BuildPlayerOptions
        {
            scenes = new[] { RutaEscena },
            locationPathName = "Builds/Linux/SuperMemoryNumbers.x86_64",
            target = BuildTarget.StandaloneLinux64,
        };
        var informe = BuildPipeline.BuildPlayer(opciones);
        Debug.Log("[SuperMemory] Build Linux: " + informe.summary.result + " (" + informe.summary.totalSize / (1024 * 1024) + " MB)");
    }

    // ---------- Piezas de la interfaz ----------

    private static CasillaNumero CrearCasilla(string nombre, RectTransform padre, TMP_FontAsset fuente, Sprite sprite, Sprite corazonRoto)
    {
        RectTransform rt = Hijo(nombre, padre);
        Image fondo = rt.gameObject.AddComponent<Image>();
        fondo.sprite = sprite;
        fondo.type = Image.Type.Sliced;
        Button boton = rt.gameObject.AddComponent<Button>();
        ColorBlock colores = boton.colors;
        colores.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        colores.disabledColor = Color.white;
        boton.colors = colores;

        TextMeshProUGUI texto = Texto("Numero", rt, fuente, "", 120, TextoClaro);
        texto.enableAutoSizing = true;
        texto.fontSizeMin = 40;
        texto.fontSizeMax = 140;
        RectTransform rtTexto = Estirar((RectTransform)texto.transform);
        rtTexto.offsetMin = new Vector2(12, 12);
        rtTexto.offsetMax = new Vector2(-12, -12);

        RectTransform rtIcono = Estirar(Hijo("CorazonRoto", rt));
        rtIcono.offsetMin = new Vector2(30, 30);
        rtIcono.offsetMax = new Vector2(-30, -30);
        Image icono = rtIcono.gameObject.AddComponent<Image>();
        icono.sprite = corazonRoto;
        icono.preserveAspect = true;
        icono.color = Rojo;
        icono.raycastTarget = false;
        icono.enabled = false;

        CasillaNumero casilla = rt.gameObject.AddComponent<CasillaNumero>();
        Asignar(casilla, "boton", boton);
        Asignar(casilla, "fondo", fondo);
        Asignar(casilla, "texto", texto);
        Asignar(casilla, "icono", icono);
        return casilla;
    }

    private static Button BotonPowerUp(string nombre, RectTransform padre, TMP_FontAsset fuente, Sprite pildora,
                                       string rutaIcono, Color colorIcono, out TextMeshProUGUI usos)
    {
        Button b = Boton(nombre, padre, fuente, pildora, "", Gris, TextoClaro, 130);
        Object.DestroyImmediate(b.GetComponentInChildren<TextMeshProUGUI>().gameObject);
        RectTransform rt = (RectTransform)b.transform;
        HorizontalLayoutGroup h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.padding = new RectOffset(20, 20, 18, 18);
        h.spacing = 10;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = true;

        Image icono = Hijo("Icono", rt).gameObject.AddComponent<Image>();
        icono.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(rutaIcono);
        icono.preserveAspect = true;
        icono.color = colorIcono;
        icono.raycastTarget = false;
        LayoutElement leIcono = icono.gameObject.AddComponent<LayoutElement>();
        leIcono.preferredWidth = 80;
        leIcono.minWidth = 50;

        usos = Texto("Usos", rt, fuente, "×1", 46, TextoClaro);
        LayoutElement leUsos = usos.gameObject.AddComponent<LayoutElement>();
        leUsos.preferredWidth = 70;
        return b;
    }

    // ---------- Fuente y sprites ----------

    private static TMP_FontAsset CrearFuente()
    {
        var existente = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RutaFuente);
        if (existente != null) return existente;

        Font ttf = AssetDatabase.LoadAssetAtPath<Font>(RutaTtf);
        TMP_FontAsset fuente = TMP_FontAsset.CreateFontAsset(ttf, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
        fuente.name = "NotoSans SDF";
        AssetDatabase.CreateAsset(fuente, RutaFuente);
        fuente.material.name = "NotoSans SDF Material";
        AssetDatabase.AddObjectToAsset(fuente.material, fuente);
        foreach (Texture2D atlas in fuente.atlasTextures)
        {
            atlas.name = "NotoSans SDF Atlas";
            AssetDatabase.AddObjectToAsset(atlas, fuente);
        }
        AssetDatabase.SaveAssets();
        return fuente;
    }

    /// <summary>Genera un rectángulo redondeado blanco y lo importa como sprite 9-slice.</summary>
    private static void CrearSpriteRedondeado(string ruta, int tam, float radio, int borde)
    {
        float mitad = tam / 2f;
        GuardarSprite(ruta, tam, (x, y) =>
        {
            float qx = Mathf.Abs(x + 0.5f - mitad) - (mitad - radio);
            float qy = Mathf.Abs(y + 0.5f - mitad) - (mitad - radio);
            float distancia = new Vector2(Mathf.Max(qx, 0), Mathf.Max(qy, 0)).magnitude
                              + Mathf.Min(Mathf.Max(qx, qy), 0) - radio;
            return Mathf.Clamp01(0.5f - distancia);
        }, borde);
    }

    /// <summary>
    /// Genera un icono blanco de 256 px a partir de una forma definida en coordenadas de -1 a 1
    /// (4×4 muestras por píxel para que los bordes salgan suaves).
    /// </summary>
    private static void CrearIcono(string ruta, Func<float, float, bool> dentro)
    {
        const int tam = 256;
        GuardarSprite(ruta, tam, (px, py) =>
        {
            int dentroCount = 0;
            for (int sy = 0; sy < 4; sy++)
                for (int sx = 0; sx < 4; sx++)
                {
                    float x = (px + (sx + 0.5f) / 4f) / tam * 2f - 1f;
                    float y = (py + (sy + 0.5f) / 4f) / tam * 2f - 1f;
                    if (dentro(x, y)) dentroCount++;
                }
            return dentroCount / 16f;
        }, 0);
    }

    private static void GuardarSprite(string ruta, int tam, Func<int, int, float> alfa, int borde)
    {
        var tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        var pixeles = new Color32[tam * tam];
        for (int y = 0; y < tam; y++)
            for (int x = 0; x < tam; x++)
                pixeles[y * tam + x] = new Color32(255, 255, 255, (byte)(alfa(x, y) * 255));
        tex.SetPixels32(pixeles);
        File.WriteAllBytes(ruta, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(ruta);
        var importador = (TextureImporter)AssetImporter.GetAtPath(ruta);
        importador.textureType = TextureImporterType.Sprite;
        importador.spriteImportMode = SpriteImportMode.Single;
        importador.spriteBorder = new Vector4(borde, borde, borde, borde);
        importador.alphaIsTransparency = true;
        importador.mipmapEnabled = false;
        importador.SaveAndReimport();
    }

    // Formas de los iconos (x e y entre -1 y 1, y hacia arriba)

    private static bool Ojo(float x, float y)
    {
        bool exterior = Mathf.Abs(x) <= 0.95f && Mathf.Abs(y) <= 0.6f * (1 - x * x / 0.9f);
        float xi = x / 0.8f;
        bool interior = Mathf.Abs(xi) <= 0.95f && Mathf.Abs(y) <= 0.4f * (1 - xi * xi / 0.9f);
        bool pupila = x * x + y * y <= 0.27f * 0.27f;
        return (exterior && !interior) || pupila;
    }

    private static bool Bombilla(float x, float y)
    {
        bool cristal = x * x + (y - 0.22f) * (y - 0.22f) <= 0.6f * 0.6f;
        bool cuello = Mathf.Abs(x) <= 0.3f && y >= -0.6f && y <= -0.18f && !(y > -0.46f && y < -0.4f);
        bool base_ = Mathf.Abs(x) <= 0.2f && y >= -0.8f && y <= -0.66f;
        return cristal || cuello || base_;
    }

    private static bool Corazon(float x, float y)
    {
        float cx = x * 1.4f;
        float cy = (y + 0.1f) * 1.4f;
        float a = cx * cx + cy * cy - 1f;
        return a * a * a - cx * cx * cy * cy * cy <= 0f;
    }

    // Corazón partido por una grieta en zigzag, con las dos mitades algo separadas
    private static bool CorazonRoto(float x, float y)
    {
        float t = y * 2.5f;
        float zigzag = 0.14f * (2f * Mathf.Abs(t - Mathf.Floor(t + 0.5f)) * 2f - 1f);
        if (x < zigzag - 0.05f) return Corazon(x + 0.1f, y);
        if (x > zigzag + 0.05f) return Corazon(x - 0.1f, y);
        return false;
    }

    // ---------- Ayudas ----------

    private static RectTransform Hijo(string nombre, RectTransform padre)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return (RectTransform)go.transform;
    }

    private static RectTransform Estirar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    private static void Controlar(HorizontalOrVerticalLayoutGroup g)
    {
        g.childControlWidth = true;
        g.childControlHeight = true;
        g.childForceExpandWidth = true;
        g.childForceExpandHeight = false;
    }

    private static TextMeshProUGUI Texto(string nombre, RectTransform padre, TMP_FontAsset fuente, string texto, float tam, Color color)
    {
        TextMeshProUGUI t = Hijo(nombre, padre).gameObject.AddComponent<TextMeshProUGUI>();
        t.font = fuente;
        t.text = texto;
        t.fontSize = tam;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.raycastTarget = false;
        return t;
    }

    private static Button Boton(string nombre, RectTransform padre, TMP_FontAsset fuente, Sprite pildora,
                                string texto, Color fondo, Color colorTexto, float alto)
    {
        RectTransform rt = Hijo(nombre, padre);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = pildora;
        img.type = Image.Type.Sliced;
        img.color = fondo;
        rt.gameObject.AddComponent<Pildora>();
        Button b = rt.gameObject.AddComponent<Button>();
        ColorBlock colores = b.colors;
        colores.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        colores.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.6f);
        b.colors = colores;
        LayoutElement le = rt.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = alto;
        le.minHeight = alto * 0.6f;
        le.flexibleHeight = 0;
        TextMeshProUGUI t = Texto("Texto", rt, fuente, texto, 60, colorTexto);
        Estirar((RectTransform)t.transform);
        return b;
    }

    private static void Asignar(Object componente, string campo, Object valor)
    {
        var so = new SerializedObject(componente);
        so.FindProperty(campo).objectReferenceValue = valor;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
