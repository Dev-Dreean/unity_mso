using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MapNavigation : MonoBehaviour
{
    // SINGLETON (Para o botao conseguir acessar facilmente)
    public static MapNavigation Instance;

    [Header("Configuracoes")]
    public float zoomSpeed = 2f;
    public float dragSpeed = 1f;
    public float minSize = 2f;
    public bool inverterArrasto = false;
    public bool travarTopo = false;

    // Variaveis internas
    private Camera cam;
    private SpriteRenderer mapBounds;
    private float mapMinX, mapMaxX, mapMinY, mapMaxY;
    private float currentMaxSize;
    private Vector3 dragOrigin;
    private bool isDragging = false;

    // Variavel publica para o botao saber se houve movimento
    public bool FoiArrastado { get; private set; } = false;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;
    }

    void Start()
    {
        if (mapBounds == null)
        {
            var switcher = FindFirstObjectByType<MapWorldSwitcher>();
            if (switcher != null && switcher.worlds.Length > 0)
            {
                var mundo = switcher.worlds[switcher.CurrentIndex];
                if (mundo.limitesParaEnquadrar != null) SetBounds(mundo.limitesParaEnquadrar);
            }
        }
    }

    public void SetBounds(SpriteRenderer novoMapa)
    {
        mapBounds = novoMapa;
        UpdateBoundsValues();
        if (cam != null)
        {
            CalculateMaxSize();
            cam.orthographicSize = currentMaxSize;
            Vector3 startPos = mapBounds.bounds.center;
            startPos.y = mapMinY + cam.orthographicSize;
            startPos.z = -10;
            cam.transform.position = startPos;
        }
    }

    void UpdateBoundsValues()
    {
        if (mapBounds != null)
        {
            mapMinX = mapBounds.bounds.min.x;
            mapMaxX = mapBounds.bounds.max.x;
            mapMinY = mapBounds.bounds.min.y;
            mapMaxY = mapBounds.bounds.max.y;
        }
    }

    void Update()
    {
        if (!cam || !mapBounds) return;

        CalculateMaxSize();

        // 1. Zoom (sempre permitido)
        HandleZoom();

        // 2. Arrasto inteligente
        // So bloqueia se for UI bloqueadora (HUD). Se for botao de fase, deixa passar.
        if (!IsPointerOverBlockingUI())
        {
            HandlePan();
        }

        // 3. Clique no fundo (para fechar popup)
        HandleBackgroundClick();

        // 4. Limites
        ClampCamera();
    }

    // O segredo do arrasto em cima de botao
    bool IsPointerOverBlockingUI()
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = GetPointerPosition()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            // Se o objeto tocado NAO tiver a tag LevelButton, entao e bloqueio (HUD, pause etc)
            if (!result.gameObject.CompareTag("LevelButton"))
            {
                return true;
            }
        }

        // Se so tocou em LevelButton ou nada, libera a camera
        return false;
    }

    void HandlePan()
    {
        if (Input.touchCount > 1)
        {
            isDragging = false;
            return;
        }

        if (IsPrimaryPointerDown())
        {
            dragOrigin = cam.ScreenToWorldPoint(GetPointerPosition());
            isDragging = true;
            FoiArrastado = false;
        }

        if (IsPrimaryPointerHeld() && isDragging)
        {
            Vector3 currentPos = cam.ScreenToWorldPoint(GetPointerPosition());
            Vector3 difference = dragOrigin - currentPos;

            // So considera que arrastou se moveu um pouco (zona morta)
            if (difference.magnitude > 0.05f)
            {
                FoiArrastado = true;

                if (!inverterArrasto) cam.transform.position += difference;
                else cam.transform.position -= difference;
            }
        }

        if (IsPrimaryPointerUp())
        {
            isDragging = false;
            // Nao resetamos FoiArrastado imediatamente, pois o botao le esse valor no frame do clique.
            Invoke(nameof(ResetDragFlag), 0.1f);
        }
    }

    void ResetDragFlag()
    {
        FoiArrastado = false;
    }

    void CalculateMaxSize()
    {
        float mapW = mapMaxX - mapMinX;
        float maxH = (mapW / cam.aspect) / 2f;
        currentMaxSize = Mathf.Max(maxH, minSize);
    }

    void HandleZoom()
    {
        float delta = 0f;
        Vector3 center = Vector3.zero;

        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            delta = -scroll * zoomSpeed;
            center = GetPointerPosition();
        }

        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            float prevMag = ((t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition)).magnitude;
            float currMag = (t0.position - t1.position).magnitude;
            delta = (prevMag - currMag) * 0.01f;
            center = (t0.position + t1.position) / 2f;
        }

        if (delta != 0)
        {
            Vector3 worldBefore = cam.ScreenToWorldPoint(center);
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + delta, minSize, currentMaxSize);
            Vector3 worldAfter = cam.ScreenToWorldPoint(center);
            cam.transform.position += (worldBefore - worldAfter);
        }
    }

    void ClampCamera()
    {
        float camH = cam.orthographicSize;
        float camWidth = camH * cam.aspect;
        float minX = mapMinX + camWidth;
        float maxX = mapMaxX - camWidth;
        float chaoAbsoluto = mapMinY + camH;
        float tetoAbsoluto = mapMaxY - camH;

        Vector3 pos = cam.transform.position;

        if (maxX < minX) pos.x = mapBounds.bounds.center.x;
        else pos.x = Mathf.Clamp(pos.x, minX, maxX);

        if (pos.y < chaoAbsoluto) pos.y = chaoAbsoluto;

        if (travarTopo)
        {
            if (pos.y > tetoAbsoluto) pos.y = tetoAbsoluto;
        }

        cam.transform.position = pos;
    }

    void HandleBackgroundClick()
    {
        if (!IsPrimaryPointerUp()) return;

        // Se arrastou a tela, nao fecha popup (foi so navegacao)
        if (FoiArrastado) return;

        if (LevelInfoPopup.Instance != null && LevelInfoPopup.Instance.painelTotal.activeSelf)
        {
            // Protecao de imunidade
            if (Time.time - LevelInfoPopup.Instance.TempoDeAbertura < 0.2f) return;

            Vector2 worldPoint = cam.ScreenToWorldPoint(GetPointerPosition());
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            bool clicouEmBotao = false;
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("LevelButton") || hit.collider.GetComponent<UnityEngine.UI.Button>() != null)
                    clicouEmBotao = true;
            }

            // Verifica UI tambem
            if (IsPointerOverBlockingUI()) clicouEmBotao = true;

            if (!clicouEmBotao)
            {
                LevelInfoPopup.Instance.Fechar();
            }
        }
    }

    private static Vector2 GetPointerPosition()
    {
        if (Input.touchCount > 0) return Input.GetTouch(0).position;
        return Input.mousePosition;
    }

    private static bool IsPrimaryPointerDown()
    {
        if (Input.touchCount > 0)
        {
            TouchPhase phase = Input.GetTouch(0).phase;
            return phase == TouchPhase.Began;
        }
        return Input.GetMouseButtonDown(0);
    }

    private static bool IsPrimaryPointerHeld()
    {
        if (Input.touchCount > 0)
        {
            TouchPhase phase = Input.GetTouch(0).phase;
            return phase == TouchPhase.Moved || phase == TouchPhase.Stationary;
        }
        return Input.GetMouseButton(0);
    }

    private static bool IsPrimaryPointerUp()
    {
        if (Input.touchCount > 0)
        {
            TouchPhase phase = Input.GetTouch(0).phase;
            return phase == TouchPhase.Ended || phase == TouchPhase.Canceled;
        }
        return Input.GetMouseButtonUp(0);
    }
}
