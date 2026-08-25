using JasonQuery.Core.Arrange.MySql.Formatters;

namespace JasonQuery.Core.Arrange.MySql
{
    public class MySqlArrangeStrategy : BaseArrangeStrategy
    {
        private readonly ISchemaPreparationStrategy _schemaPreparation;

        public MySqlArrangeStrategy(ISchemaPreparationStrategy schemaPreparation) : base (MySqlFormatterProvider.Create())
        {
            _schemaPreparation = schemaPreparation;
        }

        protected override void PrepareSchema(ArrangeContext context)
        {
            _schemaPreparation.Prepare(context);
        }
    }
}
