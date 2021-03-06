using System;
using System.Threading.Tasks;

namespace HttpServerCore
{
    public interface IRequest
    {
        Uri Url { get; }
        HttpMethodEnum HttpMethod { get; }
        //TODO: string instead of stream? Good idea?
        string Content { get; }
        IResponse Response { get; set; }

        void SendAttachedResponse();
        Task SendAttachedResponseAsync();
    }
}