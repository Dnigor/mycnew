using System;
using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Autofac.Features.ResolveAnything;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using GMF.Client;
using MaryKay.Configuration;
using MaryKay.Configuration.Autofac;
using MaryKay.IBCDataServices.Client;
using Microsoft.Practices.ServiceLocation;
using myCustomers.Contexts;
using myCustomers.ET;
using myCustomers.Facebook;
using myCustomers.Features;
using myCustomers.Globalization;
using myCustomers.Services;
using myCustomers.Services.CDS;
using myCustomers.Services.NetTax;
using myCustomers.VMO;

namespace myCustomers.Web
{
    public class ContainerConfig
    {
        public static void ConfigureContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new AutofacWebTypesModule());
            builder.RegisterControllers(typeof(MvcApplication).Assembly).PropertiesAutowired();
            builder.RegisterApiControllers(typeof(MvcApplication).Assembly);
            builder.RegisterFilterProvider();

            builder.RegisterFileSystemConfigService(HttpContext.Current.Server.MapPath("~/Config"));

            builder.RegisterType<HttpApplicationCache>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<SendMailClient>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ConsultantContext>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<QuartetClientFactory>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ConsultantDataServiceClientFactory>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<VMOLinkComposer>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ETLinkComposer>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<FeaturesConfigService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<FacebookAuthentication>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DisclaimerService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ResxFileResourceProvider>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<Code1AddressVerificationService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<PromotionService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<CDSIntegrationService>().AsImplementedInterfaces().SingleInstance();

            // register application services (~/Services/*)
            builder.RegisterAssemblyTypes(typeof(ContainerConfig).Assembly)
                   .Where(t => t.IsClass && !t.IsAbstract && t.Namespace != null && t.Namespace.EndsWith(".Services"))
                   .AsImplementedInterfaces()
                   .SingleInstance();

            // Construct address validation service based on type specified in the app.config
            builder.Register<IAddressVerificationService>(ctx =>
            {
                var typeName = ctx.Resolve<IAppSettings>().GetValue("Validation.AddressVerificationService");
                if (string.IsNullOrEmpty(typeName))
                    throw new ConfigurationErrorsException("Missing app.config setting for 'Validation.AddressVerificationService'");

                var type = Type.GetType(typeName);
                if (null == type)
                    throw new ConfigurationErrorsException(string.Format("The type '{0}' specified in the app.config setting 'Validation.AddressVerificationService' could not be loaded.", typeName));

                return Activator.CreateInstance(type) as IAddressVerificationService;
            }).InstancePerDependency();

            // Construct payment service based on type specified in the app.config
            builder.Register<ICreditCardService>(ctx =>
            {
                var typeName = ctx.Resolve<IAppSettings>().GetValue("Payment.CreditCardService");
                if (string.IsNullOrEmpty(typeName))
                    throw new ConfigurationErrorsException("Missing app.config setting for 'Payment.CreditCardService'");
                
                var type = Type.GetType(typeName);
                if (null == type)
                    throw new ConfigurationErrorsException(string.Format("The type '{0}' specified in the app.config setting 'Payment.CreditCardService' could not be loaded.", typeName));

                return Activator.CreateInstance(type) as ICreditCardService;
            }).InstancePerDependency();

            builder.Register<IInventoryService>(ctx =>
            {
                var typeName = ctx.Resolve<IAppSettings>().GetValue("Inventory.MkUnavailablePartsService");
                if (string.IsNullOrEmpty(typeName))
                    return null;

                var type = Type.GetType(typeName);
                if (null == type)
                    return null;

                return Activator.CreateInstance(type) as IInventoryService;
            }).InstancePerDependency();

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));
        }
    }
}