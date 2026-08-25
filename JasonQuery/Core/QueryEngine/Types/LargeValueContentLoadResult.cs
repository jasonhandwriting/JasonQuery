using System;

namespace JasonQuery.Core.QueryEngine.Types
{
    internal sealed class LargeValueContentLoadResult<T>
    {
        public bool Succeeded { get; set; }
        public T Content { get; set; }
        public Exception Error { get; set; }
        public string ErrorMessage => Error?.Message ?? string.Empty;
    }
}
