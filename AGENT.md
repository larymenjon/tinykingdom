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

- `Core`: regras centrais do jogo, bootstrap, estado global e coordenação de alto nível.
- `Events`: eventos de domínio, sinais e comunicação desacoplada entre sistemas.
- `UI`: apresentação, telas, HUD e componentes de interface.
- `Research`: lógica de pesquisa, progresso, desbloqueios e evolução.
- `Resources`: coleta, produção, consumo e balanço de recursos.
- `Buildings`: construção, upgrades, produção ligada a edifícios e regras específicas de construção.
- `ScriptableObjects`: dados configuráveis, custos, balanceamento e definições reutilizáveis.

### Regras de separação

- UI não deve conter regra de jogo pesada.
- Sistemas de gameplay não devem depender diretamente de detalhes visuais.
- Prefira comunicação por eventos em vez de referências diretas quando isso reduzir acoplamento.
- Evite que uma classe central vire um "Deus objeto".

## Dependências e Comunicação

- Prefira injeção explícita de dependências quando possível.
- Prefira interfaces, eventos e observadores para reduzir acoplamento.
- Evite abusar de `Singleton`.
- Evite `FindObjectOfType` e dependências implícitas escondidas.
- Quando uma dependência for inevitável, deixe isso claro no código e na cena.

## Dados e Configuração

- Não hardcode valores de balanceamento quando eles puderem viver em dados.
- Prefira `ScriptableObject` para custos, estatísticas, configurações e balanceamento.
- Separe dados de comportamento.
- Não misture valores fixos de gameplay dentro de código de interface ou de fluxo.

## Comunicação por Eventos

- Use eventos para avisar mudanças relevantes:
  - recursos alterados
  - construção concluída
  - pesquisa finalizada
  - estado do jogador alterado
- A UI deve reagir a eventos, não ficar consultando o jogo o tempo todo.
- Sistemas devem conversar com o mínimo de acoplamento possível.

## Padrões de Implementação

- Use padrões apenas quando houver ganho real de manutenção.
- Não force arquitetura excessiva em sistemas pequenos.
- Quando um sistema crescer, extraia responsabilidade antes que a complexidade se espalhe.

## Antes de Criar ou Alterar Código

Sempre avalie:

1. A mudança pertence a `UI`, `Core`, `Events`, `Research`, `Resources` ou `Buildings`?
2. A responsabilidade já existe em outro lugar?
3. Isso aumenta acoplamento desnecessário?
4. Existe uma forma mais simples de resolver?
5. O código continua fácil de ler daqui a meses?

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
