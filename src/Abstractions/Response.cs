using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace WebRPC
{
    public class Response : IDisposable
    {
        private MessagePackStreamReader _reader;

        public Response(MessagePackStreamReader reader)
        {
            _reader = reader;
        }
        public T Read<T>()
        {
            return ReadAsync<T>().GetAwaiter().GetResult();
        }
        public async Task<T> ReadAsync<T>()
        {
            if (await _reader.ReadAsync(new CancellationToken()) is ReadOnlySequence<byte> msgpack)
            {
                return MessagePackSerializer.Deserialize<T>(msgpack, ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray));
            }
            throw new DataMisalignedException();
        }
        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }
    }
}
