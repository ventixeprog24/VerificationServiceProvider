namespace VerificationServiceProvider.Factories
{
    public class VerificationReplyFactory
    {
        public static VerificationReply Success(string message = "")
        {
            return new VerificationReply
            {
                Succeeded = true,
                Message = message
            };
        }

        public static VerificationReply Failed(string error)
        {
            return new VerificationReply
            {
                Succeeded = false,
                Error = error
            };
        }
    }
}
