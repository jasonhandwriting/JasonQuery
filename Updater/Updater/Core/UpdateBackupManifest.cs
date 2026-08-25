using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Updater.Core
{
    [DataContract]
    public sealed class UpdateBackupManifest
    {
        public const int CurrentSchemaVersion = 1;

        public UpdateBackupManifest()
        {
            Files = new List<UpdateBackupFile>();
        }

        [DataMember(Name = "schema_version")]
        public int SchemaVersion { get; set; } = CurrentSchemaVersion;

        [DataMember(Name = "created_at")]
        public string CreatedAt { get; set; }

        [DataMember(Name = "installation_root")]
        public string InstallationRoot { get; set; }

        [DataMember(Name = "installed_version")]
        public string InstalledVersion { get; set; }

        [DataMember(Name = "target_version")]
        public string TargetVersion { get; set; }

        [DataMember(Name = "files")]
        public List<UpdateBackupFile> Files { get; set; }
    }

    [DataContract]
    public sealed class UpdateBackupFile
    {
        [DataMember(Name = "relative_path")]
        public string RelativePath { get; set; }

        [DataMember(Name = "existed")]
        public bool Existed { get; set; }

        [DataMember(Name = "size")]
        public long Size { get; set; }

        [DataMember(Name = "sha256")]
        public string Sha256 { get; set; }
    }
}
