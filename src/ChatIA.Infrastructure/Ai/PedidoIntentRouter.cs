using System.Globalization;
using System.Text.RegularExpressions;
using ChatIA.Domain.Services.Ai;

namespace ChatIA.Infrastructure.Ai
{
    /// <summary>
    /// Mapeia perguntas típicas de delivery/PDV → tools MCP.
    /// Usa histórico para follow-ups ("quais são eles?", "que marca?", "desse pedido").
    /// </summary>
    internal static partial class PedidoIntentRouter
    {
        public static bool TryMatch(
            string userMessage,
            IReadOnlyList<ChatTurn>? conversationHistory,
            out string toolName,
            out IReadOnlyDictionary<string, object?> arguments)
        {
            toolName = string.Empty;
            arguments = new Dictionary<string, object?>();

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return false;
            }

            var text = Normalize(userMessage);
            var contextRef = FindReferenceInMessage(text)
                ?? FindReferenceInHistory(conversationHistory);

            // --- Detalhe por referência explícita ou contexto ---
            if (contextRef is int shortReference && IsDetailQuestion(text))
            {
                toolName = "get_pedido_by_reference";
                arguments = Args(("shortReference", shortReference));
                return true;
            }

            // "pedido 12" / "pedido #5" mesmo sem verbo de detalhe
            var explicitRef = FindReferenceInMessage(text);
            if (explicitRef is int explicitShort
                && text.Contains("pedido", StringComparison.Ordinal))
            {
                toolName = "get_pedido_by_reference";
                arguments = Args(("shortReference", explicitShort));
                return true;
            }

            // Follow-up curto apontando ao pedido do contexto
            // "quais são eles?", "e os itens?", "que marca?", "e o cliente?"
            if (contextRef is int followUpRef && IsFollowUpDetail(text))
            {
                toolName = "get_pedido_by_reference";
                arguments = Args(("shortReference", followUpRef));
                return true;
            }

            // Contagem
            if (ContainsAny(text,
                    "quantos pedidos", "quantidade de pedidos", "total de pedidos",
                    "qtd de pedidos", "qtde de pedidos", "numero de pedidos",
                    "número de pedidos", "quantos pedidos tem", "quantos pedidos existem"))
            {
                toolName = "get_pedidos_count";
                return true;
            }

            // Faturamento / relatório financeiro
            if (ContainsAny(text,
                    "faturamento", "receita", "revenue", "quanto vendi", "quanto vendemos",
                    "vendas do mes", "vendas do mês", "total vendido", "ticket do mes"))
            {
                var (year, month) = ExtractYearMonth(text);
                toolName = "get_monthly_revenue";
                arguments = Args(("year", year), ("month", month));
                return true;
            }

            // Último pedido COM detalhes (itens, pagamento, cliente...)
            if (IsLatestDetailQuestion(text))
            {
                toolName = "get_latest_pedido_detail";
                return true;
            }

            // Lista resumida de recentes
            if (ContainsAny(text,
                    "ultimos pedidos", "últimos pedidos", "pedidos recentes",
                    "mostre os pedidos", "mostra os pedidos", "liste os pedidos",
                    "listar pedidos", "quais sao os pedidos", "quais são os pedidos",
                    "historico de pedidos", "histórico de pedidos"))
            {
                var limit = 10;
                var limitMatch = LimitRegex().Match(text);
                if (limitMatch.Success
                    && int.TryParse(limitMatch.Groups[1].Value, out var parsedLimit))
                {
                    limit = Math.Clamp(parsedLimit, 1, 50);
                }

                toolName = "get_latest_pedidos";
                arguments = Args(("limit", limit));
                return true;
            }

            // Fallback operacional sobre pedidos
            if (text.Contains("pedido", StringComparison.Ordinal)
                && ContainsAny(text,
                    "qual", "quais", "mostra", "mostre", "lista", "liste",
                    "detalh", "informa", "relat", "item", "cliente", "pag"))
            {
                if (contextRef is int fallbackRef)
                {
                    toolName = "get_pedido_by_reference";
                    arguments = Args(("shortReference", fallbackRef));
                    return true;
                }

                toolName = "get_latest_pedido_detail";
                return true;
            }

            return false;
        }

        private static bool IsDetailQuestion(string text) =>
            ContainsAny(text,
                "detalh", "informa", "completo",
                "item", "itens", "produto", "produtos", "cardapio", "cardápio",
                "marca", "sabor", "observ", "obs",
                "cliente", "quem pediu", "nome do cliente", "telefone", "fone", "whats", "cpf", "cnpj", "documento",
                "pagament", "pagou", "como pagou", "forma de pagamento", "pix", "cartao", "cartão", "dinheiro",
                "total", "valor", "preco", "preço", "quanto custou", "quanto deu",
                "tipo", "retirada", "entrega", "delivery", "take", "canal", "anota", "ifood",
                "data", "quando", "horario", "horário",
                "desse pedido", "deste pedido", "do pedido", "nesse pedido", "neste pedido");

