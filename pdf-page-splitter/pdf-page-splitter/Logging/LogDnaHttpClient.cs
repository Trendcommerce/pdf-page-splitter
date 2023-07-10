using Microsoft.Extensions.Configuration;
using Serilog.Sinks.Http;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace pdf_page_splitter.Logging
{
    public sealed class LogDnaHttpClient : IHttpClient
    {
        private readonly HttpClient _client;

        public LogDnaHttpClient(string apiKey)
        {
            this._client = new HttpClient();

            var authToken = Encoding.ASCII.GetBytes($"{apiKey}:");
            this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            var response = await this._client.PostAsync(requestUri, content).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.MultiStatus)
            {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return response;
        }

        public void Dispose() => this._client?.Dispose();

        public void Configure(IConfiguration configuration)
        {
        }
    }
}
