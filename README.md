# Microsoft Defender Prank v1.0 🛡️

### ⚠️ Aviso Legal
Este projeto foi desenvolvido para fins estritamente educacionais e de brincadeira entre amigos. O objetivo é demonstrar conceitos de Windows API, Hooks de teclado e manipulação de interface WPF. Não utilize para fins maliciosos.

---

### 📝 Descrição
Um aplicativo de simulação que replica a interface do **Microsoft Defender Preview**. O programa exibe um alerta de ameaça fictício e utiliza técnicas de persistência e controle de foco para criar um cenário onde o usuário não consegue fechar a janela pelos métodos convencionais.

### 🚀 Funcionalidades Principais
* **Interface Camuflada**: UI baseada em WPF que se adapta automaticamente ao tema (Claro/Escuro) do Windows.
* **Low-Level Keyboard Hook**: Bloqueia atalhos críticos do sistema, como `Alt+Tab`, `Tecla Windows`, `Alt+Esc` e `Ctrl+Esc`.
* **Anti-TaskMgr**: Um monitor ativo que encerra o processo do Gerenciador de Tarefas (`taskmgr.exe`) em milissegundos caso seja aberto.
* **Mouse Escape**: Algoritmo que utiliza a API nativa `SetCursorPos` para mover o cursor do mouse para longe dos botões de fechar/minimizar.
* **Persistência no Startup**: Inserção automática de chave no Registro do Windows (`Run`) para execução no início da sessão.
* **Foco Persistente**: Utiliza `Topmost="True"` e monitoramento de foco para garantir que a janela nunca fique atrás de outros aplicativos.
* **Invisibilidade na Barra de Tarefas**: Configurado para não exibir ícone na barra de tarefas, dificultando o encerramento via clique direito.

### 🔑 Tecla de Emergência (Kill Switch)
Caso precise encerrar o aplicativo e limpar as entradas do registro automaticamente:
* Pressione simultaneamente: **`CTRL + SHIFT + ALT + K`**

---

### 🛠️ Tecnologias Utilizadas
* **Linguagem**: C#
* **Framework**: WPF (.NET)
* **APIs Nativas**: `user32.dll`, `kernel32.dll` (Win32 API)
* **Persistência**: Registro do Windows (RegistryKey)

### 📂 Estrutura de Pastas
O executável foi projetado para ser escondido em subpastas no diretório `C:\`, exigindo que o caminho seja adicionado às exclusões do **Windows Defender** para o funcionamento pleno de todas as funcionalidades de Hook e Monitoramento.