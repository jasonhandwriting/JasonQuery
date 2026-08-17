using System.Data;

namespace JasonQuery.IntegrationTests.Infrastructure
{
    internal sealed class IntegrationTestQueryExecutionResult
    {
        public DataTable Data { get; set; } = new DataTable();
        public DataTable Schema { get; set; }
        public string ErrorPayload { get; set; } = string.Empty;

        public bool Succeeded
        {
            get
            {
                return string.IsNullOrEmpty(ErrorPayload);
            }
        }
    }
}
