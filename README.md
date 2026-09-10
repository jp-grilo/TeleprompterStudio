# Teleprompter Studio

Aplicativo desktop nativo para Windows projetado para musicos, cantores e bandas, fornecendo execucao de cifras e letras no palco com rolagem suave acelerada por hardware e modo de exibicao estruturada em quatro linhas.

## Visao Geral

O Teleprompter Studio foi desenvolvido com foco em desempenho, confiabilidade e operacao offline total. Construido com o ecossistema .NET 8 LTS e Windows Presentation Foundation (WPF), o sistema aproveita a aceleracao grafica do DirectX para proporcionar rolagem continua fluida a 60 quadros por segundo, mesmo em dispositivos de baixo consumo.

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

O projeto adota o padrao arquitetural MVVM (Model-View-ViewModel) e principios de Clean Architecture, segregando responsabilidades em cinco camadas principais:

1. Teleprompter.Core: Entidades de dominio, modelos de dados e definicao das interfaces de servico. Nao possui dependencias de interface grafica.
2. Teleprompter.Services: Implementacoes dos motores de negocio:
   - UniversalChordParserService: Deteccao de secoes, analise sintatica de acordes via expressoes regulares e alinhamento de texto.
   - ChromaticTranspositionService: Transposicao cromatica de notas e conversao de sistemas de notacao.
   - ScraperPipeline: Ingestao estruturada com CifraClubScraper e UniversalHeuristicScraper.
   - JsonSongPackageService: Serializacao e desserializacao de musicas e repertorios em JSON.
   - JsonSettingsService: Gerenciamento persistente de preferencias do usuario.
   - WpfTeleprompterEngine: Controlador de taxa de rolagem, temporizacao e paginacao.
3. Teleprompter.Data: Camada de acesso a dados utilizando Entity Framework Core com SQLite local.
4. Teleprompter.Wpf: Interface com o usuario construida em XAML, utilizando CommunityToolkit.Mvvm e estilizacao vetorial.
5. Teleprompter.Tests: Bateria de testes unitarios automatizados cobrindo a logica musical, parsing, persistencia e transposicao.

## Requisitos de Sistema

- Sistema Operacional: Windows 10 (versao 19041+) ou Windows 11.
- Runtime / SDK: .NET 8.0 SDK (LTS) ou superior.
- Arquitetura: x64 ou ARM64.

## Instrucoes de Compilacao e Testes

### Clonar o repositorio
```bash
git clone https://github.com/jp-grilo/TeleprompterStudio.git
cd TeleprompterStudio
```

### Restaurar dependencias e compilar
```bash
dotnet restore
dotnet build -c Release
```

### Executar a suite de testes unitarios
```bash
dotnet test
```

### Executar a aplicacao
```bash
dotnet run --project src/Teleprompter.Wpf/Teleprompter.Wpf.csproj
```

## Licenca

Distribuido sob a licenca MIT. Consulte o arquivo LICENSE para mais detalhes.
