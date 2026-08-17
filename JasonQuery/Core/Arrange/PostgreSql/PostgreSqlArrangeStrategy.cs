using JasonQuery.Core.Arrange.PostgreSql.Formatters;

namespace JasonQuery.Core.Arrange.PostgreSql
{
    public class PostgreSqlArrangeStrategy : BaseArrangeStrategy
    {
        private readonly ISchemaPreparationStrategy _schemaPreparation;

        public PostgreSqlArrangeStrategy(ISchemaPreparationStrategy schemaPreparation) : base (PostgreSqlFormatterProvider.Create())
        {
            _schemaPreparation = schemaPreparation;
        }

        protected override void PrepareSchema(ArrangeContext context)
        {
            _schemaPreparation.Prepare(context);
        }
    }
}