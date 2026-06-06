namespace Cart.API.Exceptions
{
    public class BadRequestException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }

    public class UnprocessableEntityException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}
