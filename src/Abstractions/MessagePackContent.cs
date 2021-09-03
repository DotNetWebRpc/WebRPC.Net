using MessagePack;
using MessagePack.Resolvers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebRPC
{
    public class MessagePackContent : HttpContent
    {
        private readonly object[] _content;

        public MessagePackContent(object[] content)
        {
            _content = content;
        }
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            for (int i = 0; i < _content.Length; i++)
            {
                await MessagePackSerializer.SerializeAsync(stream, _content[i], ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray));
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            // We can't know the length of the content being pushed to the output stream.
            length = -1;
            return false;
        }
    }
}
