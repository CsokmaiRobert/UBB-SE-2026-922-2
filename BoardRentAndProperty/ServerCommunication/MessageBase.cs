using System.Text.Json;

namespace ServerCommunication
{
    public abstract class MessageBase
    {
        public MessageWrapper ToMessageWrapper()
        {
            return new MessageWrapper
            {
                Type = GetType().Name,
                Payload = JsonSerializer.SerializeToUtf8Bytes((object)this)
            };
        }
    }
}
