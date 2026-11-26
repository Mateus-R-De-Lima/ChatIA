using System.Text.Json;
using System.Text.RegularExpressions;

namespace ChatIA.Infrastructure.Ai
{
    /// <summary>
    /// Modelos pequenos às vezes escrevem tool calls como JSON no texto (à vezes vários,
    /// misturados com prosa). Extrai a primeira tool conhecida.
    /// </summary>
    internal static partial class PseudoToolCallParser
    {
        private static readonly HashSet<string> KnownTools = new(StringComparer.OrdinalIgnoreCase)
        {
            "get_pedidos_count",
            "get_latest_pedidos",
            "get_latest_pedido_detail",
            "get_pedido_by_reference",
            "get_monthly_revenue"
        };

        private static readonly HashSet<string> SchemaNoise = new(StringComparer.OrdinalIgnoreCase)
        {
            "type",
            "description",
            "properties",
            "required",
            "default",
            "name",
            "function",
            "parameters",
            "arguments",
            "tool_calls",
            "id",
            "index"
        };

        public static bool TryParse(
            string? assistantText,
            out string toolName,
            out IReadOnlyDictionary<string, object?> arguments)
        {
            toolName = string.Empty;
            arguments = new Dictionary<string, object?>();

            if (string.IsNullOrWhiteSpace(assistantText))
            {
                return false;
            }

            foreach (var json in ExtractJsonObjects(assistantText))
            {
                if (TryParseSingle(json, out toolName, out arguments))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// True se o texto parece lixo de tool call (para filtrar do histórico).
        /// </summary>
        public static bool LooksLikeToolDump(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            return text.Contains("get_pedidos_count", StringComparison.OrdinalIgnoreCase)
                || text.Contains("get_latest_pedidos", StringComparison.OrdinalIgnoreCase)
                || text.Contains("get_latest_pedido_detail", StringComparison.OrdinalIgnoreCase)
                || text.Contains("get_pedido_by_reference", StringComparison.OrdinalIgnoreCase)
                || text.Contains("get_monthly_revenue", StringComparison.OrdinalIgnoreCase)
                || text.Contains("\"type\":\"function\"", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParseSingle(
            string json,
            out string toolName,
            out IReadOnlyDictionary<string, object?> arguments)
        {
            toolName = string.Empty;
            arguments = new Dictionary<string, object?>();

            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (!TryResolveName(root, out toolName) || !KnownTools.Contains(toolName))
                {
                    return false;
                }

                arguments = ExtractArguments(root);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static IEnumerable<string> ExtractJsonObjects(string text)
        {
            var stripped = MarkdownFenceRegex().Replace(text, "$1");
            var results = new List<string>();
            var depth = 0;
            var start = -1;

            for (var i = 0; i < stripped.Length; i++)
            {
                var ch = stripped[i];
                if (ch == '{')
                {
                    if (depth == 0)
                    {
                        start = i;
                    }

                    depth++;
                }
                else if (ch == '}')
                {
                    if (depth <= 0)
                    {
                        continue;
                    }

                    depth--;
                    if (depth == 0 && start >= 0)
                    {
                        results.Add(stripped[start..(i + 1)]);
                        start = -1;
                    }
                }
            }

            return results;
        }

        private static bool TryResolveName(JsonElement root, out string toolName)
        {
            toolName = string.Empty;

            if (root.TryGetProperty("tool_calls", out var toolCalls)
                && toolCalls.ValueKind == JsonValueKind.Array
                && toolCalls.GetArrayLength() > 0)
            {
                return TryResolveName(toolCalls[0], out toolName);
            }

            if (root.TryGetProperty("function", out var function)
                && function.ValueKind == JsonValueKind.Object
                && function.TryGetProperty("name", out var nestedName)
                && nestedName.ValueKind == JsonValueKind.String)
            {
                toolName = nestedName.GetString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(toolName);
            }

            if (root.TryGetProperty("name", out var name)
                && name.ValueKind == JsonValueKind.String)
            {
                toolName = name.GetString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(toolName);
            }

            return false;
        }

        private static IReadOnlyDictionary<string, object?> ExtractArguments(JsonElement root)
        {
            var args = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            if (root.TryGetProperty("tool_calls", out var toolCalls)
                && toolCalls.ValueKind == JsonValueKind.Array
                && toolCalls.GetArrayLength() > 0)
            {
                return ExtractArguments(toolCalls[0]);
            }

            CollectFromObject(root, args);

            if (root.TryGetProperty("function", out var function)
                && function.ValueKind == JsonValueKind.Object)
            {
                CollectFromObject(function, args);

                if (function.TryGetProperty("parameters", out var functionParams)
                    && functionParams.ValueKind == JsonValueKind.Object)
                {
                    CollectFromObject(functionParams, args);
                }

                if (function.TryGetProperty("arguments", out var functionArguments))
                {
                    MergeArgumentsProperty(functionArguments, args);
                }
            }

            if (root.TryGetProperty("parameters", out var parameters)
                && parameters.ValueKind == JsonValueKind.Object)
            {
                CollectFromObject(parameters, args);
            }

            if (root.TryGetProperty("arguments", out var arguments))
            {
                MergeArgumentsProperty(arguments, args);
            }

            foreach (var noise in SchemaNoise)
            {
                args.Remove(noise);
            }

            // Alias comum: reference → shortReference
            if (!args.ContainsKey("shortReference") && args.TryGetValue("reference", out var reference))
            {
                args["shortReference"] = reference;
                args.Remove("reference");
            }

            return args;
        }

        private static void MergeArgumentsProperty(
            JsonElement arguments,
            Dictionary<string, object?> args)
        {
            if (arguments.ValueKind == JsonValueKind.String)
            {
                var raw = arguments.GetString();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return;
                }

                try
                {
                    using var nested = JsonDocument.Parse(raw);
                    if (nested.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        CollectFromObject(nested.RootElement, args);
                    }
                }
                catch (JsonException)
                {
                    // ignore
                }

                return;
            }

            if (arguments.ValueKind == JsonValueKind.Object)
            {
                CollectFromObject(arguments, args);
            }
        }

        private static void CollectFromObject(JsonElement obj, Dictionary<string, object?> args)
        {
            foreach (var property in obj.EnumerateObject())
            {
                if (SchemaNoise.Contains(property.Name))
                {
                    continue;
                }

                if (property.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                {
                    continue;
                }

                args[property.Name] = ConvertJsonValue(property.Value);
            }
        }

        private static object? ConvertJsonValue(JsonElement value) =>
            value.ValueKind switch
            {
                JsonValueKind.String => CoerceString(value.GetString()),
                JsonValueKind.Number when value.TryGetInt32(out var i) => i,
                JsonValueKind.Number when value.TryGetInt64(out var l) => l,
                JsonValueKind.Number => value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => value.GetRawText()
            };

        private static object? CoerceString(string? value)
        {
            if (value is null)
            {
                return null;
            }

            if (int.TryParse(value, out var i))
            {
                return i;
            }

            return value;
        }

        [GeneratedRegex(@"```(?:json)?\s*(.*?)\s*```", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
        private static partial Regex MarkdownFenceRegex();
    }
}
