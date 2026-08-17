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

        private sealed class RecordingHttpMessageHandler : HttpMessageHandler
        {
            private readonly string _json;

            public RecordingHttpMessageHandler(string json)
            {
                _json = json;
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
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(_json)
                    }
                );
            }
        }
    }
}