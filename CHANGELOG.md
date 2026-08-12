# Changelog

All notable changes to this project will be documented in this file.

## [v0.3.7] - 2026-08-12

### Added / Adicionado
- **[English]** **Crash Shielding & Multi-Layer Resilience:** Implemented 5-layer shielding architecture to prevent configuration and icon data loss when Stream Dock crashes or closes abruptly due to third-party plugin errors.
- **[Português]** **Blindagem Contra Travamentos & Resiliência Multi-Camada:** Implementada arquitetura de blindagem em 5 camadas para proteger whitelists (Games Whitelist, Software Whitelist, Blacklist) e ícones personalizados contra perda de dados em travamentos do software Stream Dock.
- **[English]** **Physical Icon File Storage (`icons/`):** Custom icons are now automatically saved as physical image files in `%APPDATA%\com.alazter.audio_controller.sdPlugin\icons\`. If settings are corrupted, icons are auto-recovered from disk.
- **[Português]** **Armazenamento Físico de Ícones (`icons/`):** Ícones personalizados agora são salvos como arquivos de imagem físicos em `%APPDATA%\com.alazter.audio_controller.sdPlugin\icons\`. Se o arquivo de perfis for corrompido, os ícones são auto-recuperados automaticamente do disco.
- **[English]** **Atomic File Writes & Automatic Backups:** `saveData()` writes to `profiles.json.tmp` before atomically swapping, while generating `profiles.json.bak` backups to prevent 0-byte file corruption during unexpected system kills.
- **[Português]** **Gravação Atômica e Backup Automático:** `saveData()` grava primeiro em `profiles.json.tmp` antes de realizar a substituição atômica, criando um backup `profiles.json.bak` para impedir arquivos zerados ou corrompidos em quedas abruptas.
- **[English]** **Non-Destructive List Merging:** Incoming empty setting payloads (`{}`) sent by Stream Dock during post-crash restarts are safely merged, preserving local AppData whitelists and profiles.
- **[Português]** **Fusão Não-Destrutiva de Listas:** Payloads vazios (`{}`) enviados pelo Stream Dock ao reiniciar após crashes são mesclados de forma não-destrutiva, preservando a Games Whitelist e a Whitelist salvas localmente.

---

## [v0.3.6] - 2026-08-10

### Fixed / Corrigido
- **[English]** **Multi-Audio Endpoint & Voicemeeter Support:** Updated WASAPI session enumeration in `VolumeControl.exe` from querying solely the Windows default playback endpoint to enumerating ALL active audio rendering devices (`IMMDeviceCollection`). Fixes control and detection for applications (such as Discord, Spotify, Chrome) routed to custom virtual audio cables (VB-Audio Cable A/B, Voicemeeter VAIO, SteelSeries Sonar, GoXLR, etc.).
- **[Português]** **Suporte a Múltiplos Dispositivos de Áudio e Voicemeeter:** Atualizada a enumeração de sessões de áudio no executável C# (`VolumeControl.exe`) para buscar em TODOS os dispositivos de saída ativos do Windows (`IMMDeviceCollection`), em vez de consultar apenas o dispositivo padrão. Resolve o controle de volume e mudo de aplicativos (como Discord) configurados para sair em cabos virtuais como CABLE-A, CABLE-B, Voicemeeter, SteelSeries Sonar, etc.

---

## [v0.3.5] - 2026-07-13

### Added / Adicionado
- **[English]** **Foreground Game Priority:** In Games Whitelist autofocus mode, the game focused in first-person (active window) now has maximum priority over background games (such as `taskbarhero.exe`).
- **[Português]** **Priorização de Jogo em Primeiro Plano:** No autofoco da Games Whitelist, o jogo focado na tela (janela ativa) agora possui prioridade máxima sobre jogos rodando em segundo plano (como `taskbarhero.exe`).
- **[English]** **Temporary Focus with Auto-Return Timeout:** Cycling to an app in Games Whitelist mode now holds focus temporarily for 60 seconds (renewed on every interaction/rotation). It then automatically returns to the active foreground game.
- **[Português]** **Foco Temporário com Retorno Automático:** Ao alternar (ciclar) para outro jogo da Games Whitelist pelo knob físico/tela, o foco fica fixo por 1 minuto (renovado a cada giro/clique). Após 1 minuto de inatividade, o foco retorna de forma automática para o jogo em primeiro plano.
- **[English]** **Automatic Option in selector:** Added "Automatic (Auto Focus)" to the Controlling App dropdown in Property Inspector for Games Whitelist mode.
- **[Português]** **Opção Automática no Seletor:** Adicionada a opção "Automatic (Auto Focus)" no menu de seleção de aplicativos no computador para a Games Whitelist.

### Fixed / Corrigido
- **[English]** **Disabled Debug Logging:** Deactivated debug log file writing (`plugin_debug.log`) in the plugin folder to avoid unnecessary SSD/NVMe read/write cycles.
- **[Português]** **Desativação de Logs de Depuração:** Desativada a gravação de arquivos de log local (`plugin_debug.log`) para economizar ciclos de leitura/escrita e proteger o SSD/NVMe.

---

## [v0.3.4] - 2026-07-13

### Fixed / Corrigido
- **[English]** **Restaured Knob controller:** Reverted controller type from `Encoder` back to `Knob` in `manifest.json`. The Mirabox Stream Dock software rejects the `Encoder` value causing the plugin to disappear or not be draggable. Note: Due to Mirabox API limitations, screen touch and knob press are physically fused into the same WebSocket event (`dialDown`), making separation impossible on this hardware.
- **[Português]** **Restauração do controlador Knob:** Revertido o tipo de controlador de `Encoder` de volta para `Knob` no `manifest.json`. O software da Mirabox rejeita o valor `Encoder`, fazendo com que o plugin sumisse ou não pudesse ser arrastado. Nota: Devido a limitações da API da Mirabox, o toque na tela e o clique no botão físico são fundidos no mesmo evento de WebSocket (`dialDown`), impossibilitando a separação física de ações neste hardware.

---

## [v0.3.3] - 2026-07-13

### Changed / Alterado
- **[English]** **Controller declaration change:** Updated plugin controllers declaration in `manifest.json` from `Knob` to `Encoder`. This triggers the full Elgato Stream Deck API compatibility layer on the Mirabox Stream Dock software, enabling isolated events (`touchTap` and separate clicks).
- **[Português]** **Alteração de Declaração do Controlador:** Alterada a declaração de controladores no `manifest.json` de `Knob` para `Encoder`. Isso ativa a camada completa de compatibilidade com a API da Elgato no software da Mirabox, ativando eventos isolados de toque e cliques.

---

## [v0.3.2] - 2026-07-13

### Fixed / Corrigido
- **[English]** **Toque vs Knob Click Logic separation:** Implemented logic to differentiate screen touch (`touchTap` and `dialDown` events with `tapPos` payload) from physical knob pressing (`dialDown` without `tapPos`). This allows different actions on screen tap vs knob click.
- **[Português]** **Separação de Toque na Tela vs Clique do Knob:** Diferenciado de forma robusta o toque na tela LCD (`touchTap` e `dialDown` com coordenadas de toque `tapPos` no payload) do clique no knob físico (`dialDown` sem `tapPos`), permitindo configurar ações independentes.
- **[English]** **Local Debug Logging Mode:** Enabled a secure, local debug file `plugin_debug.log` written in the plugin folder to analyze hardware event patterns if needed.
- **[Português]** **Modo de Log de Debug Local:** Habilitada a gravação de logs locais e silenciosos no arquivo `plugin_debug.log` na pasta do plugin para análise opcional de assinaturas de hardware.

---

## [v0.3.1] - 2026-07-13

### Fixed / Corrigido
- **[English]** **IPC Correlation ID Sychronization:** Upgraded Node.js <-> C# communication with transactional Correlation IDs (`id|payload`). This eliminates all possibilities of command/response misalignment, resolving the "swapped app icons/controls" bug.
- **[Português]** **Sincronização IPC com Correlation IDs:** Atualizado canal de comunicação entre Node.js e C# com IDs de correlação transacionais (`id|payload`), eliminando desalinhamento de comandos e resolvendo a exibição de ícones e controles trocados.
- **[English]** **Dynamic Path Cache Invalidation:** Implemented dynamic cache invalidation for closed applications. Once an application is opened and registered in the system audio sessions, its `'NOT_FOUND'` cache entry is invalidated, loading its icon instantly.
- **[Português]** **Invalidação Dinâmica de Cache de Caminhos:** Corrigido problema onde ícones nativos não carregavam se a aplicação fosse aberta após a inicialização do plugin. Agora o cache `'NOT_FOUND'` é limpo de imediato assim que a aplicação é iniciada.
- **[English]** **UWP/Store App Native Icon Resolution:** Added a native Windows API fallback (`QueryFullProcessImageName` via P/Invoke) in the C# executable. Resolves access denied errors when querying path modules for UWP Store apps (like Spotify) and administrative processes.
- **[Português]** **Resolução Nativa de Caminhos UWP/Admin:** Adicionado fallback usando API nativa do Windows (`QueryFullProcessImageName` no C#) para obter os executáveis e ícones de aplicativos UWP (Windows Store, como Spotify) e processos Administradores sem erros de privilégios.

---

## [v0.3.0] - 2026-07-13

### Added / Adicionado
- **[English]** **Intelligent Game Detection:** Automatically detects when any game from your Games Whitelist is open (via active audio sessions) and switches the knob focus to it. Keeps controlling the game even when you Alt-Tab out of it (e.g. to Discord).
- **[Português]** **Detecção Inteligente de Jogos:** Detecta automaticamente quando qualquer jogo da sua Games Whitelist é aberto (através de sessões de áudio ativas) e foca os botões nele. O controle continua ativo mesmo se você der Alt-Tab (ex: para o Discord).
- **[English]** **Persistent IPC Architecture:** Replaced all one-shot child process executions with a single persistent background C# instance. Communication now flows entirely in memory, reducing CPU usage to 0.00% and NVMe/SSD read cycles to zero.
- **[Português]** **Arquitetura IPC Persistente:** Substituído todo o spawn de subprocessos por uma única instância em background do C# que conversa via memória. Isso reduz o uso de CPU a 0% e elimina ciclos de leitura no SSD/NVMe.

---

## [v0.2.4] - 2026-07-06

### Added / Adicionado
- **[English]** **Audio Controller Namespace Transition:** Renamed the entire plugin and action UUID to `com.alazter.audio_controller.sdPlugin` to establish a clearer name.
- **[Português]** **Transição para Namespace Audio Controller:** Renomeado todo o plugin e UUID de ação para `com.alazter.audio_controller.sdPlugin` para estabelecer uma identidade de nome mais clara.
- **[English]** **Plugin Rebranding:** Renamed plugin name, action name, action tooltip, and category in all translations and manifests to `Audio Controller [Alazter]`.
- **[Português]** **Renomeação do Plugin:** Renomeado o nome do plugin, nome da ação, tooltip de ação e categoria em todas as traduções e manifestos para `Audio Controller [Alazter]`.
- **[English]** **Settings Auto-Migration Fallback:** Implemented multi-stage automatic migration in the backend server. It now searches for old profiles from both `com.alazter.mirabox.volume.sdPlugin` and the original `com.mirabox.streamdock.time.sdPlugin` directories, preventing loss of settings.
- **[Português]** **Migração Automática Avançada:** Implementado suporte a migração automática em múltiplos estágios no servidor backend, buscando perfis anteriores nos diretórios `com.alazter.mirabox.volume.sdPlugin` e `com.mirabox.streamdock.time.sdPlugin` para evitar perda de dados.

---

## [v0.2.2] - 2026-07-05

### Fixed / Corrigido
- **[English]** **Base64 Payload Throttling:** Prevented re-sending 100KB Base64 image payloads in `setFeedback` during volume adjustments, reducing WebSocket data traffic by 99.9% and eliminating QML UI thread freezes in Stream Dock.
- **[Português]** **Otimização de Payload Base64:** Eliminado o envio repetido de imagens Base64 de ~100KB no `setFeedback` durante giros de volume, reduzindo o tráfego em 99.9% e impedindo o congelamento da interface visual (Qt/QML) do Stream Dock.
- **[English]** **Focus Resolution Timeout Guard:** Added an 800ms `Promise.race` timeout to active window focus resolution (`getFocusedWindowAsync`), ensuring full-screen games never hang background polling.
- **[Português]** **Proteção de Timeout de Foco:** Adicionado limite de 800ms com `Promise.race` para consulta de janela ativa, garantindo que jogos em tela cheia nunca travem a fila de segundo plano.

---

## [v0.2.1] - 2026-07-05

### Fixed / Corrigido
- **[English]** **Complete Elimination of Synchronous Blockers:** Replaced all `activeWin.sync()` and `execSync` calls with non-blocking async `await activeWin()` and asynchronous Promises (`exec`), completely preventing Stream Dock UI freezes.
- **[Português]** **Eliminação Completa de Bloqueios Síncronos:** Substituídas todas as chamadas síncronas `activeWin.sync()` e `execSync` por operações assíncronas `await activeWin()` e `exec` com Promises, impedindo totalmente o congelamento da interface do Stream Dock.

---

## [v0.2.0] - 2026-07-05

### Added / Adicionado
- **[English]** **Dedicated Games Whitelist:** Introduced a dedicated Games Whitelist alongside the Software Whitelist, allowing users to separate work/media apps (Discord, Spotify, Chrome) from games (League of Legends, CS:GO, GTA).
- **[Português]** **Whitelist Dedicada de Jogos:** Criada uma Whitelist exclusiva para Jogos ao lado da Whitelist de Softwares, permitindo separar o controle de volume de aplicativos de trabalho/mídia (Discord, Spotify, Chrome) dos seus jogos (League of Legends, CS:GO, GTA).
- **[English]** **App Pool Selection:** Added "Games Whitelist" as a selectable option in the Knob's App Pool radio group, giving complete control over which application list each knob rotates or tracks.
- **[Português]** **Seleção de App Pool:** Adicionada a opção "Games Whitelist" no grupo de seleção do App Pool do Knob, permitindo escolher se o botão alterna apenas entre softwares, apenas entre jogos ou por todos os apps.
- **[English]** **Quick Add Buttons:** Enhanced the Detected Audio Processes section with `💻` (Add to Software Whitelist) and `🎮` (Add to Games Whitelist) action buttons.
- **[Português]** **Botões Rápidos de Cadastro:** Atualizada a seção de Processos Detectados com botões diretos para enviar aplicativos para a Whitelist de Software (`💻`) ou para a Whitelist de Jogos (`🎮`).

---

## [v0.1.1] - 2026-07-05

### Fixed / Corrigido
- **[English]** **Icon Cache Failure Loop Fix:** Cached failed icon extractions (`null`) to eliminate continuous synchronous `execSync` process spawning loops every 500ms for apps without extractable icons.
- **[Português]** **Eliminação de Loop no Cache de Ícones:** Adicionado cache de falha (`null`) para evitar que a extração de ícones tente re-executar o `VolumeControl.exe` de forma síncrona a cada 500ms em aplicativos sem ícone.
- **[English]** **Qt Base64 SVG Crash Fix:** Removed nested Base64 SVG image generation on muted state to prevent Qt image rendering crashes in recent Stream Dock updates.
- **[Português]** **Correção de Crash no Renderizador Qt:** Remoção da geração de SVGs com Base64 aninhado para estados mudos, evitando congelamentos do motor de renderização da Qt nas atualizações recentes do Stream Dock.
- **[English]** **WebSocket Event Deduplication:** Throttled and deduplicated `setTitle`, `setImage`, and `setFeedback` WebSocket events, sending updates only when values actually change.
- **[Português]** **Deduplicação de Mensagens WebSocket:** Filtragem e controle de frequência dos eventos `setTitle`, `setImage` e `setFeedback`, reduzindo o tráfego WebSocket e a carga na UI do Stream Dock.
- **[English]** **Focus Tracking Loop Optimization:** Optimized the background active window polling loop (`activeWin`) to execute only when automatic focus tracking is required, spaced at 1000ms intervals.
- **[Português]** **Otimização de Checagem de Foco:** O loop de verificação de janela ativa (`activeWin`) agora é executado apenas quando necessário (knobs em modo automático) e espaçado para 1000ms.

---

## [v0.1.0] - 2026-07-04

### Added / Adicionado
- **[English]** **Standalone Namespace Transition:** Decoupled the plugin identifier from the official Mirabox Clock plugin (`com.mirabox.streamdock.time.sdPlugin`) to our independent namespace `com.alazter.mirabox.volume.sdPlugin` to prevent Stream Dock app updates from overwriting custom configurations.
- **[Português]** **Transição para Namespace Standalone:** Desconexão total do identificador de plugin oficial da Mirabox (`com.mirabox.streamdock.time.sdPlugin`) para o namespace exclusivo `com.alazter.mirabox.volume.sdPlugin`, protegendo o plugin personalizado contra substituições automáticas durante atualizações do software Stream Dock.
- **[English]** **Automatic Configuration Migration:** Integrated a migration check that automatically copies previous user configurations (whitelist, blacklist, and custom icons) from the old AppData folder to the new standalone storage path on startup.
- **[Português]** **Migração Automática de Configuração:** Adicionado mecanismo de importação transparente que copia as whitelists, blacklists e ícones customizados do diretório antigo do AppData para o novo armazenamento independente na primeira inicialização.
- **[English]** **Clean Translations:** Overhauled `es.json` and `ja.json` files to remove obsolete clock/timer entries and add clean Japanese/Spanish translations for the standalone Volume Controller.
- **[Português]** **Localizações Limpas:** Reestruturação completa das traduções nos arquivos `es.json` e `ja.json` para expurgar referências antigas do relógio original e fornecer localizações corretas para o volume de aplicativos em espanhol e japonês.

---

## [v1.1.1] - 2026-06-04

### Fixed / Corrigido
- **[English]** **Custom Icon Resize:** Resized custom application and mute overlay icons to a native 144x144 pixels resolution at the Property Inspector source to prevent massive Base64 payloads from crashing the Stream Dock WebSocket buffer and terminating the plugin backend.
- **[Português]** **Redimensionamento de Ícones Customizados:** Redimensionamento automático de ícones de aplicativos e de mute para a resolução nativa de 144x144 pixels diretamente no Property Inspector, evitando payloads imensos em Base64 que causavam travamento no buffer do WebSocket e derrubavam o backend do plugin.

---

## [v1.1.0] - 2026-06-01

### Added / Adicionado
- **[English]** **AppData Persistence:** Migrated `profiles.json` and custom Base64 app icons to the global `%APPDATA%` directory to shield them from Git rollbacks, cleans, and Stream Dock plugin updates.
- **[Português]** **Persistência no AppData:** Migração do `profiles.json` e ícones em Base64 para a pasta global `%APPDATA%`, protegendo as configurações do usuário contra limpezas do Git, rollbacks ou atualizações de software do Stream Dock.
- **[English]** **Auto-Migration:** Implemented seamless auto-migration of old local profiles to `%APPDATA%` upon plugin startup.
- **[Português]** **Migração Automática:** Importação automatizada das preferências locais anteriores na primeira execução após a atualização.
- **[English]** **Custom Process Blacklist:** Added an interactive blacklist feature to exclude specific background processes from being controlled or displayed on the dials.
- **[Português]** **Blacklist de Aplicativos:** Adição de filtro interativo no painel de configurações para banir aplicativos específicos de segundo plano do visor e rotação física.
- **[English]** **Auto-Shutdown Logic:** Added a listener to gracefully shut down the Node.js process (`process.exit(0)`) whenever the Mirabox Stream Dock application or WebSocket connection is closed.
- **[Português]** **Encerramento Automático:** Fechamento automático e limpo do processo do Node.js ao detectar a desconexão ou fechamento do software Mirabox Stream Dock (eliminando processos órfãos).

### Optimized / Otimizado
- **[English]** **Subprocess Debouncing (CPU Saver):** Segmented background window polling (500ms) from passive volume checks (2000ms), reducing C# subprocess spawns by **75%** while keeping knobs responsive.
- **[Português]** **Sincronização Inteligente (Economia de CPU):** Divisão do loop de janelas (500ms) e checagem de volume (2000ms), reduzindo as inicializações de subprocessos C# em **75%** e diminuindo a CPU residual a quase zero.
- **[English]** **Non-blocking Disk I/O:** Switched profile saving from blocking synchronous `writeFileSync` to non-blocking asynchronous `fs.writeFile` to avoid UI micro-stutters during rotation.
- **[Português]** **I/O Não-Bloqueante (Disco):** Substituição do método síncrono `writeFileSync` pelo método assíncrono `fs.writeFile` para eliminar qualquer latência durante a rotação física dos knobs.
- **[English]** **Logging Clean-up:** Disabled disk logging (`debug.log` and user Desktop logs) entirely to optimize system I/O resources and disk space.
- **[Português]** **Remoção de Gravação de Logs:** Desativação completa da escrita de logs locais e na Área de Trabalho para economizar escritas em disco e otimizar velocidade.

### Fixed / Corrigido
- **[English]** **N4 Pro Dial Click:** Resolved the missing click feedback on Stream Dock N4 Pro devices (which fail to send consistent `dialUp` events) by triggering on `dialDown` events.
- **[Português]** **Clique do Knob no N4 Pro:** Resolução do bug de clique no visor físico nos modelos N4 Pro (que não enviam o evento `dialUp` de forma consistente) alterando o gatilho para o evento `dialDown`.
- **[English]** **CLI Argument Parsing:** Fixed crashes caused by JSON parameters (like `-info`) containing spaces being split by the Windows batch runner.
- **[Português]** **Parse de Parâmetros CLI:** Correção de falhas de inicialização do WebSocket devido a espaços ou aspas ausentes nos argumentos CLI analisados na inicialização do servidor.

---

## [v1.0.0] - 2026-05-12

### Added / Adicionado
- **[English]** Initial release of the modified and enhanced Time and Volume plugin.
- **[Português]** Lançamento inicial da versão modificada e aprimorada do plugin de Tempo e Volume.
