using System.Text;
using System.Text.Json;

namespace ServerCommunication
{
    public static class CommunicationHelper
    {
        public static byte[] SerializeMessage(MessageBase messageToSerialize)
        {
            return JsonSerializer.SerializeToUtf8Bytes(messageToSerialize.ToMessageWrapper());
        }

        public static MessageWrapper? GetMessageWrapper(byte[] receivedPayloadBytes)
        {
            string receivedJsonPayload = Encoding.UTF8.GetString(receivedPayloadBytes);
            return JsonSerializer.Deserialize<MessageWrapper>(receivedJsonPayload);
        }
    }
}
