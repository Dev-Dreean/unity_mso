# Control Map

Este documento responde duas perguntas:

1. Quem controla o que dentro do jogo.
2. Em quais arquivos voce precisa mexer para cada tipo de alteracao.

## Cadeia principal de controle

### Entrada do jogo

- `GameBootstrapper.cs` controla o inicio seguro da runtime.
- `MenuManagerSimple.cs` decide se o botao de iniciar fica bloqueado ou liberado.
- `RadioSignal.SessionReady` e o gatilho que libera o jogo para o usuario entrar.

Fluxo:

`GameBootstrapper` -> `MenuManagerSimple` -> `RadioSignal.SessionReady` -> gameplay ativo

### Navegacao do mapa

- `MapWorldSwitcher.cs` controla qual mundo esta visivel.
- `MapNavigation.cs` controla camera, zoom, arrasto e limite do mapa.
- `Portalgate.cs` controla transicoes entre menu, mapa e castelo.
- `AutoSwitchWorld.cs` ajuda a reposicionar/trocar mundo automaticamente.

Fluxo:

`MapWorldSwitcher` define mundo ativo -> `MapNavigation` aplica limites/camera -> `LevelNode` reage ao clique no mapa

### Niveis e popup

- `LevelNodesBootstrapper.cs` instancia/configura os nos.
- `LevelNode.cs` controla clique, bloqueio e bind com `RadioSignal`.
- `LevelInfoPopup.cs` abre o popup, consome energia e manda o evento de nivel.
- `LevelDatabase.cs` fornece titulo, tipo, icone e cor do nivel.
- `Assets/DB_LEVELS.asset` armazena os dados reais dos niveis.

Fluxo:

`DB_LEVELS.asset` -> `LevelDatabase` -> `LevelNodesBootstrapper` / `LevelInfoPopup` -> `LevelNode` -> `RadioSignal`

### Sessao, progresso e host externo

- `RadioSignal.cs` e o controlador central da integracao web.
- `ExternalAdmin.cs` recebe comandos JS administrativos.
- `Plugins/WebGL/*.jslib` fazem a ponte Unity <-> JavaScript.
- `ProfileSignal.cs` trata dados de perfil.

Fluxo:

host externo -> `RadioSignal.OnJsMessage()` -> atualiza sessao/progresso/recompensa -> UI e gameplay reagem

### Energia e restricoes

- `EnergyManager.cs` controla saldo, premium, recarga e consumo.
- `LevelInfoPopup.cs` chama `ConsumirEnergia()` antes de abrir nivel.
- `RadioSignal` pode adicionar energia via mensagem de recompensa.
- `ExternalAdmin` tambem pode adicionar energia em fluxo administrativo.

Fluxo:

`RadioSignal` ou `ExternalAdmin` -> `EnergyManager.AtualizarDados()` / `AdicionarEnergia()` -> `LevelInfoPopup` consulta `ConsumirEnergia()`

### HUD, menu e configuracoes

- `HUDManager.cs` abre/fecha menu em jogo.
- `SimplePopup.cs` controla popups simples.
- `SettingsLogic.cs` ajusta audio/config.
- `FullscreenToggle.cs` controla fullscreen.
- `ButtonResizer.cs`, `ScalePulse` e `UIButtonFlipbook` cuidam de feedback visual.

### Audio e video

- `AudioManager.cs` e o hub principal de audio.
- `MusicController.cs` troca trilha por estado/bioma.
- `MusicManager.cs` cuida do audio do mundo.
- `WebVideoFix.cs` corrige reproducao em web.

### Ferramentas de editor

- `LevelNodesAutoSetup.cs` automatiza setup de nos.
- `FindMissing.cs` ajuda a encontrar referencias faltando.
- `WebGLIosMemoryTools.cs` ajusta perfil de memoria/build para WebGL iOS.

## Se voce quer mudar X, mexa em Y

### Liberar ou travar entrada no jogo

- `Assets/Scripts/UI/MenuManagerSimple.cs`
- `Assets/Scripts/RadioSignal/RadioSignal.cs`

Procure por:

- `RadioSignal.SessionReady`
- `LiberarOJogo()`
- `BloquearOJogo()`

### Alterar comportamento do menu inicial

