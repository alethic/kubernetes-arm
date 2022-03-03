using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ArmKubeOper.Extensions
{

    /// <summary>
    /// Provides extension methods for working with <see cref="JsonNode"/>.
    /// </summary>
    public static class JsonNodeExtensions
    {

        /// <summary>
        /// Gets the <see cref="JsonValueKind"/> for a given node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static JsonValueKind GetValueKind(this JsonNode node, JsonSerializerOptions options = null)
        {
            JsonValueKind valueKind;

            if (node is null)
            {
                valueKind = JsonValueKind.Null;
            }
            else if (node is JsonObject)
            {
                valueKind = JsonValueKind.Object;
            }
            else if (node is JsonArray)
            {
                valueKind = JsonValueKind.Array;
            }
            else
            {
                JsonValue jValue = (JsonValue)node;

                if (jValue.TryGetValue(out JsonElement element))
                {
                    // Typically this will occur in read mode after a Parse(), so just use the JsonElement.
                    valueKind = element.ValueKind;
                }
                else
                {
                    object obj = jValue.GetValue<object>();


                    if (obj is string)
                    {
                        valueKind = JsonValueKind.String;
                    }
                    else if (IsKnownNumberType(obj.GetType()))
                    {
                        valueKind = JsonValueKind.Number;
                    }
                    else
                    {
                        // Slow, but accurate.
                        string json = jValue.ToJsonString();
                        valueKind = JsonSerializer.Deserialize<JsonElement>(json, options).ValueKind;
                    }
                }
            }

            return valueKind;

            static bool IsKnownNumberType(Type type) =>
                type == typeof(sbyte) ||
                type == typeof(byte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal);
        }

    }

}
