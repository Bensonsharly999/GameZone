namespace GameZone.Maui.Services;

public class ApiException : Exception
{
    public ApiException(string message) : base(message)
    {
    }
}
