using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WebRPC
{
    public class Client<I> where I : class
    {
        private readonly WebRPCClientSettings<I> _settings;
        private readonly string _handler;
        private readonly HttpClient _client;

        public Client(WebRPCClientSettings<I> settings, HttpClient client)
        {
            if (!typeof(I).IsInterface)
            {
                throw new ArgumentException("TypeOf T must be an interface type");
            }

            _handler = typeof(I).FullName;
            _settings = settings;
            _client = client;
            _client.Timeout = TimeSpan.FromMilliseconds(settings.Timeout);
        }
        public Response MakeRequest(string id, string name, params object[] content)
        {
            return MakeRequestAsync(id, name, content).GetAwaiter().GetResult();
        }
        public async Task<Response> MakeRequestAsync(string id, string name, params object[] content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _settings.Url + "/" + name)
            {
                Content = new MessagePackContent(content)
            };

            request.Headers.Add("X-MethodSignature", id);
            request.Headers.Add("X-MethodHandler", _handler);
            try
            {
                var httpResponse = await _client.SendAsync(request);

                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return new Response(new MessagePackStreamReader(await httpResponse.Content.ReadAsStreamAsync()));
                    case HttpStatusCode.NotFound:
                        throw new RPCException("Not Found: " + _settings.Url, null);
                    case HttpStatusCode.InternalServerError:
                        throw await CreateExceptionAsync<Exception>(httpResponse);
                    default:
                        throw await CreateExceptionAsync<Exception>(httpResponse);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<Exception> CreateExceptionAsync<X>(HttpResponseMessage response) where X : Exception
        {
            try
            {
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    return MessagePackSerializer.Deserialize<X>(await response.Content.ReadAsStreamAsync(), ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray));
                }
                else
                {
                    return new Exception("Unknown response: " + await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                var json = await response.Content.ReadAsStringAsync();
                return new RPCException("Could not parse rpc exception", ex)
                {
                    Source = json
                };
            }
        }
    }

    public class Response : IDisposable
    {
        private MessagePackStreamReader _reader;

        internal Response(MessagePackStreamReader reader)
        {
            _reader = reader; ;
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
