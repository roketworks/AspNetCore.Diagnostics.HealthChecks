using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Uris
{
    public class UriHealthCheck
        : IHealthCheck
    {        
        private readonly UriHealthCheckOptions _options;
        private readonly HttpClient _httpClient;

        public UriHealthCheck(UriHealthCheckOptions options, IHttpClientFactory httpClientFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClient = httpClientFactory.CreateClient();
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var defaultHttpMethod = _options.HttpMethod;
            var defaultCodes = _options.ExpectedHttpCodes;
            var idx = 0;

            try
            {
                foreach (var item in _options.UrisOptions)
                {
                    var method = item.HttpMethod ?? defaultHttpMethod;
                    var expectedCodes = item.ExpectedHttpCodes ?? defaultCodes;

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"{nameof(UriHealthCheck)} execution is cancelled.");
                    }

                    var requestMessage = new HttpRequestMessage(method, item.Uri);

                    foreach (var header in item.Headers)
                    {
                        requestMessage.Headers.Add(header.Name, header.Value);
                    }

                    var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

                    if (!((int)response.StatusCode >= expectedCodes.Min && (int)response.StatusCode <= expectedCodes.Max))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"Discover endpoint #{idx} is not responding with code in {expectedCodes.Min}...{expectedCodes.Max} range, the current status is {response.StatusCode}.");
                    }

                    ++idx;
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
