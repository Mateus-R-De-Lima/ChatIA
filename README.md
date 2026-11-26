# ChatIA

Projeto de aprendizado: chatbot com **C# / .NET**, **MongoDB** e **MCP**, seguindo a Clean Architecture do [CashFlow](https://github.com/Mateus-R-De-Lima/CashFlow).

## Arquitetura

```
src/
  ChatIA.Domain          → entidades, contratos (repositórios, IChatAiService)
  ChatIA.Application     → use cases
  ChatIA.Communication   → Request/Response JSON
  ChatIA.Exception       → exceções tipadas
  ChatIA.Infrastructure  → MongoDB, OpenAI (MEAI), cliente MCP
  ChatIA.Api             → host HTTP
  ChatIA.McpServer       → host MCP (tools de leitura)
  ChatIA.Web             → Blazor (UI do chat)
testes/
  UseCase.Tests, Validator.Tests, WebApi.Test, CommonTestUtilities
```

## Fluxo do chat

1. Blazor envia a pergunta para `POST /api/chat`.
2. `SendChatMessageUseCase` valida e chama `IChatAiService`.
3. A Infrastructure envia mensagens ao LLM (`IChatClient` + `UseFunctionInvocation`).
4. Se a pergunta precisar de dados, o modelo escolhe uma tool MCP.
5. O cliente MCP sobe/comunica com `ChatIA.McpServer` (stdio).
6. A tool executa o use case → MongoDB `homologacao.Pedidos`.
7. O resultado volta ao modelo, que responde em linguagem natural.

**Cenário A (direto):** “O que é Clean Architecture?” → só LLM.  
**Cenário B (dados):** “Quantos pedidos existem?” → LLM + tool MCP.

## Pré-requisitos

- .NET 9+
- MongoDB em `localhost:27017` (database `homologacao`, collection `Pedidos`)
- Chave de API OpenAI (ou endpoint compatível)

## Configuração (`appsettings`)

Edite `src/ChatIA.Api/appsettings.json` (ou `appsettings.Development.json`):

```json
"ConnectionStrings": {
  "MongoDb": "mongodb://localhost:27017"
},
"Settings": {
  "Ai": {
    "ApiKey": "ollama",
    "Model": "llama3.2:1b",
    "Endpoint": "http://localhost:11434/v1"
  }
}
```

| Chave | Uso |
|-------|-----|
| `Settings:Ai:ApiKey` | OpenAI, ou qualquer string se usar Ollama local |
| `Settings:Ai:Endpoint` | Vazio = OpenAI oficial; ex. Ollama: `http://localhost:11434/v1` |
| `ConnectionStrings:MongoDb` | MongoDB |

Evite commitar chaves reais no Git.

## Como executar

```bash
dotnet build ChatIA.sln

# Terminal 1 — Api (ela sobe o McpServer automaticamente via stdio)
dotnet run --project src/ChatIA.Api

# Terminal 2 — Blazor
dotnet run --project src/ChatIA.Web
```

Abra o chat em `/chat`.

Endpoints úteis de pedidos (sem IA):

- `GET /api/pedidos/count`
- `GET /api/pedidos/latest?limit=10`
- `GET /api/pedidos/{shortReference}`
- `GET /api/pedidos/revenue?year=2025&month=9`

## Tools MCP

| Tool | Uso |
|------|-----|
| `get_pedidos_count` | Quantidade de pedidos |
| `get_latest_pedidos` | Lista resumida dos últimos N |
| `get_latest_pedido_detail` | Detalhe completo do último (itens, cliente, pagamento…) |
| `get_pedido_by_reference` | Detalhe completo pela ref. `#` |
| `get_monthly_revenue` | Faturamento do mês/ano |

Perguntas de pedidos são **roteadas no código** para o MCP (histórico resolve follow-ups como “quais são eles?” / “que marca?”).

### O que o banco devolve (e o que não tem)

Campos usados: `shortReference`, cliente (nome/telefone/doc), `items` (nome, qtd, preço, total, observação), `payments`, `total`, `type`, `salesChannel`, `createdAt`.

**Não existe** campo separado de marca — marca/sabor costuma estar no **nome do item** ou na **observação**.

### Exemplos de perguntas (delivery / PDV)

| Intenção | Exemplos | Tool |
|----------|----------|------|
| Contagem | Quantos pedidos? Total de pedidos? | `get_pedidos_count` |
| Lista | Últimos 5 pedidos; pedidos recentes | `get_latest_pedidos` |
| Último completo | Qual o último pedido? Itens do último? | `get_latest_pedido_detail` |
| Detalhe / itens | Detalhes do pedido 12; itens do #5 | `get_pedido_by_reference` |
| Follow-up | Quais são eles? Que marca? E o cliente? | `get_pedido_by_reference` (via histórico) |
| Cliente | Quem pediu? Telefone do pedido 8? | `get_pedido_by_reference` |
| Pagamento | Como pagou? Foi PIX? | `get_pedido_by_reference` |
| Tipo/canal | É delivery ou retirada? Canal Anota/iFood? | `get_pedido_by_reference` |
| Faturamento | Faturamento de setembro/2025 | `get_monthly_revenue` |

Somente leitura. Sem query arbitrária no MongoDB.

## Testes

```bash
dotnet test ChatIA.sln
```

## Segurança (aprendizado)

- Não versionar API keys nem senhas do Mongo (User Secrets / env vars).
- Tools MCP específicas e limitadas.
- Sem operações de escrita no Mongo via MCP.
