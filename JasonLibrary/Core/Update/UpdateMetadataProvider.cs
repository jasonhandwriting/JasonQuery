using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace JasonLibrary.Core.Update
{
    public sealed class UpdateMetadataProvider : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly bool _ownsHttpClient;
        private readonly string _userAgent;
        private readonly UpdateMetadataParser _parser = new UpdateMetadataParser();

        public UpdateMetadataProvider(string userAgent) : this(CreateHttpClient(), userAgent, true)
        {
        }

        public UpdateMetadataProvider(HttpClient httpClient, string userAgent) : this(httpClient, userAgent, false)
        {
        }

        private UpdateMetadataProvider(HttpClient httpClient, string userAgent, bool ownsHttpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _userAgent = string.IsNullOrWhiteSpace(userAgent) ? "JasonQuery" : userAgent.Trim();
            _ownsHttpClient = ownsHttpClient;
        }

        public async Task<UpdateMetadataManifest> LoadAsync(UpdateMetadataSourceKind source, string localFolder, CancellationToken cancellationToken)
        {
            var location = UpdateSourceResolver.ResolveMetadata(source, localFolder);

            if (location.IsLocalFile)
            {
                var localJson = File.ReadAllText(location.Value);

                return _parser.Parse(localJson);
            }

            using (var request = new HttpRequestMessage(HttpMethod.Get, location.Value))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);

                if (source == UpdateMetadataSourceKind.GitHub)
                {
                    request.Headers.Accept.Clear();
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                    request.Headers.TryAddWithoutValidation
                    (
                        "X-GitHub-Api-Version",
                        UpdateMetadataSettingsContract.GitHubApiVersion
                    );
                }

                using (var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return _parser.Parse(json);
                }
            }
        }

        public void Dispose()
        {
            if (_ownsHttpClient)
            {
                _httpClient.Dispose();
            }
        }

        private static HttpClient CreateHttpClient()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            return new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
        }
    }
}