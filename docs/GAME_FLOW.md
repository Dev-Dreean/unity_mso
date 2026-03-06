# Game Flow

## Visao geral

O projeto representa um mapa de progressao gamificado para conteudo musical. O jogador navega por um mapa 2D, abre niveis, consulta informacoes do nivel e interage com um portal externo que entrega progresso, conteudo e recompensas.

## Ponto de entrada

- Cena principal: `Assets/Scenes/SampleScene.unity`
- Bootstrap inicial: `Assets/Scripts/GameBootstrapper.cs`

O `GameBootstrapper` desliga blocos pesados no frame inicial, aplica ajustes de memoria para WebGL e abre o menu principal com atraso para reduzir problemas de memoria em mobile/web.

## Navegacao principal

- `MapNavigation.cs`: zoom, pan, limites da camera e fechamento de popup ao clicar no fundo.
- `MapWorldSwitcher.cs`: troca entre mundos do mapa.
- `AutoSwitchWorld.cs`: ajuda na troca automatica de mundo.
- `Portalgate.cs`: entrada/saida entre menu, mundo e castelo.

## Sistema de niveis

- `LevelDatabase.cs`: banco de dados dos niveis.
- `LevelNodesBootstrapper.cs`: monta os botoes/nos de nivel.
- `LevelInfoPopup.cs`: exibe titulo, icone e cor por nivel.
- `LevelNode.cs`: comportamento de cada no de nivel.

O arquivo `Assets/DB_LEVELS.asset` e a fonte principal de conteudo leve dentro do repositorio. Ele aponta para uma planilha Google Sheets em CSV e lista niveis com titulo e tipo, por exemplo:

- `TEORIA`
- `VOLUME`
- `BANCO DE QUESTOES`
- `PROVAS`
- `ESCALAS`
- `LEITURA RITMICA`

## Integracao externa

- `RadioSignal/RadioSignal.cs`
- `ADM_PAINEL/ExternalAdmin.cs`
- `Plugins/WebGL/*.jslib`

O `RadioSignal` e a ponte principal entre Unity e o host WebGL/iframe. Ele:

- recebe sessao e identidade de usuario;
- recebe `current_level`;
- recebe `level_content` com DRM/video;
- envia eventos como `level_click`, `level_play` e `level_complete`;
- processa recompensas e atualiza a energia.

## UI e estado do jogador

- `HUDManager.cs`
- `EnergyManager.cs`
- `SettingsLogic.cs`
- `SimplePopup.cs`
- `ProfileSignal.cs`
- `MenuManagerSimple.cs`
- `FullscreenToggle.cs`

Esses scripts cobrem HUD, saldo de energia, popup simples, perfil, menu e configuracoes de tela.

## Audio e video

- `AudioManager.cs`
- `MusicController.cs`
- `MusicManager.cs`
- `WebVideoFix.cs`

Esses componentes controlam trilha, audio por mundo e adaptacoes para reproducao em WebGL.

## Ferramentas de editor

- `Assets/Editor/FindMissing.cs`
- `Assets/Editor/LevelNodesAutoSetup.cs`
- `Assets/Editor/WebGLIosMemoryTools.cs`

Essas rotinas apoiam setup de nos, deteccao de referencias ausentes e perfis/guardas de memoria para build WebGL.
