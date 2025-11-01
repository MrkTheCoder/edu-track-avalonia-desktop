namespace EduTrack.Core.Exceptions
{
    public class DuplicateEntityException : ServiceException
    {
        public DuplicateEntityException() { }

        public DuplicateEntityException(string message)
            : base(message) { }

        public DuplicateEntityException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
