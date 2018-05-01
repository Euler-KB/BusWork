using BookingSystem.API.Models;
using BookingSystem.API.Services.Email;
using BookingSystem.API.Services.Payment;
using BookingSystem.API.Services.SMS;
using System.Configuration;
using System.Data.Entity;
using System.Web.Http;
using Unity;
using Unity.WebApi;

namespace BookingSystem.API
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers
            RegisterEmailService(container);
            RegisterPaymentService(container);
            RegisterSMSService(container);

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }

        static void RegisterEmailService(UnityContainer container)
        {
            string sourceEmail = ConfigurationManager.AppSettings["EmailSource"];
            string password = ConfigurationManager.AppSettings["EmailPassword"];
            string host = ConfigurationManager.AppSettings["EmailHost"];
            int port = int.Parse(ConfigurationManager.AppSettings["EmailPort"]);

            container.RegisterInstance<IEmailService>(new SmtpEmailSender(sourceEmail, password, host, port));
        }

        static void RegisterSMSService(UnityContainer container)
        {
            string clientId = ConfigurationManager.AppSettings["TwilioClientId"];
            string dispatchContact = ConfigurationManager.AppSettings["TwilioDispatchContact"];
            string clientSecret = ConfigurationManager.AppSettings["TwilioClientSecret"];
            container.RegisterInstance<ISMSService>(new TwilioClient(clientId, clientSecret, dispatchContact));
        }

        static void RegisterPaymentService(UnityContainer container)
        {

            string clientId = ConfigurationManager.AppSettings["HubtelClientId"];
            string clientSecret = ConfigurationManager.AppSettings["HubtelClientSecret"];
            string merchatnAccountNo = ConfigurationManager.AppSettings["HubtelMerchantAccount"];

            container.RegisterInstance<IPaymentService>(new HubtelPaymentService(clientId, clientSecret, merchatnAccountNo));
        }
    }
}