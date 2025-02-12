
using System.Security.Cryptography;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        var timestamp = DateTimeOffset.UtcNow;
        //var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1739398219897);
        var canonicalString = CreateCanonicalString<object>(
            "POST",
            "/signature/validate",
            [],
            new Dictionary<string, string>()
            {
                //{"x-authorization-api-key","6Mi38PDHIi"},
                //{"x-authorization-timestamp",timestamp.ToUnixTimeMilliseconds().ToString()}
            },
            new
            {
                accountUID = "555",
                externalSubAccountUID = "123",
                //subAccountUID = "123",
                customerPhoneNumber = "+12176225710",
                //apiKey = "6Mi38PDHIi",
                //secretKey = "4qdJAItVs8"
            });

        Console.WriteLine($"Canonical String:\n{canonicalString}");
        var signature = GenerateCanonicalSignature("4qdJAItVs8", canonicalString, timestamp);

        Console.WriteLine($"Signature:\n{signature}");
    }

    static string OrderJsonKeysAscending<T>(T? item)
    {
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

        return Convert.ToHexStringLower(hashedPayload);
    }

    static string CreateCanonicalString<T>(string method, string path, Dictionary<string, string> queryParameters, Dictionary<string, string> headers, T? item)
    {
        var canonicalQueryString = CreateCanonicalQueryString(queryParameters);
        var canonicalHeadersString = CreateCanonicalHeadersString(headers);
        var payload = HashPayload(item);

        return $"{method}\n{path}\n{canonicalQueryString}\n{canonicalHeadersString}\n{payload}";
    }

    static string GenerateCanonicalSignature(string key, string data, DateTimeOffset timestamp)
    {
        var unix = timestamp.ToUnixTimeMilliseconds();
        var finalString = $"{unix}\n{data}";
        var finalBytes = Encoding.UTF8.GetBytes(finalString);
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var hash = HMACSHA256.HashData(keyBytes, finalBytes);

        return Convert.ToHexStringLower(hash);
    }
}