        private static bool IsFollowUpDetail(string text) =>
            ContainsAny(text,
                "quais sao eles", "quais são eles", "e eles", "e os itens", "os itens",
                "que marca", "qual marca", "e a marca", "marcas",
                "e o cliente", "quem foi", "telefone", "e o pagamento", "como pagou",
                "e o total", "qual o valor", "observacao", "observação",
                "mais detalhes", "completa", "completo", "me fala mais", "e ai", "e aí")
            || (text.Length <= 40 && ContainsAny(text, "itens", "item", "marca", "cliente", "pagamento", "total", "eles"));

        private static bool IsLatestDetailQuestion(string text)
        {
            var mentionsLatest = ContainsAny(text,
                "ultimo pedido", "último pedido",
                "pedido mais recente", "pedido recente",
                "ultimo que chegou", "último que chegou");

            if (!mentionsLatest)
            {
                return false;
            }

            // "últimos pedidos" (plural lista) não entra aqui
            if (ContainsAny(text, "ultimos pedidos", "últimos pedidos")
                && !ContainsAny(text, "ultimo pedido", "último pedido"))
            {
                return false;
            }

            return true;
        }

        private static int? FindReferenceInMessage(string text) =>
            TryParseReference(PedidoReferenceRegex().Match(text));

        private static int? FindReferenceInHistory(IReadOnlyList<ChatTurn>? history)
        {
            if (history is null || history.Count == 0)
            {
                return null;
            }

            for (var i = history.Count - 1; i >= 0; i--)
            {
                var content = history[i].Content;
                if (string.IsNullOrWhiteSpace(content) || PseudoToolCallParser.LooksLikeToolDump(content))
                {
                    continue;
                }

                var value = TryParseReference(HistoryReferenceRegex().Match(content));
                if (value is not null)
                {
                    return value;
                }
            }

            return null;
        }

        private static int? TryParseReference(Match match)
        {
            if (!match.Success)
            {
                return null;
            }

            for (var i = 1; i < match.Groups.Count; i++)
            {
                var group = match.Groups[i];
                if (!group.Success || string.IsNullOrWhiteSpace(group.Value))
                {
                    continue;
                }

                if (int.TryParse(group.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                    && value > 0)
                {
                    return value;
                }
            }

            return null;
        }

        private static (int Year, int Month) ExtractYearMonth(string text)
        {
            var now = DateTime.UtcNow;
            var year = now.Year;
            var month = now.Month;

            var yearMatch = YearRegex().Match(text);
            if (yearMatch.Success)
            {
                year = int.Parse(yearMatch.Value, CultureInfo.InvariantCulture);
            }

            var slash = MonthSlashRegex().Match(text);
            if (slash.Success
                && int.TryParse(slash.Groups[1].Value, out var slashMonth)
                && slashMonth is >= 1 and <= 12)
            {
                return (year, slashMonth);
            }

            var months = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["janeiro"] = 1,
                ["fevereiro"] = 2,
                ["março"] = 3,
                ["marco"] = 3,
                ["abril"] = 4,
                ["maio"] = 5,
                ["junho"] = 6,
                ["julho"] = 7,
                ["agosto"] = 8,
                ["setembro"] = 9,
                ["outubro"] = 10,
                ["novembro"] = 11,
                ["dezembro"] = 12
            };

            foreach (var pair in months)
            {
                if (text.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))
                {
                    month = pair.Value;
                    break;
                }
            }

            return (year, month);
        }

        private static string Normalize(string value) =>
            value.Trim().ToLowerInvariant();

        private static bool ContainsAny(string text, params string[] needles) =>
            needles.Any(needle => text.Contains(needle, StringComparison.OrdinalIgnoreCase));

        private static IReadOnlyDictionary<string, object?> Args(params (string Key, object? Value)[] pairs)
        {
            var map = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var (key, value) in pairs)
            {
                map[key] = value;
            }

            return map;
        }

        [GeneratedRegex(@"pedido\s*#?\s*(\d+)|#\s*(\d+)\b|ref(?:erencia|erência)?\s*#?\s*(\d+)", RegexOptions.IgnoreCase)]
        private static partial Regex PedidoReferenceRegex();

        // Aceita "Pedido #48", "Ref #48" vindos das respostas do MCP
        [GeneratedRegex(@"Pedido\s*#\s*(\d+)|Ref\s*#\s*(\d+)|#\s*(\d+)", RegexOptions.IgnoreCase)]
        private static partial Regex HistoryReferenceRegex();

        [GeneratedRegex(@"\b(20\d{2})\b")]
        private static partial Regex YearRegex();

        [GeneratedRegex(@"\b(\d{1,2})\s*/\s*20\d{2}\b")]
        private static partial Regex MonthSlashRegex();

        [GeneratedRegex(@"\b(\d{1,2})\s+pedidos?\b", RegexOptions.IgnoreCase)]
        private static partial Regex LimitRegex();
    }
}
