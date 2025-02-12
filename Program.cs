
using System.Security.Cryptography;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        var canonicalString = CreateCanonicalString<object>(
            "POST",
            "/signature/validate",
            [],
            [],
            new
            {
                accountUID = "xxxxx",
                subAccountUID = "xxxxx",
                customerPhoneNumber = "+12176225710"
            });

        Console.WriteLine($"Canonical String:\n{canonicalString}");
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1739390054136);
        var signature = GenerateCanonicalSignature("xxxxx", canonicalString, timestamp);

        Console.WriteLine($"Signature:\n{signature}");
    }

    static string OrderJsonKeysAscending<T>(T? item)
    {
        if (item == null) return string.Empty;
        return OrderedJsonSerializer.Serialize(item);
    }

    static string CreateCanonicalQueryString(Dictionary<string, string> queryParameters)
    {
        return string.Join("&", queryParameters.OrderBy(t => t.Key)
            .Select(t => $"{t.Key}={t.Value}"));
    }

    static string CreateCanonicalHeadersString(Dictionary<string, string> headers)
    {
        return string.Join("\n", headers.OrderBy(t => t.Key)
            .Select(t => $"{t.Key}:{t.Value}"));
    }

    static string HashPayload<T>(T? item)
    {
        var orderedPayload = OrderJsonKeysAscending(item);
        Console.WriteLine($"Ordered Payload:\n{orderedPayload}");
        var payloadBytes = Encoding.UTF8.GetBytes(orderedPayload);
        var hashedPayload = SHA256.HashData(payloadBytes);

        return Encoding.UTF8.GetString(hashedPayload);
    }

    static string CreateCanonicalString<T>(string method, string path, Dictionary<string, string> queryParameters, Dictionary<string, string> headers, T? item)
    {
        var canonicalQueryString = CreateCanonicalQueryString(queryParameters);
        var canonicalHeadersString = CreateCanonicalHeadersString(headers);
        var payload = HashPayload(item);

        return $"{method}\n{path}" +
        (string.IsNullOrWhiteSpace(canonicalQueryString) ? string.Empty : $"\n{canonicalQueryString}") +
        (string.IsNullOrWhiteSpace(canonicalHeadersString) ? string.Empty : $"\n{canonicalHeadersString}") +
        $"\n{payload}";
    }

    static string GenerateCanonicalSignature(string key, string data, DateTimeOffset timestamp)
    {
        var unix = timestamp.ToUnixTimeMilliseconds();
        var finalString = $"{data}\n{unix}";
        var finalBytes = Encoding.UTF8.GetBytes(finalString);
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var hash = HMACSHA256.HashData(keyBytes, finalBytes);

        return Convert.ToHexString(hash);
    }
}