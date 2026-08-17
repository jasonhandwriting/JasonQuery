namespace JasonQuery.Core.Arrange
{
    public interface ISchemaPreparationStrategy
    {
        void Prepare(ArrangeContext context);
    }
}