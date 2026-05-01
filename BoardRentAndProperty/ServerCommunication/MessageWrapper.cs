using System;
using System.Text.Json;

namespace ServerCommunication
{
    public class MessageWrapper
    {
        public string Type { get; set; } = string.Empty;
        public byte[] Payload { get; set; } = Array.Empty<byte>();

        public byte[] Serialize()
        {
            return JsonSerializer.SerializeToUtf8Bytes(this);
        }

        public T? Deserialize<T>()
            where T : MessageBase
        {
            return JsonSerializer.Deserialize<T>(Payload);
        }
    }
}
