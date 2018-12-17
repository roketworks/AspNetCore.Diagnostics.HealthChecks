using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.IdSvr
{
    public class IdSvrHealthCheck
        : IHealthCheck
    {
        const string IDSVR_DISCOVER_CONFIGURATION_SEGMENT = ".well-known/openid-configuration";
        private readonly Uri _idSvrUri;
        private readonly HttpClient _httpClient;

        public IdSvrHealthCheck(Uri idSvrUri, IHttpClientFactory httpClientFactory)
        {
            _idSvrUri = idSvrUri ?? throw new ArgumentNullException(nameof(idSvrUri));
            _httpClient = httpClientFactory.CreateClient();
        }
        
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(IDSVR_DISCOVER_CONFIGURATION_SEGMENT, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"Discover endpoint is not responding with 200 OK, the current status is {response.StatusCode} and the content { (await response.Content.ReadAsStringAsync())}");
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
