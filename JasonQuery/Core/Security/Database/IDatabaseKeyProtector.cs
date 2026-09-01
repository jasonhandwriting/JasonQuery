namespace JasonQuery.Core.Security.Database
{
    public interface IDatabaseKeyProtector
    {
        byte[] Protect(byte[] databaseKey);

        byte[] Unprotect(byte[] protectedDatabaseKey);
    }
}
