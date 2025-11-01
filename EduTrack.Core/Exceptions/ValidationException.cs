namespace EduTrack.Core.Exceptions
{
    public class ValidationException : ServiceException
    {
        public ValidationException() { }

        public ValidationException(string message)
            : base(message) { }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
