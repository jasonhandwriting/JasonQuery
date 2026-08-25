using System;
using System.IO;

namespace Updater.Core
{
    public enum UpdatePackageVerificationFailureKind
    {
        Missing,
        SizeMismatch,
        DigestMismatch
    }

    public sealed class UpdatePackageVerificationException : IOException
    {
        public UpdatePackageVerificationException(UpdatePackageVerificationFailureKind failureKind, string message) : base(message)
        {
            FailureKind = failureKind;
        }

        public UpdatePackageVerificationFailureKind FailureKind { get; }
    }

    public sealed class UpdatePackageVerificationResult
    {
        public UpdatePackageVerificationResult(long size, string sha256)
        {
            Size = size;
            Sha256 = sha256;
        }

        public long Size { get; }

        public string Sha256 { get; }
    }

    public static class UpdatePackageVerifier
    {
        public static UpdatePackageVerificationResult Verify(string packagePath, long expectedSize, string expectedDigest)
        {
            if (string.IsNullOrWhiteSpace(packagePath) || !File.Exists(packagePath))
            {
                throw new UpdatePackageVerificationException
                (
                    UpdatePackageVerificationFailureKind.Missing,
                    $"The update package was not found: {packagePath}"
                );
            }

            if (expectedSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expectedSize));
            }

            if (!Sha256Digest.TryNormalize(expectedDigest, out var expectedSha256))
            {
                throw new ArgumentException("The expected SHA-256 digest is invalid.", nameof(expectedDigest));
            }

            var actualSize = new FileInfo(packagePath).Length;

            if (actualSize != expectedSize)
            {
                throw new UpdatePackageVerificationException
                (
                    UpdatePackageVerificationFailureKind.SizeMismatch,
                    $"The update package size does not match. Expected: {expectedSize}; Actual: {actualSize}."
                );
            }

            var actualSha256 = Sha256Digest.ComputeFile(packagePath);

            if (!string.Equals(actualSha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new UpdatePackageVerificationException
                (
                    UpdatePackageVerificationFailureKind.DigestMismatch,
                    $"The update package SHA-256 does not match. Expected: {expectedSha256}; Actual: {actualSha256}."
                );
            }

            return new UpdatePackageVerificationResult(actualSize, actualSha256);
        }
    }
}