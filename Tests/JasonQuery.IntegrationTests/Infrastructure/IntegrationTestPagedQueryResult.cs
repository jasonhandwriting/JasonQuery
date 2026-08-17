using System.Data;

namespace JasonQuery.IntegrationTests.Infrastructure
{
    internal sealed class IntegrationTestPagedQueryResult
    {
        public DataTable Data { get; set; }
        public DataTable Schema { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public bool Succeeded
        {
            get
            {
                return string.IsNullOrEmpty(ErrorMessage);
            }
        }
    }
}