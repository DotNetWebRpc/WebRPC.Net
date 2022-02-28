using MessagePack;
using MessagePack.Resolvers;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebRPC
{
    public class MessagePackContent : HttpContent
    {
        private readonly Func<Stream, Task> _action;

        public MessagePackContent(Func<Stream, Task> action)
        {
            _action = action;
        }
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            await _action(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            // We can't know the length of the content being pushed to the output stream.
            length = -1;
            return false;
        }
    }
}
