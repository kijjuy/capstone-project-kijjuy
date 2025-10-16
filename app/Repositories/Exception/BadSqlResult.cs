namespace app.Repositories;

public class BadSqlResultException : Exception
{
    public BadSqlResultException(String message)
    : base(message)
    { }
}
