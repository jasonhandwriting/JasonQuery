using JasonQuery.Core.Arrange.Oracle.Formatters;

namespace JasonQuery.Core.Arrange.Oracle
{
    public class OracleArrangeStrategy : BaseArrangeStrategy
    {
        private readonly ISchemaPreparationStrategy _schemaPreparation;

        public OracleArrangeStrategy(ISchemaPreparationStrategy schemaPreparation) : base (OracleFormatterProvider.Create())
        {
            _schemaPreparation = schemaPreparation;
        }

        protected override void PrepareSchema(ArrangeContext context)
        {
            _schemaPreparation.Prepare(context);
        }
    }
}
