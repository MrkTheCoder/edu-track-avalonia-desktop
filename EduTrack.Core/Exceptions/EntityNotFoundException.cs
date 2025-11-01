namespace EduTrack.Core.Exceptions
{
    public class EntityNotFoundException : ServiceException
    {
       public EntityNotFoundException() { }

       public EntityNotFoundException(string message)
        :base(message) { }

       public EntityNotFoundException(string message, Exception innerException)
           : base(message, innerException)
       { }
    }
}
