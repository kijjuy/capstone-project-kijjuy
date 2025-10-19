namespace app.Services;

public class BadSqlResultException : Exception
{
    public BadSqlResultException(String message)
    : base(message)
    { }
}
