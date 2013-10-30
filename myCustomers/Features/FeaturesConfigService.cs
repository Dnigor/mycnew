using MaryKay.Configuration;

namespace myCustomers.Features
{
    public class FeaturesConfigService : IFeaturesConfigService
    {
        IAppSettings _appSettings;

        public FeaturesConfigService(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public OrderFeatures GetOrderFeatures()
        {
            return new OrderFeatures
            {
                PRR                                      = _appSettings.GetValue<bool>("Feature.Orders.PRR"),
                CDS                                      = _appSettings.GetValue<bool>("Feature.Orders.CDS"),
                GWP                                      = _appSettings.GetValue<bool>("Feature.Orders.GWP"),
                CreditCard                               = _appSettings.GetValue<bool>("Feature.Payment.CreditCard"),
                GiftMessage                              = _appSettings.GetValue<bool>("Feature.Orders.GiftMessage"),
                VerifyAddress                            = _appSettings.GetValue<bool>("Feature.Orders.VerifyAddress"),
                ConfirmationEmail                        = _appSettings.GetValue<bool>("Feature.Orders.ConfirmationEmail"),
                ShowCountryCodeWithAddress               = _appSettings.GetValue<bool>("Feature.CustomerList.ShowCountryCodeWithAddress"),
                ShowConfirmationTimePickerInMilitaryTime = _appSettings.GetValue<bool>("Feature.CustomerList.ShowConfirmationTimePickerInMilitaryTime")
            };
        }

        public CustomerDetailFeatures GetCustomerDetailFeatures()
        {
            return new CustomerDetailFeatures
            {
                ExactTargetSuscriptionManagement = _appSettings.GetValue<bool>("Feature.ExactTarget.SuscriptionManagement"),
                UseMailtoLinks                   = _appSettings.GetValue<bool>("Feature.UseMailtoLinks"),
                Ecards                           = _appSettings.GetValue<bool>("Feature.ECards")
            };
        }

        public CustomerListFeatures GetCustomerListFeatures()
        {
            return new CustomerListFeatures
            {
                Rolodex                    = _appSettings.GetValue<bool>("Feature.CustomerList.Rolodex"),
                UseMailtoLinks             = _appSettings.GetValue<bool>("Feature.UseMailtoLinks"),
                ShowCountryCodeWithAddress = _appSettings.GetValue<bool>("Feature.CustomerList.ShowCountryCodeWithAddress"),
                ShowRegionWithAddress      = _appSettings.GetValue<bool>("Feature.ShowRegionWithAddress"),
                Ecards                     = _appSettings.GetValue<bool>("Feature.ECards")
            };
        }

        public ImportCustomersFeatures GetImportCustomersFeatures() 
        {            
            return new ImportCustomersFeatures
            {
                Subscriptions = _appSettings.GetValue<bool>("Feature.ImportCustomers.Subscriptions")
            };
        }

        public EditCustomerFeatures GetEditCustomerFeatures()
        {
            return new EditCustomerFeatures
            {
                Gender                  = _appSettings.GetValue<bool>("Feature.EditCustomer.Gender"),
                Spouse                  = _appSettings.GetValue<bool>("Feature.EditCustomer.Spouse"),
                PreferredShoppingMethod = _appSettings.GetValue<bool>("Feature.EditCustomer.PreferredShoppingMethod"),
                PlaceOfWork             = _appSettings.GetValue<bool>("Feature.EditCustomer.PlaceOfWork"),
                PreferredContactDays    = _appSettings.GetValue<bool>("Feature.EditCustomer.PreferredContactDays"),
                EmailSubscriptions      = _appSettings.GetValue<bool>("Feature.EditCustomer.EmailSubscriptions")
            };
        }

        public AddressViewModelFeatures GetAddressViewModelFeatures()
        {
            return new AddressViewModelFeatures
            {
                ShowCountryCodeWithAddress = _appSettings.GetValue<bool>("Feature.CustomerList.ShowCountryCodeWithAddress"),
                ShowRegionWithAddress      = _appSettings.GetValue<bool>("Feature.ShowRegionWithAddress")
            };
        }

        public GroupDetailFeatures GetGroupDetailFeatures()
        {
            return new GroupDetailFeatures
            {
                ShowRegionWithAddress = _appSettings.GetValue<bool>("Feature.ShowRegionWithAddress"),
                UseMailtoLinks        = _appSettings.GetValue<bool>("Feature.UseMailtoLinks")
            };
        }

        public HCardViewModelFeatures GetHCardViewModelFeatures()
        {
            return new HCardViewModelFeatures
            {
                UseMailtoLinks = _appSettings.GetValue<bool>("Feature.UseMailtoLinks")
            };
        }
    }
}
