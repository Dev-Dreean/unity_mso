# MSO GameMap

Repositorio enxuto para suporte de codigo e documentacao do projeto Unity.

## Objetivo

Este repositorio foi reduzido para ficar leve e facil de usar com assistentes como Gemini, mantendo:

- scripts C# e bridges WebGL;
- documentacao funcional do jogo;
- arquivos de contexto leves do Unity;
- cena principal e banco de niveis em formato texto.

Assets pesados, audio, imagens, videos, `Library/` e afins continuam apenas no ambiente local.

## Stack

- Unity `6000.2.14f1`
- URP / 2D
- WebGL como alvo importante
- Integracao com host externo via bridge JavaScript

## Arquivos de contexto mantidos no Git

- `Assets/Scripts/` e `Assets/Editor/`
- `Assets/Scenes/SampleScene.unity`
- `Assets/DB_LEVELS.asset`
- `Packages/manifest.json`
- `ProjectSettings/ProjectVersion.txt`
- `ProjectSettings/EditorBuildSettings.asset`
- `docs/`

## Leituras recomendadas

- `docs/GAME_FLOW.md`
- `docs/SCRIPT_MAP.md`

## Observacao

Se em algum momento voce quiser uma versao "boa para Unity abrir do zero", vai ser melhor voltar a rastrear tambem os arquivos `.meta`, alguns assets de configuracao e talvez `Packages/packages-lock.json`.
