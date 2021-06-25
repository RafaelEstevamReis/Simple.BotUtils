using System.Threading.Tasks;

namespace Simple.BotUtils.Messages
{
    public interface IRequestBase { }
    public interface IRequest<T> : IRequestBase
    {
        T Handle();
    }
    public interface IRequest : IRequestBase
    {
        void Handle();
    }
    public interface IRequestAsync<T> : IRequestBase
    {
        Task<T> HandleAsync();
    }
    public interface IRequestAsync : IRequestBase
    {
        Task HandleAsync();
    }
}
