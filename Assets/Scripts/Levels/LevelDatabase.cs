using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Net; 

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Config/Level Database")]
public class LevelDatabase : ScriptableObject
{
    // --- CONFIGURAÇÃO VISUAL ---
    [System.Serializable]
    public class CategoriaIcone
    {
        public string idTipo; // Ex: "TEORIA", "MSA" (Igual à planilha)
        public Sprite icone;  // O desenho da pastinha
        public Color corTexto = Color.white; 
    }

    // --- DADOS DO NÍVEL ---
    [System.Serializable]
    public class LevelData
    {
        public int nivel;
        public string tituloAula;
        public string tipo;
    }

    [Header("1. Link do Google (CSV)")]
    // Seu link já está aqui!
    public string urlGoogleSheets = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRfUEx2sMAFhWL6u0KiEC2IjFNy1KssqeZQkIPEPiHzewhtn7v5c6MlqiXgZoRrqfUKTndgyK2jHc7E/pub?gid=1534533149&single=true&output=csv";

    [Header("2. Biblioteca de Ícones")]
    public List<CategoriaIcone> bibliotecaDeIcones;

    [Header("3. Lista Final (Gerada Autom.)")]
    public List<LevelData> todosOsNiveis;

    // --- BUSCA OS DADOS PARA O POPUP ---
    public (string titulo, Sprite icone, Color cor) GetInfo(int nivel)
    {
        // 1. Acha o nível na lista baixada
        var dados = todosOsNiveis.FirstOrDefault(x => x.nivel == nivel);
        
        if (dados != null)
        {
            // 2. Acha o ícone baseado no TIPO
            var config = bibliotecaDeIcones.FirstOrDefault(x => x.idTipo.ToUpper().Trim() == dados.tipo.ToUpper().Trim());
            
            Sprite icon = config != null ? config.icone : null;
            Color color = config != null ? config.corTexto : Color.white;

            return (dados.tituloAula, icon, color);
        }

        return ($"Nível {nivel}", null, Color.white);
    }

    // --- BOTÃO DE SINCRONIZAR ---
    [ContextMenu("☁️ SINCRONIZAR AGORA")]
    public void SincronizarGoogleSheets()
    {
        if (string.IsNullOrEmpty(urlGoogleSheets))
        {
            Debug.LogError("O Link do CSV está vazio!");
            return;
        }

        Debug.Log("Baixando planilha da nuvem...");
        string csvText = "";

        try
        {
            using (WebClient client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8; // Garante que acentos funcionem
                csvText = client.DownloadString(urlGoogleSheets);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao baixar: {e.Message}");
            return;
        }

        ProcessarCSV(csvText);
    }

    void ProcessarCSV(string texto)
    {
        todosOsNiveis = new List<LevelData>();
        string[] linhas = texto.Split('\n');
        int sucesso = 0;

        foreach (var linha in linhas)
        {
            if (string.IsNullOrWhiteSpace(linha)) continue;

            string[] colunas = linha.Split(',');

            // Precisa ter pelo menos 3 colunas (Level, Nome, Tipo)
            if (colunas.Length >= 3)
            {
                LevelData novo = new LevelData();

                // Coluna A: Level (Remove texto "Level" e pega só o número)
                string nivelLimpo = System.Text.RegularExpressions.Regex.Replace(colunas[0], "[^0-9]", "");
                
                if (int.TryParse(nivelLimpo, out int n))
                {
                    novo.nivel = n;
                    // Coluna B: Nome (Remove aspas extras do CSV)
                    novo.tituloAula = colunas[1].Replace("\"", "").Trim();
                    // Coluna C: Tipo
                    novo.tipo = colunas[2].Replace("\"", "").Trim().ToUpper(); 

                    todosOsNiveis.Add(novo);
                    sucesso++;
                }
            }
        }

        Debug.Log($"✅ SUCESSO! Baixados {sucesso} níveis da sua planilha.");
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}