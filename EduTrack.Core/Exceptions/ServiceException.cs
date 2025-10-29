namespace EduTrack.Core.Exceptions
{
    public class ServiceException : Exception
    {
        //    It’s recommended to include an empty constructor:
        // For `Serialization compatibility` and `Ease of use`.
        public ServiceException()
        { }

        public ServiceException(string message)
        : base(message)
        { }

        public ServiceException(string message, Exception innerException)
        : base(message, innerException)
        { }
    }
}
