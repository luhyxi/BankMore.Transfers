using System.Security.Cryptography;
using System.Text;
using SharedKernel;

namespace BankMore.Transfers.Domain.IdempotenciaAggregate;

public sealed class IdempotenciaHashedRequest : ValueObject
{
    private const int StoredHashMaxLength = 1000;

    public string Value { get; }

    private IdempotenciaHashedRequest(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > StoredHashMaxLength)
        {
            throw new ArgumentException(
                $"Idempotencia hashed request must be provided and must have less than {StoredHashMaxLength} characters.",
                nameof(value));
        }

        Value = value;
    }

    public static IdempotenciaHashedRequest FromPlainText(string request)
    {
        if (string.IsNullOrWhiteSpace(request))
        {
            throw new ArgumentException("Request must be provided.", nameof(request));
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(request));
        return new IdempotenciaHashedRequest(Convert.ToBase64String(bytes));
    }

    public static string? FromHashed(string hash) => new(hash);
    
    public static implicit operator IdempotenciaHashedRequest?(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : new IdempotenciaHashedRequest(value);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}