- `Assets/Scripts/GameBootstrapper.cs`
- `Assets/Scripts/UI/MenuManagerSimple.cs`
- `Assets/Scripts/IntroManager.cs`

### Mudar camera, zoom, pan ou limites do mapa

- `Assets/Scripts/MapNavigation.cs`
- `Assets/Scripts/MapWorldSwitcher.cs`

Procure por:

- `SetBounds`
- `HandleZoom`
- `HandlePan`
- `ClampCamera`
- `Show(...)`

### Trocar mundo, castelo ou destino de portal

- `Assets/Scripts/MapWorldSwitcher.cs`
- `Assets/Scripts/Portalgate.cs`
- `Assets/Scripts/AutoSwitchWorld.cs`

### Mudar quais niveis existem ou seus nomes/tipos

- `Assets/DB_LEVELS.asset`
- `Assets/Scripts/Levels/LevelDatabase.cs`

Procure por:

- `todosOsNiveis`
- `bibliotecaDeIcones`
- `urlGoogleSheets`
- `GetInfo(int nivel)`

### Mudar popup do nivel

- `Assets/Scripts/Levels/LevelInfoPopup.cs`

Procure por:

- `Abrir`
- `AssistirAula`
- `Fechar`

### Mudar logica de bloqueio/desbloqueio de fases

- `Assets/Scripts/UI/LevelNode.cs`
- `Assets/Scripts/UI/LevelManager1.cs`
- `Assets/Scripts/Levels/LevelNodesBootstrapper.cs`
- `Assets/Scripts/RadioSignal/RadioSignal.cs`

### Mudar consumo, recarga ou saldo de energia

- `Assets/Scripts/UI/EnergyManager.cs`
- `Assets/Scripts/Levels/LevelInfoPopup.cs`
- `Assets/Scripts/RadioSignal/RadioSignal.cs`
- `Assets/Scripts/ADM_PAINEL/ExternalAdmin.cs`

Procure por:

- `ConsumirEnergia`
- `AdicionarEnergia`
- `AtualizarDados`
- `popupSemEnergia`

### Mudar comunicacao com o site/app host

- `Assets/Scripts/RadioSignal/RadioSignal.cs`
- `Assets/Scripts/Plugins/WebGL/PlansulBridge.jslib`
- `Assets/Scripts/Plugins/WebGL/WebGLBridge.jslib`
- `Assets/Scripts/Plugins/WebGL/YouTubeOverlay.jslib`
- `Assets/Scripts/ADM_PAINEL/ExternalAdmin.cs`

Procure por:

- `SendLevelClick`
- `SendLevelPlay`
- `SendLevelComplete`
- `OnJsMessage`
- `JS_SetLevel`
- `JS_SetPremium`
- `JS_AddEnergy`

### Mudar audio do jogo

- `Assets/Scripts/AudioManager.cs`
- `Assets/Scripts/MusicController.cs`
- `Assets/Scripts/MusicManager.cs`
- `Assets/Scripts/SettingsLogic.cs`

### Mudar fullscreen e comportamento WebGL

- `Assets/Scripts/FullscreenToggle.cs`
- `Assets/Scripts/WebVideoFix.cs`
- `Assets/Editor/WebGLIosMemoryTools.cs`
- `Assets/Scripts/Plugins/WebGL/UnityFullscreen.jslib`

## Ordem recomendada para qualquer manutencao

1. Descubra se a mudanca nasce da UI, do mapa, do nivel, da energia ou da integracao host.
2. Abra primeiro o controlador principal daquela area.
3. Veja quem ele chama e quem chama ele.
4. So depois altere scripts auxiliares.
5. Se a mudanca tocar progressao, valide `RadioSignal`, `EnergyManager` e `LevelNode` juntos.

## Arquivos que servem de ancora para entendimento

- `Assets/Scenes/SampleScene.unity`: ligacoes principais da cena.
- `Assets/DB_LEVELS.asset`: dados dos niveis e tipos.
- `Packages/manifest.json`: stack e dependencias Unity.
- `ProjectSettings/EditorBuildSettings.asset`: cena incluida no build.
- `ProjectSettings/ProjectVersion.txt`: versao exata do Unity.
