using System.Data;
using System.Globalization;

namespace ReactSpa_Backend.Helpers;

public class AppException : Exception
{
    public AppException() : base() { }

    public AppException(string message) : base(message) { }

    public AppException(string message, params object[] args)
    : base(String.Format(CultureInfo.CurrentCulture, message, args))
    {
    }
}