using UnityEngine;
using UnityEngine.UI;

public class PortalGate : MonoBehaviour
{
    // AGORA SIM: Os nomes batem com seus objetos
    public enum Destino { Menu = 0, MapWorld = 1, Castelo = 2 }

    [Header("Para onde vamos?")]
    public Destino irPara; 

    [Header("Bloqueio (Opcional)")]
    public int nivelNecessario = 0; 
    public GameObject cadeadoVisual;

    private Button btn;
    private MapWorldSwitcher switcher;
    private LevelManager1 levelManager;

    void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(AoClicar);

        switcher = FindFirstObjectByType<MapWorldSwitcher>();
        levelManager = FindFirstObjectByType<LevelManager1>();

        if (levelManager) 
        {
            bool bloqueado = levelManager.unlockedMax < nivelNecessario;
            btn.interactable = !bloqueado;
            if (cadeadoVisual) cadeadoVisual.SetActive(bloqueado);
        }
    }

    void AoClicar()
    {
        if (switcher)
        {
            // O MapWorld é o índice 1 na lista do Switcher
            switcher.Show((int)irPara);
        }
    }
}