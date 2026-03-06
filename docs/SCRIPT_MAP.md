# Script Map

## Bootstrap e runtime base

- `Assets/Scripts/GameBootstrapper.cs`: inicializacao segura para menu/gameplay e perfil de memoria WebGL.
- `Assets/Scripts/IntroManager.cs`: logica de abertura/intro.
- `Assets/Scripts/DebugSimulator.cs`: simulacao e apoio de debug.

## Mapa e navegacao

- `Assets/Scripts/MapNavigation.cs`: camera, zoom, drag e clique fora do popup.
- `Assets/Scripts/MapWorldSwitcher.cs`: mundos/configuracoes do mapa.
- `Assets/Scripts/AutoSwitchWorld.cs`: transicao automatica de mundo.
- `Assets/Scripts/Portalgate.cs`: portal entre estados/cenas logicas.
- `Assets/Scripts/CameraPanVertical.cs`: movimento vertical de camera.

## Niveis e progressao

- `Assets/Scripts/Levels/LevelDatabase.cs`: modelo e consulta dos niveis.
- `Assets/Scripts/Levels/LevelNodesBootstrapper.cs`: cria/configura nos.
- `Assets/Scripts/Levels/LevelInfoPopup.cs`: popup com informacoes do nivel.
- `Assets/Scripts/UI/LevelNode.cs`: comportamento do no clicavel.
- `Assets/Scripts/UI/LevelManager1.cs`: fluxo auxiliar de nivel.
- `Assets/Scripts/UI/NivelFlag.cs`: estrutura simples de sinalizacao.

## Integracao host/web

- `Assets/Scripts/RadioSignal/RadioSignal.cs`: canal de mensagens host <-> Unity.
- `Assets/Scripts/ADM_PAINEL/ExternalAdmin.cs`: integracao administrativa externa.
- `Assets/Scripts/Plugins/WebGL/PlansulBridge.jslib`: bridge principal.
- `Assets/Scripts/Plugins/WebGL/WebGLBridge.jslib`: ponte JS generica.
- `Assets/Scripts/Plugins/WebGL/UnityFullscreen.jslib`: fullscreen no navegador.
- `Assets/Scripts/Plugins/WebGL/YouTubeOverlay.jslib`: suporte a overlay/video.
- `Assets/Scripts/WebVideoFix.cs`: ajustes de video para web.

## UI e feedback

- `Assets/Scripts/HUDManager.cs`: HUD geral.
- `Assets/Scripts/UI/MenuManagerSimple.cs`: menu principal.
- `Assets/Scripts/SettingsLogic.cs`: configuracoes.
- `Assets/Scripts/SimplePopup.cs`: popup generico.
- `Assets/Scripts/ButtonResizer.cs`: feedback de botao.
- `Assets/Scripts/Movimento - Sprite button.cs`: animacao de sprite de botao.
- `Assets/Scripts/Movimento_exit.cs`: efeito de pulsacao.
- `Assets/Scripts/DailyHighlight.cs`: destaque diario.
- `Assets/Scripts/FullscreenToggle.cs`: alterna fullscreen.
- `Assets/Scripts/UI/ProfileSignal.cs`: dados/sinal do perfil.

## Recursos do jogador

- `Assets/Scripts/UI/EnergyManager.cs`: energia/recompensas.

## Audio

- `Assets/Scripts/AudioManager.cs`: audio geral.
- `Assets/Scripts/MusicController.cs`: selecao de musica por bioma/mundo.
- `Assets/Scripts/MusicManager.cs`: controle de audio de mundo.

## Ferramentas de editor

- `Assets/Editor/FindMissing.cs`: encontra referencias faltando.
- `Assets/Editor/LevelNodesAutoSetup.cs`: automatiza setup de nos.
- `Assets/Editor/WebGLIosMemoryTools.cs`: utilitarios e guardas para memoria/build WebGL iOS.
