
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class OrderedJsonSerializer
{
    public static string Serialize<T>(T obj)
    {
        if (obj == null) return string.Empty;
        var token = JToken.FromObject(obj);
        var orderedToken = OrderJToken(token);
        return JsonConvert.SerializeObject(orderedToken, Formatting.None);
    }

    private static JToken OrderJToken(JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Object:
                var obj = (JObject)token;
                var orderedObj = new JObject(
                    obj.Properties()
                       .OrderBy(p => p.Name)
                       .Select(p => new JProperty(p.Name, OrderJToken(p.Value)))
                );
                return orderedObj;

            case JTokenType.Array:
                var array = (JArray)token;
                if (array.All(IsPrimitive))
                {
                    var sortedArray = new JArray(array.OrderBy(a => a.ToString(), StringComparer.Ordinal));
                    return sortedArray;
                }
                else
                {
                    var orderedArray = new JArray(array.Select(OrderJToken));
                    return new JArray(orderedArray.OrderBy(a => a.ToString(Formatting.None), StringComparer.Ordinal));
                }

            default:
                return token;
        }
    }

    private static bool IsPrimitive(JToken token)
    {
        return token.Type == JTokenType.Integer ||
               token.Type == JTokenType.Float ||
               token.Type == JTokenType.String ||
               token.Type == JTokenType.Boolean;
    }
}
