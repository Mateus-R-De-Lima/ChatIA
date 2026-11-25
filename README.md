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

## Configuração (User Secrets)

```bash
# Api
dotnet user-secrets set "ConnectionStrings:MongoDb" "mongodb://root:root@localhost:27017/?authSource=admin" --project src/ChatIA.Api
dotnet user-secrets set "Settings:Ai:ApiKey" "SUA_CHAVE" --project src/ChatIA.Api

# McpServer (também usado se rodar isolado)
dotnet user-secrets set "ConnectionStrings:MongoDb" "mongodb://root:root@localhost:27017/?authSource=admin" --project src/ChatIA.McpServer
```

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
- `GET /api/pedidos/revenue?year=2025&month=9`

## Tools MCP

| Tool | Uso |
|------|-----|
| `get_pedidos_count` | Quantidade de pedidos |
| `get_latest_pedidos` | Últimos N pedidos |
| `get_monthly_revenue` | Faturamento do mês/ano |

Somente leitura. Sem query arbitrária no MongoDB.

## Testes

```bash
dotnet test ChatIA.sln
```

## Segurança (aprendizado)

- Não versionar API keys nem senhas do Mongo (User Secrets / env vars).
- Tools MCP específicas e limitadas.
- Sem operações de escrita no Mongo via MCP.
