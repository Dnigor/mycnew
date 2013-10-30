namespace myCustomers.Features
{
    public interface IFeaturesConfigService
    {
        OrderFeatures GetOrderFeatures();
        CustomerDetailFeatures GetCustomerDetailFeatures();
        CustomerListFeatures GetCustomerListFeatures();
        ImportCustomersFeatures GetImportCustomersFeatures();
        EditCustomerFeatures GetEditCustomerFeatures();
        AddressViewModelFeatures GetAddressViewModelFeatures();
        GroupDetailFeatures GetGroupDetailFeatures();
        HCardViewModelFeatures GetHCardViewModelFeatures();
    }
}
