using JasonLibrary.Core.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class UpdateMetadataParserTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void Parse_OfficialWebsiteManifest_ReturnsReleaseAndAsset()
        {
            const string json = @"{
                                    ""schema_version"": 1,
                                    ""product"": ""JasonQuery"",
                                    ""releases"": [
                                      {
                                        ""tag_name"": ""v0.92.0"",
                                        ""prerelease"": false,
                                        ""assets"": [
                                          {
                                            ""name"": ""JasonQuery64.zip"",
                                            ""size"": 100,
                                            ""digest"": ""sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa""
                                          }
                                        ]
                                      }
                                    ]
                                  }";

            var manifest = new UpdateMetadataParser().Parse(json);

            Assert.AreEqual(1, manifest.SchemaVersion);
            Assert.AreEqual("JasonQuery", manifest.Product);
            Assert.AreEqual(1, manifest.Releases.Count);
            Assert.AreEqual("v0.92.0", manifest.Releases[0].TagName);
            Assert.AreEqual("JasonQuery64.zip", manifest.Releases[0].Assets[0].Name);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void Parse_GitHubReleaseArray_NormalizesToManifest()
        {
            const string json = @"[
                                    {
                                      ""tag_name"": ""v0.92.1"",
                                      ""draft"": false,
                                      ""prerelease"": true,
                                      ""assets"": []
                                    }
                                  ]";

            var manifest = new UpdateMetadataParser().Parse(json);

            Assert.AreEqual(1, manifest.SchemaVersion);
            Assert.AreEqual("JasonQuery", manifest.Product);
            Assert.AreEqual(1, manifest.Releases.Count);
            Assert.IsTrue(manifest.Releases[0].Prerelease);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void Parse_UnsupportedSchema_ThrowsNotSupportedException()
        {
            const string json = @"{ ""schema_version"": 2, ""product"": ""JasonQuery"", ""releases"": [] }";

            Assert.ThrowsException<NotSupportedException>(() => new UpdateMetadataParser().Parse(json));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void Parse_DifferentProduct_ThrowsFormatException()
        {
            const string json = @"{ ""schema_version"": 1, ""product"": ""AnotherProduct"", ""releases"": [] }";

            Assert.ThrowsException<FormatException>(() => new UpdateMetadataParser().Parse(json));
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        [DataRow("")]
        [DataRow("not-json")]
        [DataRow("{")]
        public void Parse_InvalidContent_ThrowsFormatException(string json)
        {
            Assert.ThrowsException<FormatException>(() => new UpdateMetadataParser().Parse(json));
        }
    }
}