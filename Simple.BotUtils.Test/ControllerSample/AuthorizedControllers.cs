using Simple.BotUtils.Controllers;
using System;
using System.Linq;

namespace ControllerSample
{
    public class AuthorizedControllers
    {
        public static void ProgramMain(string[] args)
        {
            var ctrl = new ControllerManager()
                         .AddController<Controllers>();
            ctrl.Filter += Ctrl_Filter;


            string message = "ShowInfoPublic abc";
            ctrl.ExecuteFromText(context: (User)null, text: message);

            message = "ShowInfoPrivate abc";
            ctrl.ExecuteFromText(context: (User)null, text: message);
        }

        private static void Ctrl_Filter(object sender, FilterEventArgs e)
        {
            var user = e.Args[0] as User;
            if (e.HasAttribute<AuthorizeAttribute>() && user == null)
            {
                e.BlockReason = new Unauthorized();
            }
            // Ok
        }

        public class Controllers : IController
        {
            [Authorize()]
            public void ShowInfoPrivate(User u, string info) => Console.WriteLine(info);
            public void ShowInfoPublic(User u, string info) => Console.WriteLine(info);
        }

        public class User
        {
            public string Name { get; set; }
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class AuthorizeAttribute : Attribute
        { }
        public class Unauthorized : FilterException
        { }
    }
}
