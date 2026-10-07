using System.Net;
using System.Net.Http;

namespace CurrencyConverter.Tests.Fixtures;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, HttpResponseMessage> _responses = new(StringComparer.OrdinalIgnoreCase);
    private Exception? _exceptionToThrow;
    private readonly List<Uri> _requestedUris = new();

    public IReadOnlyList<Uri> RequestedUris => _requestedUris;

    public void SetException(Exception exception)
    {
        _exceptionToThrow = exception;
    }

    public void RegisterResponse(string urlSubstr, HttpStatusCode statusCode, string content)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content)
        };
        _responses[urlSubstr] = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri != null)
        {
            _requestedUris.Add(request.RequestUri);
        }

        if (_exceptionToThrow != null)
        {
            throw _exceptionToThrow;
        }

        if (request.RequestUri != null)
        {
            foreach (var kvp in _responses)
            {
                if (request.RequestUri.ToString().Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    // Clone response content for multiple reads
                    var resp = new HttpResponseMessage(kvp.Value.StatusCode)
                    {
                        Content = new StringContent(kvp.Value.Content.ReadAsStringAsync().Result)
                    };
                    return Task.FromResult(resp);
                }
            }
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
