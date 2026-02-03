using System.Net.Http.Json;
using System.Text.Json;
using BankMore.Transfers.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace BankMore.Transfers.Application.Services;

public class ContaCorrenteService : IContaCorrenteService
{
    private readonly ContaCorrenteOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ContaCorrenteService> _logger;

    private const string TipoMovimentoDebito = "D";
    private const string TipoMovimentoCredito = "C";

    public ContaCorrenteService(
        HttpClient httpClient,
        IOptions<ContaCorrenteOptions> options,
        ILogger<ContaCorrenteService> logger)
    {
        _options = options.Value;
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.ApiUri);

        _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3872.0 Safari/537.36 Edg/78.0.244.0");
        _httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        _httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
    }

    public async ValueTask<bool> RealizeDebit(string apiToken, decimal valor, string? accountNumber = null)
    {
        SetAuthorizationHeader(apiToken);

        var request = new TransactionRequest(accountNumber, valor, TipoMovimentoDebito);

        var response = await _httpClient.PostAsJsonAsync("transaction", request);
        return response.IsSuccessStatusCode;
    }

    public async ValueTask<bool> RealizeCredit(string apiToken, decimal valor, string? accountNumber = null)
    {
        SetAuthorizationHeader(apiToken);

        var request = new TransactionRequest(accountNumber, valor, TipoMovimentoCredito);

        var response = await _httpClient.PostAsJsonAsync("transaction", request);
        return response.IsSuccessStatusCode;
    }

    private void SetAuthorizationHeader(string apiToken)
    {
        _httpClient.DefaultRequestHeaders.Remove(HeaderNames.Authorization);

        var tokenString = apiToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? apiToken
            : $"Bearer {apiToken}";

        _httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, tokenString);
    }
    private sealed record TransactionRequest(string? Numero, decimal Valor, string TipoMovimento);
}