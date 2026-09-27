namespace JasonQuery.Core.Security.JasonQueryDb
{
    public interface IJasonQueryDbKeyProtector
    {
        byte[] Protect(byte[] databaseKey);

        byte[] Unprotect(byte[] protectedDatabaseKey);
    }
}
