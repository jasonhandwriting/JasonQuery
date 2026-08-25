using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Updater.Core
{
    [DataContract]
    internal sealed class UpdaterWorkerPlan
    {
        public const int CurrentSchemaVersion = 1;

        [DataMember(Name = "schema_version")]
        public int SchemaVersion { get; set; } = CurrentSchemaVersion;

        [DataMember(Name = "parent_process_id")]
        public int ParentProcessId { get; set; }

        [DataMember(Name = "installation_root")]
        public string InstallationRoot { get; set; }

        [DataMember(Name = "workspace_root")]
        public string WorkspaceRoot { get; set; }

        [DataMember(Name = "payload_root")]
        public string PayloadRoot { get; set; }

        [DataMember(Name = "backup_root")]
        public string BackupRoot { get; set; }

        [DataMember(Name = "launch_jasonquery")]
        public bool LaunchJasonQuery { get; set; }

        [DataMember(Name = "request")]
        public UpdaterRequest Request { get; set; }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
            {
                throw new NotSupportedException($"Updater worker plan schema version {SchemaVersion} is not supported.");
            }

            if (ParentProcessId <= 0)
            {
                throw new FormatException("The parent process identifier is invalid.");
            }

            Request?.Validate();

            if (Request == null)
            {
                throw new FormatException("The Updater worker plan does not contain a request.");
            }

            InstallationRoot = RequireAbsoluteDirectory(InstallationRoot, nameof(InstallationRoot), true);
            WorkspaceRoot = RequireAbsoluteDirectory(WorkspaceRoot, nameof(WorkspaceRoot), true);
            PayloadRoot = RequireAbsoluteDirectory(PayloadRoot, nameof(PayloadRoot), true);
            BackupRoot = RequireAbsoluteDirectory(BackupRoot, nameof(BackupRoot), false);

            if (!IsUnderRoot(PayloadRoot, WorkspaceRoot))
            {
                throw new FormatException("The extracted payload is outside the Updater workspace.");
            }

            if (!File.Exists(Path.Combine(InstallationRoot, "JasonQuery.exe")))
            {
                throw new FileNotFoundException("JasonQuery.exe was not found in the installation directory.");
            }
        }

        public static void Write(string planPath, UpdaterWorkerPlan plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            plan.Validate();

            var serializer = new DataContractJsonSerializer(typeof(UpdaterWorkerPlan));

            using (var stream = new FileStream(planPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                serializer.WriteObject(stream, plan);
                stream.Flush(true);
            }
        }

        public static UpdaterWorkerPlan Load(string planPath)
        {
            if (string.IsNullOrWhiteSpace(planPath) || !File.Exists(planPath))
            {
                throw new FileNotFoundException("The Updater worker plan was not found.", planPath);
            }

            var serializer = new DataContractJsonSerializer(typeof(UpdaterWorkerPlan));

            using (var stream = new FileStream(planPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var plan = (UpdaterWorkerPlan)serializer.ReadObject(stream);

                if (plan == null)
                {
                    throw new FormatException("The Updater worker plan is invalid.");
                }

                plan.Validate();

                return plan;
            }
        }

        private static string RequireAbsoluteDirectory(string path, string parameterName, bool mustExist)
        {
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path))
            {
                throw new FormatException($"The {parameterName} directory is invalid.");
            }

            var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (mustExist && !Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException($"The required directory was not found: {fullPath}");
            }

            return fullPath;
        }

        private static bool IsUnderRoot(string path, string root)
        {
            var normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var normalizedPath = path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
        }
    }
}
