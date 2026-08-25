using JasonQuery.Core.Arrange.SqlServer.Formatters;

namespace JasonQuery.Core.Arrange.SqlServer
{
    public class SqlServerArrangeStrategy : BaseArrangeStrategy
    {
        private readonly ISchemaPreparationStrategy _schemaPreparation;

        public SqlServerArrangeStrategy(ISchemaPreparationStrategy schemaPreparation) : base (SqlServerFormatterProvider.Create())
        {
            _schemaPreparation = schemaPreparation;
        }

        protected override void PrepareSchema(ArrangeContext context)
        {
            _schemaPreparation.Prepare(context);
        }
    }
}
