using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using MaryKay.Configuration;
using myCustomers.Contexts;
using NLog;
using Quartet.Client;
using Quartet.Client.Customers;
using Quartet.Client.Notifications;
using Quartet.Client.Ordering;
using Quartet.Client.Products;
using Quartet.Client.Search;
using Quartet.Client.Tasks;
using Quartet.Entities;
using Quartet.Services.Contracts;

namespace myCustomers
{
    public class QuartetClientFactory : IQuartetClientFactory, IProductCatalogClientFactory
    {
        public class QuartetCommandServiceWrapper : ICommandServiceClient
        {
            static readonly Logger _logger = LogManager.GetCurrentClassLogger();

            ICommandServiceClient _client;

            public QuartetCommandServiceWrapper(ICommandServiceClient client)
            {
                _client = client;
            }

            public CommandResult Execute<TCommand>(TCommand command) where TCommand : ICommand
            {
                try
                {
                    return _client.Execute(command);
                }
                catch (FaultException<ValidationResult> vfex)
                {
                    _logger.ErrorException(string.Format("{0}: {1}: {2};",
                        command.GetType(),
                        vfex.Message,
                        string.Join("|", vfex.Detail.Errors ?? new string[] { })), vfex);

                    var errors = new List<string> { vfex.Message };
                    errors.AddRange(vfex.Detail.Errors ?? new string[] { });

                    throw new CommandException { Errors = errors.ToArray() };
                }
                catch (FaultException fex)
                {
                    _logger.ErrorException(string.Format("{0}: {1};", command.GetType(), fex.Message), fex);
                    throw new CommandException { Errors = new string[] { fex.Message } };
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(string.Format("{0}: {1};", command.GetType(), ex.Message), ex);
                    throw new CommandException { Errors = new string[] { ex.Message } };
                }
            }
        }

        static Logger _logger = LogManager.GetCurrentClassLogger();

        string _clientKey;
        ISubsidiaryAccessor _subsidiaryAccessor;
        IConsultantContext _consultantContext;

        public QuartetClientFactory(ISubsidiaryAccessor subsidiaryAcessor, IConsultantContext consultantContext)
        {
            _clientKey = ConfigurationManager.AppSettings["ClientKey"];
            _subsidiaryAccessor = subsidiaryAcessor;
            _consultantContext = consultantContext;
        }

        public ICommandServiceClient GetCommandServiceClient()
        {
            try
            {
                var subsidiaryCode = _subsidiaryAccessor.GetSubsidiaryCode();
                var consultant = _consultantContext.Consultant;

                if (consultant != null)
                    return new QuartetCommandServiceWrapper(new CommandService(_clientKey, subsidiaryCode, consultant.ConsultantKey));

                _logger.Error("ConsultantContext.Consultant is null");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        public IGlobalQueryServiceClient GetGlobalQueryServiceClient()
        {
            try
            {
                var subsidiaryCode = _subsidiaryAccessor.GetSubsidiaryCode();
                var consultant = _consultantContext.Consultant;

                if (consultant != null)
                    return new GlobalQueryService(_clientKey, subsidiaryCode, consultant.ConsultantKey);

                _logger.Error("ConsultantContext.Consultant is null");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        public ICustomersQueryServiceClient GetCustomersQueryServiceClient()
        {
            try
            {
                var subsidiaryCode = _subsidiaryAccessor.GetSubsidiaryCode();
                var consultant = _consultantContext.Consultant;

                if (consultant != null)
                    return new CustomersQueryService(_clientKey, subsidiaryCode, consultant.ConsultantKey);

                _logger.Error("ConsultantContext.Consultant is null");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        public ICustomerPictureServiceClient GetCustomerPictureServiceClient()
        {
            try
            {
                var subsidiaryCode = _subsidiaryAccessor.GetSubsidiaryCode();
                var consultant = _consultantContext.Consultant;

                if (consultant != null)
                    return new CustomerPictureService(_clientKey, subsidiaryCode, consultant.ConsultantKey);

                _logger.Error("ConsultantContext.Consultant is null");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        public IBasketQueryServiceClient GetBasketQueryServiceClient()
        {
            try
            {
                var subsidiaryCode = _subsidiaryAccessor.GetSubsidiaryCode();
                var consultant = _consultantContext.Consultant;

                if (consultant != null)
                    return new BasketQueryService(_clientKey, subsidiaryCode, consultant.ConsultantKey);

                _logger.Error("ConsultantContext.Consultant is null");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        public ITaskQueryServiceClient GetTaskQueryServiceClient()
        {
            try
            {
                var subsidiaryCode = _subsidiaryAccessor.GetSubsidiaryCode();
                var consultant = _consultantContext.Consultant;

                if (consultant != null)
                    return new TaskQueryService(_clientKey, subsidiaryCode, consultant.ConsultantKey);

                _logger.Error("ConsultantContext.Consultant is null");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        public ICustomerSearch GetCustomerSearchClient()
        {
            try
            {
                var consultant = _consultantContext.Consultant;

                if (consultant != null)
                    return CustomerSearch.Create(consultant.ConsultantKey.Value);

                _logger.Error("ConsultantContext.Consultant is null");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        public IOrderSearch GetOrderSearchClient()
        {
            try
            {
                var consultant = _consultantContext.Consultant;

                if (consultant != null)
                    return OrderSearch.Create(consultant.ConsultantKey.Value);

                _logger.Error("ConsultantContext.Consultant is null");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        public ICustomerNoteSearch GetCustomerNotesSearchClient()
        {
            try
            {
                var consultant = _consultantContext.Consultant;

                if (consultant != null)
                    return CustomerNoteSearch.Create(consultant.ConsultantKey.Value);

                _logger.Error("ConsultantContext.Consultant is null");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        public IProductCatalog GetProductCatalogClient()
        {
            return new ProductCatalog(_clientKey, _subsidiaryAccessor.GetSubsidiaryCode());
        }

        public INotificationQueryServiceClient GetNotificationQueryServiceClient()
        {
            try
            {
                var subsidiaryCode = _subsidiaryAccessor.GetSubsidiaryCode();
                var consultant = _consultantContext.Consultant;

                if (consultant != null)
                    if (consultant.ConsultantKey != null)
                        return new NotificationQueryService(_clientKey, subsidiaryCode, (Guid)consultant.ConsultantKey);

                _logger.Error("ConsultantContext.Consultant is null");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        public IPromotionQueryServiceClient GetPromotionQueryServiceClient()
        {
            try
            {
                var subsidiaryCode = _subsidiaryAccessor.GetSubsidiaryCode();
                var consultant = _consultantContext.Consultant;

                if (consultant != null)
                    if (consultant.ConsultantKey != null)
                        return new PromotionQueryService(_clientKey, subsidiaryCode, (Guid)consultant.ConsultantKey);

                _logger.Error("ConsultantContext.Consultant is null");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }
    }
}
