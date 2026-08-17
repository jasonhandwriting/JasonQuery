using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JasonLibrary.Core.Update
{
    [DataContract]
    public sealed class UpdateMetadataManifest
    {
        public UpdateMetadataManifest()
        {
            Releases = new List<UpdateReleaseMetadata>();
        }

        [DataMember(Name = "schema_version")]
        public int SchemaVersion { get; set; }

        [DataMember(Name = "product")]
        public string Product { get; set; }

        [DataMember(Name = "generated_at")]
        public string GeneratedAt { get; set; }

        [DataMember(Name = "releases")]
        public List<UpdateReleaseMetadata> Releases { get; set; }
    }

    [DataContract]
    public sealed class UpdateReleaseMetadata
    {
        public UpdateReleaseMetadata()
        {
            Assets = new List<UpdateAssetMetadata>();
        }

        [DataMember(Name = "tag_name")]
        public string TagName { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "body")]
        public string Body { get; set; }

        [DataMember(Name = "draft")]
        public bool Draft { get; set; }

        [DataMember(Name = "prerelease")]
        public bool Prerelease { get; set; }

        [DataMember(Name = "published_at")]
        public string PublishedAt { get; set; }

        [DataMember(Name = "html_url")]
        public string HtmlUrl { get; set; }

        [DataMember(Name = "assets")]
        public List<UpdateAssetMetadata> Assets { get; set; }
    }

    [DataContract]
    public sealed class UpdateAssetMetadata
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "state")]
        public string State { get; set; }

        [DataMember(Name = "content_type")]
        public string ContentType { get; set; }

        [DataMember(Name = "size")]
        public long Size { get; set; }

        [DataMember(Name = "digest")]
        public string Digest { get; set; }

        [DataMember(Name = "browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }
}