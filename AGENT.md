# AGENT.md

Diretrizes para agentes e contribuições no projeto `tinykingdom`.

## Objetivo

Manter o código do jogo legível, sustentável e fácil de evoluir. Toda mudança deve priorizar arquitetura clara, baixo acoplamento e responsabilidade bem definida.

## Princípios de Código

### 1. Nomes significativos

- Use nomes claros e descritivos.
- Evite abreviações, siglas obscuras e nomes genéricos como `Manager`, `Helper` e `Utils`, a menos que o papel esteja realmente explícito.
- Nomes de classes, métodos, variáveis, arquivos e pastas devem revelar intenção.

Exemplos:

- `currentCoins`
- `constructionProgress`
- `StartBuildingConstruction()`

### 2. Responsabilidade única

- Cada classe, método e componente deve ter uma única responsabilidade principal.
- Não concentre movimento, inventário, UI, áudio, salvamento e lógica de jogo na mesma classe.
- Se uma classe estiver fazendo demais, divida em componentes menores e especializados.

### 3. Funções pequenas e diretas

- Prefira funções curtas e fáceis de ler.
- Uma função deve fazer uma coisa e fazê-la bem.
- Evite blocos muito longos, condicionais profundas e fluxo difícil de seguir.

### 4. Parâmetros sob controle

- Prefira poucos parâmetros por método.
- Se a assinatura crescer demais, use um objeto de dados, uma configuração ou `ScriptableObject`.
- Não force chamadas com listas longas de argumentos.

### 5. Código autoexplicativo

- O código deve se explicar pelo nome e pela estrutura.
- Comentários não devem compensar nomes ruins ou lógica confusa.
- Use comentários apenas para regras de negócio, restrições técnicas ou decisões não óbvias.

### 6. DRY

- Não duplique lógica.
- Se algo aparece repetido, extraia método, classe, serviço ou componente reutilizável.

### 7. KISS

- Prefira a solução mais simples que realmente atende ao problema.
- Não introduza abstrações, padrões ou camadas sem benefício claro.
- Simplicidade vem antes de sofisticação.

### 8. Boy Scout Rule

- Ao mexer em um trecho, deixe-o melhor do que encontrou.
- Sempre que possível, aproveite para melhorar nomes, reduzir duplicação e simplificar o fluxo.

## Arquitetura do Projeto

### Padrão principal: MVC adaptado para Unity

O projeto deve seguir um MVC pragmático, adaptado ao fluxo do Unity:

| Camada | Responsabilidade | Exemplo em Tiny Kingdom |
| --- | --- | --- |
| `Model` (`ScriptableObjects`) | Dados puros de configuração | `BuildingData.asset` com custo, produção e sprite |
| `GameManager` (`Singleton`) | Estado global do jogo | Recursos atuais, estágio e tempo offline |
| `Controller` (`MonoBehaviour`) | Lógica de gameplay | `BuildingController.cs` para construir, coletar e fazer upgrade |
| `View` (`UI Toolkit` / `uGUI`) | Apresentação visual | `ResourceHUD.cs` para textos e ícones |
| `SaveSystem` | Persistência | JSON serializado via `PlayerPrefs` com backup em nuvem |

### Regras do MVC no projeto

- `Model` deve conter dados de configuração, não lógica de interface.
- `Controller` deve orquestrar regras de gameplay e conversar com o `GameManager`.
- `View` deve apenas exibir estado e reagir a eventos ou chamadas do controller.
- `GameManager` centraliza o estado global que precisa ser compartilhado.
- `SaveSystem` deve cuidar de persistência, sem misturar regra de jogo ou UI.
- `ScriptableObjects` são a fonte preferida para dados de balanceamento e configuração.

### Estrutura de pastas esperada

O projeto já está organizado em:

- `Assets/Scripts/Core`
- `Assets/Scripts/Events`
- `Assets/Scripts/Research`
- `Assets/Scripts/Resources`
- `Assets/Scripts/Buildings`
- `Assets/Scripts/UI`
- `Assets/ScriptableObjects`
- `Assets/Prefabs`
- `Assets/Scenes`

