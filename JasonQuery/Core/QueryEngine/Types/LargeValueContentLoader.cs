using System;

namespace JasonQuery.Core.QueryEngine.Types
{
    internal static class LargeValueContentLoader
    {
        public static LargeValueContentLoadResult<string> LoadText(Func<string> loader)
        {
            if (loader == null)
            {
                return CreateTextSuccess(string.Empty);
            }

            try
            {
                return CreateTextSuccess(loader() ?? string.Empty);
            }
            catch (Exception ex)
            {
                return new LargeValueContentLoadResult<string>
                {
                    Succeeded = false,
                    Content = string.Empty,
                    Error = ex
                };
            }
        }

        public static LargeValueContentLoadResult<byte[]> LoadBinary(Func<byte[]> loader)
        {
            if (loader == null)
            {
                return CreateBinarySuccess(Array.Empty<byte>());
            }

            try
            {
                return CreateBinarySuccess(loader() ?? Array.Empty<byte>());
            }
            catch (Exception ex)
            {
                return new LargeValueContentLoadResult<byte[]>
                {
                    Succeeded = false,
                    Content = Array.Empty<byte>(),
                    Error = ex
                };
            }
        }

        private static LargeValueContentLoadResult<string> CreateTextSuccess(string content)
        {
            return new LargeValueContentLoadResult<string>
            {
                Succeeded = true,
                Content = content ?? string.Empty
            };
        }

        private static LargeValueContentLoadResult<byte[]> CreateBinarySuccess(byte[] content)
        {
            return new LargeValueContentLoadResult<byte[]>
            {
                Succeeded = true,
                Content = content ?? Array.Empty<byte>()
            };
        }
    }
}