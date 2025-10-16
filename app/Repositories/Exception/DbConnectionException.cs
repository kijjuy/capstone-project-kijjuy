namespace app.Repositories;

public class DbConnectionException : Exception
{
    public DbConnectionException(String message)
    : base(message)
    { }
}

