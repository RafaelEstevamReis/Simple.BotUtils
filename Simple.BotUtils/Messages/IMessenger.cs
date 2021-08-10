namespace Simple.BotUtils.Messages
{
    public interface IMessengerBase { }
    public interface IMessenger<Req, Resp>
        : IMessengerBase
    {
        Resp Send(Req request);
    }
}
