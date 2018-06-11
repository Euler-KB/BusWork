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
            switch (ConfigurationManager.AppSettings["SMS_ENGINE"])
            {
                case "MNotify":
                    {
                        string key = ConfigurationManager.AppSettings["SMS_ENGINE"];
                        container.RegisterInstance<ISMSService>(new MNotify(key));
                    }
                    break;
                case "Twilio":
                    {
                        string clientId = ConfigurationManager.AppSettings["TwilioClientId"];
                        string dispatchContact = ConfigurationManager.AppSettings["TwilioDispatchContact"];
                        string clientSecret = ConfigurationManager.AppSettings["TwilioClientSecret"];
                        container.RegisterInstance<ISMSService>(new TwilioClient(clientId, clientSecret, dispatchContact));
                    }
                    break;
            }

        }

        static void RegisterPaymentService(UnityContainer container)
        {
            switch (ConfigurationManager.AppSettings["PAYMENT_PROCESSOR"])
            {
                case "SlydePay":
                    {
                        string apiVer = ConfigurationManager.AppSettings["SPAY_API_VER"];
                        string merchantEmail = ConfigurationManager.AppSettings["SPAY_EMAIL"];
                        string apiKey = ConfigurationManager.AppSettings["SPAY_API_KEY"];

                        container.RegisterInstance<IPaymentService>(new SlydePayPayment(apiVer, merchantEmail, apiKey,
#if DEBUG
                            true
#else
                            false
#endif
                            ));

                    }
                    break;
                case "AMS":
                    {
                        string appId = ConfigurationManager.AppSettings["AMSPAYMENT_APP_ID"];
                        string apiKey = ConfigurationManager.AppSettings["AMSPAYMENT_API_KEY"];
                        container.RegisterInstance<IPaymentService>(new AMSPaymentService(appId, apiKey));
                    }
                    break;

                case "Hubtel":
                    {
                        string clientId = ConfigurationManager.AppSettings["HubtelClientId"];
                        string clientSecret = ConfigurationManager.AppSettings["HubtelClientSecret"];
                        string merchatnAccountNo = ConfigurationManager.AppSettings["HubtelMerchantAccount"];

                        container.RegisterInstance<IPaymentService>(new HubtelPaymentService(clientId, clientSecret, merchatnAccountNo));
                    }
                    break;
            }

        }
    }
}