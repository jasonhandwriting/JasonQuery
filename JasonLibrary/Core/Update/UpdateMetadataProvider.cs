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
                string localJson;

                try
                {
                    localJson = File.ReadAllText(location.Value);
                }
                catch (FileNotFoundException ex)
                {
                    throw CreateLoadException(UpdateMetadataFailureKind.NotFound, location.Value, ex);
                }
                catch (DirectoryNotFoundException ex)
                {
                    throw CreateLoadException(UpdateMetadataFailureKind.NotFound, location.Value, ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw CreateLoadException(UpdateMetadataFailureKind.AccessDenied, location.Value, ex);
                }
                catch (IOException ex)
                {
                    throw CreateLoadException(UpdateMetadataFailureKind.ReadError, location.Value, ex);
                }

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

                HttpResponseMessage response;

                try
                {
                    response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    throw CreateLoadException(UpdateMetadataFailureKind.Timeout, location.Value, ex);
                }
                catch (HttpRequestException ex)
                {
                    throw CreateLoadException(UpdateMetadataFailureKind.Network, location.Value, ex);
                }

                using (response)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw CreateHttpLoadException(location.Value, response.StatusCode);
                    }

                    string json;

                    try
                    {
                        json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                    {
                        throw CreateLoadException(UpdateMetadataFailureKind.Timeout, location.Value, ex);
                    }
                    catch (HttpRequestException ex)
                    {
                        throw CreateLoadException(UpdateMetadataFailureKind.Network, location.Value, ex);
                    }
                    catch (IOException ex)
                    {
                        throw CreateLoadException(UpdateMetadataFailureKind.ReadError, location.Value, ex);
                    }

                    return _parser.Parse(json);
                }
            }
        }

        private static UpdateMetadataLoadException CreateHttpLoadException(string location, HttpStatusCode statusCode)
        {
            var failureKind = statusCode == HttpStatusCode.NotFound
                              ? UpdateMetadataFailureKind.NotFound
                              : statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden
                                  ? UpdateMetadataFailureKind.AccessDenied
                                  : statusCode == HttpStatusCode.RequestTimeout
                                      ? UpdateMetadataFailureKind.Timeout
                                      : (int)statusCode >= 500
                                          ? UpdateMetadataFailureKind.ServerError
                                          : UpdateMetadataFailureKind.HttpError;

            return new UpdateMetadataLoadException
            (
                failureKind,
                location,
                $"The update information source returned HTTP {(int)statusCode} ({statusCode}).",
                statusCode
            );
        }

        private static UpdateMetadataLoadException CreateLoadException(UpdateMetadataFailureKind failureKind, string location, Exception innerException)
        {
            return new UpdateMetadataLoadException
            (
                failureKind,
                location,
                innerException.Message,
                null,
                innerException
            );
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
