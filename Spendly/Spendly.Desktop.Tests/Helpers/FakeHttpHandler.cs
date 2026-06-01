using System.Net;
using System.Net.Http;
using System.Text;

namespace Spendly.Desktop.Tests.Helpers;

internal sealed class FakeHttpHandler : HttpMessageHandler
{
    private readonly Queue<(HttpStatusCode Status, string Json)> _queue = new();

    public void Enqueue(HttpStatusCode status, string json = "null")
        => _queue.Enqueue((status, json));

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var (status, json) = _queue.Count > 0 ? _queue.Dequeue() : (HttpStatusCode.OK, "null");
        return Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }
}
