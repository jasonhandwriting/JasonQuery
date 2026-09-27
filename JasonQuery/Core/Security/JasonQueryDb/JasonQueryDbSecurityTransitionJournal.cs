using Newtonsoft.Json;
using System;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public sealed class JasonQueryDbSecurityTransitionJournal
    {
        public const int CurrentVersion = 1;

        [JsonProperty("transitionVersion")]
        public int TransitionVersion { get; set; }

        [JsonProperty("sourceMetadata")]
        public JasonQueryDbSecurityMetadata SourceMetadata { get; set; }

        [JsonProperty("targetMetadata")]
        public JasonQueryDbSecurityMetadata TargetMetadata { get; set; }

        [JsonProperty("protectedSourceDatabasePassword")]
        public string ProtectedSourceDatabasePassword { get; set; }

        [JsonProperty("protectedTargetDatabasePassword")]
        public string ProtectedTargetDatabasePassword { get; set; }

        public void Validate()
        {
            if (TransitionVersion != CurrentVersion)
            {
                throw new NotSupportedException($"Database security transition journal version {TransitionVersion} is not supported.");
            }

            if (SourceMetadata == null || TargetMetadata == null)
            {
                throw new InvalidOperationException("Database security transition metadata is incomplete.");
            }

            SourceMetadata.Validate();
            TargetMetadata.Validate();

            if (string.IsNullOrWhiteSpace(ProtectedSourceDatabasePassword) || string.IsNullOrWhiteSpace(ProtectedTargetDatabasePassword))
            {
                throw new InvalidOperationException("Database security transition recovery keys are incomplete.");
            }

            ValidateBase64(ProtectedSourceDatabasePassword);
            ValidateBase64(ProtectedTargetDatabasePassword);
        }

        private static void ValidateBase64(string value)
        {
            try
            {
                Convert.FromBase64String(value);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Database security transition recovery data is invalid.", ex);
            }
        }
    }
}
