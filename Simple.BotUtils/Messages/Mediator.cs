using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.BotUtils.Messages
{
    public class Mediator
    {
        private static readonly Dictionary<Type, IMessengerBase> messengers;
        static Mediator()
        {
            messengers = new Dictionary<Type, IMessengerBase>();
        }

        public static T Send<T>(IRequest<T> request)
        {
            return request.Handle();
        }
        public static void Send(IRequest request)
        {
            request.Handle();
        }

#if !NET40
        public static async Task<T> SendAsync<T>(IRequestAsync<T> request)
        {
            return await request.HandleAsync();
        }
        public static async Task SendAsync(IRequestAsync request)
        {
            await request.HandleAsync();
        }
#endif

        public static void Register(IMessengerBase messenger)
        {
            messengers[messenger.GetType()] = messenger;
        }
        public static TResp Execute<TMsgr, TReq, TResp>(TReq request)
            where TMsgr : IMessenger<TReq, TResp>
        {
            var t = typeof(TMsgr);

            var msgr = (TMsgr)messengers[t];

            return msgr.Send(request);
        }
    }
}
