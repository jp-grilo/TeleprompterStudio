# Teleprompter Studio

Aplicativo desktop nativo para Windows projetado para musicos, cantores e bandas, fornecendo execucao de cifras e letras no palco com rolagem suave acelerada por hardware e modo de exibicao estruturada em quatro linhas.

---

## Download e Execucao Rapida (Recomendado para Usuarios)

Nao e necessario instalar ferramentas de programacao, Git, Node.js ou ter familiaridade com o GitHub para utilizar o Teleprompter Studio. O aplicativo e distribuido em formato executavel portatil (.exe), pronto para uso imediato.

### Link Direto para Download
- **[Download Direto: Teleprompter Studio para Windows (.exe)](https://github.com/jp-grilo/TeleprompterStudio/releases/latest/download/Teleprompt.1.0.0.exe)**
- Pagina com todas as versoes e historico: [Releases do Teleprompter Studio](https://github.com/jp-grilo/TeleprompterStudio/releases/latest)

### Como Usar em 3 Passos
1. **Baixar**: Clique no link de download direto acima para salvar o arquivo executavel portatil no seu computador.
2. **Executar**: Acesse sua pasta de Downloads e de um duplo clique no arquivo baixado (`Teleprompt.1.0.0.exe`). Nao e necessario instalar nada.
3. **Aviso do Windows Defender (SmartScreen)**:
   - Como se trata de um software livre independente e sem certificado digital pago, o Windows pode exibir uma janela com o aviso *"O Windows protegeu o seu computador"*.
   - Para abrir o programa normalmente: clique em **Mais informacoes** e em seguida selecione **Executar assim mesmo**. O aplicativo abrira de imediato.

### Como Rodar Diretamente pela Pasta do Repositorio
Caso voce tenha baixado ou clonado este repositorio:
- Basta dar um duplo clique no atalho **`Teleprompter.lnk`** ou no arquivo inicializador **`Iniciar-Teleprompter.bat`** presente diretamente na raiz do projeto.
- O inicializador detecta automaticamente o executavel compilado e abre a aplicacao instantaneamente.

---

## Visao Geral

O Teleprompter Studio foi desenvolvido com foco em desempenho, confiabilidade e operacao offline total. Projetado para proporcionar rolagem continua fluida a 60 quadros por segundo e visualizacao em tela cheia no palco, o sistema conta com uma interface desktop dedicada desenvolvida em Electron e React, alem de bibliotecas de dominio em .NET para manipulacao musical e persistencia local SQLite.

## Principais Funcionalidades

- Exibicao de Palco em Tela Cheia: Modos de rolagem continua fluida e modo de slides por blocos de 4 linhas intercaladas (Letra, Acorde, Letra, Acorde).
- Transposicao Musical em Tempo Real: Motor matematico para calculo de 12 semitons com suporte a fundamentais, tensoes complexas, baixos invertidos e alternancia entre notacao Anglo-saxa (A-G) e Latina (Do-Si).
- Customizacao Pontual de Acordes: Capacidade de sobrepor acordes individuais em linhas especificas sem perder a integridade da estrutura harmonica da musica.
- Ingestao Universal e Web Scraping: Pipeline de extracao com suporte dedicado para sites populares de cifras e fallback heuristico inteligente para capturar conteudo musical de paginas genericas.
- Suporte a Letras Sem Cifra e ChordPro: Importacao de letras puras com capacidade de adicao posterior de acordes e deteccao automatica do padrao ChordPro.
- Organizacao Hierarquica: Sidebar com categorizacao automatica (Artistas, Albuns, Musicas), gerenciamento de pastas e subpastas de repertorios personalizados, e lista de favoritos.
- Compartilhamento Offline Descomplicado: Exportacao e importacao de musicas completas em arquivos JSON puros, garantindo portabilidade entre musicos da banda.
- Configuracoes Globais: Modos de tema Dark Stage e Light, selecao de fontes monoespacadas, ajuste de cores para cifras e letras, e mapeamento de atalhos para teclado e pedais Bluetooth.

## Arquitetura do Software

O projeto adota principios de Clean Architecture e separacao modular de responsabilidades:

1. teleprompter-ui: Aplicacao desktop construida com Electron, React, TypeScript, Tailwind/CSS e Prisma com banco SQLite embarcado, fornecendo interface grafica de alta performance, visualizador de palco em janela dedicada e scraping automatizado.
2. Teleprompter.Core: Modelos e contratos de dominio musical puros (.NET 8).
3. Teleprompter.Services: Implementacoes de motores de negócio em C# (parser sintatico de acordes, algoritmo de transposicao cromatica e scrapers).
4. Teleprompter.Data: Camada de persistencia Entity Framework Core com SQLite.
5. Teleprompter.Tests: Bateria de testes unitarios cobrindo a logica harmonica, parsing e persistencia.

## Requisitos de Sistema

### Para Usuarios Finais (Apenas Executar o Aplicativo)
- Sistema Operacional: Windows 10 (versao 19041+) ou Windows 11 (64-bit).
- Dependencias: Nenhuma. O executavel portatil contem todos os componentes necessarios embutidos.

### Para Desenvolvedores (Compilar a Partir do Codigo-Fonte)
- Node.js 18.x ou superior e npm.
- .NET 8.0 SDK (LTS) ou superior.

## Instrucoes de Desenvolvimento e Compilacao

### Clonar o repositorio
```bash
git clone https://github.com/jp-grilo/TeleprompterStudio.git
cd TeleprompterStudio
```

### Executar a Interface Desktop (Modo Desenvolvedor)
```bash
cd teleprompter-ui
npm install
npm run dev
```

### Compilar o Executavel Portatil (.exe)
```bash
cd teleprompter-ui
npm run dist
```
O executavel portatil sera gerado no diretorio `teleprompter-ui/dist-bin/`.

### Executar os Testes Unitarios .NET
```bash
dotnet test
```

## Licenca

Distribuido sob a licenca MIT. Consulte o arquivo LICENSE para mais detalhes.
