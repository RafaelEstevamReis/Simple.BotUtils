using Simple.BotUtils.Controllers;
using System;

namespace ControllerSample
{
    public class AuthorizedControllers
    {
        public static void ProgramMain(string[] args)
        {
            var ctrl = new ControllerManager()
                         .AddController<Controllers>();
            ctrl.Filter += Ctrl_Filter;

            User user = null;
            string message = "ShowInfoPublic abc";
            ctrl.ExecuteFromText(context: user, text: message);

            message = "ShowInfoPrivate abc";
            try
            {
                ctrl.ExecuteFromText(context: user, text: message);
            }
            catch (FilteredException ex)
            {

            }
            catch (Exception ex)
            {
            }
        }

        private static void Ctrl_Filter(object sender, FilterEventArgs e)
        {
            var user = e.GetArg<User>();
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
