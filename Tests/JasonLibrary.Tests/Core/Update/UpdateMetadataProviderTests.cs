using JasonLibrary.Core.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class UpdateMetadataProviderTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public async Task LoadAsync_GitHub_AddsRequiredHeadersAndParsesReleaseArray()
        {
            const string json = @"[{ ""tag_name"": ""v0.92.1"", ""assets"": [] }]";
            var handler = new RecordingHttpMessageHandler(json);

            using (var httpClient = new HttpClient(handler))
            using (var provider = new UpdateMetadataProvider(httpClient, "JasonQuery.Tests/1.0"))
            {
                var manifest = await provider.LoadAsync
                (
                    UpdateMetadataSourceKind.GitHub,
                    string.Empty,
                    CancellationToken.None
                );

                Assert.AreEqual(1, manifest.Releases.Count);
                Assert.AreEqual("JasonQuery.Tests/1.0", handler.UserAgent);
                Assert.AreEqual("application/vnd.github+json", handler.Accept);
                Assert.AreEqual(UpdateMetadataSettingsContract.GitHubApiVersion, handler.ApiVersion);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public async Task LoadAsync_LocalFolder_ReadsMetadataWithoutHttpRequest()
        {
            var folder = Path.Combine(Path.GetTempPath(), "JasonQueryUpdateTests", Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(folder);

            try
            {
                File.WriteAllText
                (
                    Path.Combine(folder, UpdateMetadataSettingsContract.MetadataFileName),
                    @"{ ""schema_version"": 1, ""product"": ""JasonQuery"", ""releases"": [] }"
                );

                var handler = new RecordingHttpMessageHandler("[]");

                using (var httpClient = new HttpClient(handler))
                using (var provider = new UpdateMetadataProvider(httpClient, "JasonQuery.Tests/1.0"))
                {
                    var manifest = await provider.LoadAsync
                    (
                        UpdateMetadataSourceKind.LocalFolder,
                        folder,
                        CancellationToken.None
                    );

                    Assert.AreEqual("JasonQuery", manifest.Product);
                    Assert.AreEqual(0, handler.RequestCount);
                }
            }
            finally
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public async Task LoadAsync_HttpNotFound_ThrowsClassifiedFailureWithLocation()
        {
            var handler = new RecordingHttpMessageHandler(string.Empty, HttpStatusCode.NotFound);

            using (var httpClient = new HttpClient(handler))
            using (var provider = new UpdateMetadataProvider(httpClient, "JasonQuery.Tests/1.0"))
            {
                var exception = await Assert.ThrowsExactlyAsync<UpdateMetadataLoadException>
                (
                    () => provider.LoadAsync
                    (
                        UpdateMetadataSourceKind.OfficialWebsite,
                        string.Empty,
                        CancellationToken.None
                    )
                );

                Assert.AreEqual(UpdateMetadataFailureKind.NotFound, exception.FailureKind);
                Assert.AreEqual(HttpStatusCode.NotFound, exception.StatusCode.Value);
                Assert.AreEqual(UpdateMetadataSettingsContract.OfficialWebsiteMetadataUrl, exception.Location);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public async Task LoadAsync_LocalFileMissing_ThrowsClassifiedFailureWithPath()
        {
            var folder = Path.Combine(Path.GetTempPath(), "JasonQueryUpdateTests", Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(folder);

            try
            {
                using (var httpClient = new HttpClient(new RecordingHttpMessageHandler("[]")))
                using (var provider = new UpdateMetadataProvider(httpClient, "JasonQuery.Tests/1.0"))
                {
                    var exception = await Assert.ThrowsExactlyAsync<UpdateMetadataLoadException>
                    (
                        () => provider.LoadAsync
                        (
                            UpdateMetadataSourceKind.LocalFolder,
                            folder,
                            CancellationToken.None
                        )
                    );

                    Assert.AreEqual(UpdateMetadataFailureKind.NotFound, exception.FailureKind);
                    Assert.AreEqual(Path.Combine(folder, UpdateMetadataSettingsContract.MetadataFileName), exception.Location);
                    Assert.IsNull(exception.StatusCode);
                }
            }
            finally
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public async Task LoadAsync_RequestTimeout_ThrowsClassifiedFailure()
        {
            var handler = new ThrowingHttpMessageHandler(new TaskCanceledException("Timed out."));

            using (var httpClient = new HttpClient(handler))
            using (var provider = new UpdateMetadataProvider(httpClient, "JasonQuery.Tests/1.0"))
            {
                var exception = await Assert.ThrowsExactlyAsync<UpdateMetadataLoadException>
                (
                    () => provider.LoadAsync
                    (
                        UpdateMetadataSourceKind.OfficialWebsite,
                        string.Empty,
                        CancellationToken.None
                    )
                );

                Assert.AreEqual(UpdateMetadataFailureKind.Timeout, exception.FailureKind);
                Assert.AreEqual(UpdateMetadataSettingsContract.OfficialWebsiteMetadataUrl, exception.Location);
            }
        }

        private sealed class RecordingHttpMessageHandler : HttpMessageHandler
        {
            private readonly string _json;
            private readonly HttpStatusCode _statusCode;

            public RecordingHttpMessageHandler(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
            {
                _json = json;
                _statusCode = statusCode;
            }

            public int RequestCount { get; private set; }

            public string UserAgent { get; private set; }

            public string Accept { get; private set; }

            public string ApiVersion { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestCount++;
                UserAgent = string.Join(" ", request.Headers.UserAgent);
                Accept = string.Join(",", request.Headers.Accept);

                if (request.Headers.TryGetValues("X-GitHub-Api-Version", out var values))
                {
                    ApiVersion = string.Join(",", values);
                }

                return Task.FromResult
                (
                    new HttpResponseMessage(_statusCode)
                    {
                        Content = new StringContent(_json)
                    }
                );
            }
        }

        private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
        {
            private readonly Exception _exception;

            public ThrowingHttpMessageHandler(Exception exception)
            {
                _exception = exception;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromException<HttpResponseMessage>(_exception);
            }
        }
    }
}
