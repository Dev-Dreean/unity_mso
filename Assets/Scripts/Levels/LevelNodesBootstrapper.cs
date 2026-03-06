using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class LevelNodesBootstrapper : MonoBehaviour
{
    [Header("Onde varrer (se vazio usa este GameObject)")]
    public Transform root;

    [Header("Filtro de Segurança")]
    [Tooltip("Se marcado, SÓ altera botões que começam com 'Level_'. Ignora botões de UI, Castelo, etc.")]
    public bool useNameFilter = true;
    public string prefixFilter = "Level_";

    [Header("Configuração Inteligente")]
    [Tooltip("Se marcado, tenta ler o número escrito no Texto do botão ou no Nome do objeto.")]
    public bool readNumberFromText = true; 
    public int startLevel = 1;

    [Header("Overlay de Lock")]
    public bool createOverlayIfMissing = true;
    public string overlayName = "LockOverlay";

    [Header("Ponte")]
    public RadioSignal radio;

    void Reset()
    {
        if (!root) root = transform;
    }

    [ContextMenu("Bootstrap Now")]
    public void BootstrapNow()
    {
        if (!root) root = transform;
        if (!radio) radio = FindRadio();

        var buttons = root.GetComponentsInChildren<Button>(true);
        int contadorManual = startLevel;
        int configurados = 0;
        int ignorados = 0;

        foreach (var btn in buttons)
        {
            if (!btn) continue;

            // --- TRAVA DE SEGURANÇA (NOVO) ---
            // Se o botão não começar com "Level_", a gente PULA ele.
            if (useNameFilter && !string.IsNullOrEmpty(prefixFilter))
            {
                if (!btn.name.StartsWith(prefixFilter))
                {
                    ignorados++;
                    continue; // Pula para o próximo
                }
            }
            // ---------------------------------

            // Garante LevelNode
            var node = btn.GetComponent<LevelNode>();
            if (!node) node = btn.gameObject.AddComponent<LevelNode>();

            int levelFinal = 0;

            // Lógica Inteligente de Leitura
            if (readNumberFromText)
            {
                var textoTMP = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (textoTMP != null && int.TryParse(textoTMP.text, out int nTexto))
                {
                    levelFinal = nTexto;
                }
                else if (btn.name.Contains("_"))
                {
                    string[] partes = btn.name.Split('_'); 
                    if (partes.Length > 1 && int.TryParse(partes[1], out int nNome))
                    {
                        levelFinal = nNome;
                    }
                }
            }

            if (levelFinal == 0)
            {
                levelFinal = contadorManual;
                contadorManual++;
            }

            node.SetLevelNumber(levelFinal);

            // Overlay
            var overlay = FindOverlay(btn.transform) ?? (createOverlayIfMissing ? CreateOverlay(btn.transform) : null);

            node.Bind(btn, overlay, radio);
            node.ApplyLocked(node.levelNumber > 1);
            
            configurados++;
        }

        Debug.Log($"[Bootstrapper] FIM! {configurados} botões configurados. {ignorados} botões ignorados (Castelo/UI).");
    }

    CanvasGroup FindOverlay(Transform t)
    {
        return t.GetComponentsInChildren<CanvasGroup>(true)
                .FirstOrDefault(x => x && (x.name == overlayName || x.name.ToLower().Contains("lock")));
    }

    CanvasGroup CreateOverlay(Transform t)
    {
        var go = new GameObject(overlayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        go.transform.SetParent(t, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        var img = go.GetComponent<Image>();
        img.raycastTarget = true;
        img.color = new Color(0, 0, 0, 0.25f);

        var cg = go.GetComponent<CanvasGroup>();
        cg.alpha = 0.6f; cg.blocksRaycasts = true;
        return cg;
    }

#if UNITY_2023_1_OR_NEWER
    RadioSignal FindRadio()
    {
        var r = Object.FindFirstObjectByType<RadioSignal>();
        if (!r) r = Object.FindAnyObjectByType<RadioSignal>();
        return r;
    }
#else
    RadioSignal FindRadio() => Object.FindObjectOfType<RadioSignal>();
#endif
}