Use essa estrutura como guia de responsabilidade.

### Regras por área

- `Core`: regras centrais do jogo, bootstrap, estado global e coordenação de alto nível, incluindo `GameManager` e `SaveSystem` quando aplicável.
- `Events`: eventos de domínio, sinais e comunicação desacoplada entre sistemas.
- `UI`: apresentação, telas, HUD e componentes de interface, atuando como `View`.
- `Research`: lógica de pesquisa, progresso, desbloqueios e evolução.
- `Resources`: coleta, produção, consumo e balanço de recursos.
- `Buildings`: construção, upgrades, produção ligada a edifícios e regras específicas de construção, normalmente no papel de `Controller`.
- `ScriptableObjects`: dados configuráveis, custos, balanceamento e definições reutilizáveis, normalmente no papel de `Model`.

### Regras de separação

- UI não deve conter regra de jogo pesada.
- Sistemas de gameplay não devem depender diretamente de detalhes visuais.
- Prefira comunicação por eventos em vez de referências diretas quando isso reduzir acoplamento.
- Evite que uma classe central vire um "Deus objeto".

## Dependências e Comunicação

- Prefira injeção explícita de dependências quando possível.
- Prefira interfaces, eventos e observadores para reduzir acoplamento.
- `GameManager` pode ser `Singleton` quando atuar como ponto único de estado global, mas seu uso deve ser restrito a esse papel.
- Evite abusar de `Singleton` fora do núcleo global do jogo.
- Evite `FindObjectOfType` e dependências implícitas escondidas.
- Quando uma dependência for inevitável, deixe isso claro no código e na cena.

## Dados e Configuração

- Não hardcode valores de balanceamento quando eles puderem viver em dados.
- Prefira `ScriptableObject` para custos, estatísticas, configurações e balanceamento.
- Separe dados de comportamento.
- `BuildingData`, `ResearchData`, `ResourceData` e outros assets de configuração devem ser tratados como `Model`.
- Não misture valores fixos de gameplay dentro de código de interface ou de fluxo.

## Comunicação por Eventos

- Use eventos para avisar mudanças relevantes:
  - recursos alterados
  - construção concluída
  - pesquisa finalizada
  - estado do jogador alterado
- A UI deve reagir a eventos, não ficar consultando o jogo o tempo todo.
- Sistemas devem conversar com o mínimo de acoplamento possível.
- Views não devem calcular regra de negócio para exibir dados.

## Padrões de Implementação

- Use padrões apenas quando houver ganho real de manutenção.
- Não force arquitetura excessiva em sistemas pequenos.
- Quando um sistema crescer, extraia responsabilidade antes que a complexidade se espalhe.

## Antes de Criar ou Alterar Código

Sempre avalie:

1. A mudança pertence a `UI`, `Core`, `Events`, `Research`, `Resources` ou `Buildings`?
2. Isso é `Model`, `Controller`, `View`, `GameManager` ou `SaveSystem`?
3. A responsabilidade já existe em outro lugar?
4. Isso aumenta acoplamento desnecessário?
5. Existe uma forma mais simples de resolver?
6. O código continua fácil de ler daqui a meses?

## Regras de Qualidade

- Priorize legibilidade.
- Mantenha a manutenção simples.
- Preserve escalabilidade sem exagerar na abstração.
- Não sacrifique arquitetura por rapidez de curto prazo.
- Código bom é código que outra pessoa entende rápido.

## Ordem de Prioridade

Ao gerar ou alterar código, siga esta ordem:

1. Legibilidade
2. Manutenibilidade
3. Escalabilidade
4. Simplicidade
5. Performance

## Observação Final

Se uma mudança aparentemente pequena comprometer a clareza, a responsabilidade única ou o desacoplamento, pare e reorganize antes de seguir.
