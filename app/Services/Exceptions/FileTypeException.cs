namespace app.Services;

public class FileTypeException : Exception
{
    public FileTypeException(String message)
    : base(message)
    { }

    public FileTypeException(String message, Exception inner)
    : base(message, inner)
    { }
}

