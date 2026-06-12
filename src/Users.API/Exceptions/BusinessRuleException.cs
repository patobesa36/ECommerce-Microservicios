namespace Users.API.Exceptions
{
    public class BusinessRuleException(string errorCode, string message, int statusCode = StatusCodes.Status409Conflict) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
        public int StatusCode { get; } = statusCode;
    }
}
