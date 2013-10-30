using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using MaryKay.Configuration;
using MaryKay.IBCDataServices.Entities;
using myCustomers.Contexts;
using myCustomers.Data;
using myCustomers.Facebook;
using myCustomers.Globalization;
using myCustomers.Web.Models;
using Quartet.Client;
using Quartet.Client.Customers;
using Quartet.Services.Contracts;
using QuartetCommands = Quartet.Entities.Commands;
using QuartetServices = Quartet.Services.Contracts;
using QuartetViewEntities = Quartet.Entities.Views;
using myCustomers.Web.Controllers.Api;
using Quartet.Entities;
using NLog;

namespace myCustomers.Web.Services
{
    public class ImportCustomer : IImportCustomerService
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        const int MaxCSVFileSize = 1048576;
        const string UploadedCustomersSessionKey = "UploadedCustomers";
        const string UploadedCustomerGeneratorConfigName = "UploadedCustomerGenerator_OutlookCsv";
        const string FacebookConfigKey = "FacebookFriendsMVCCallbackUrl";

        IConsultantContext _consultantContext;
        IConfigService _configService;
        IQuartetClientFactory _quartetClientFactory;
        IAppSettings _appSettings;
        IMappingService<UploadedCustomer, ImportedCustomer> _customerMappingService;
        IFacebookAuthentication _fbAuth;
        IMailingService _emailService;

        public ImportCustomer
        (
            IConfigService configService,
            IQuartetClientFactory clientFactory,
            IAppSettings appSettings,
            IConsultantContext consultantContext,
            IMappingService<UploadedCustomer, ImportedCustomer> customerMappingService,
            IFacebookAuthentication fbAuth,
            IMailingService emailService
        )
        {
            _configService          = configService;
            _quartetClientFactory   = clientFactory;
            _appSettings            = appSettings;
            _consultantContext      = consultantContext;
            _customerMappingService = customerMappingService;
            _fbAuth                 = fbAuth;
            _emailService           = emailService;
        }

        public List<UploadedCustomer> UploadedCustomers
        {
            get { return HttpContext.Current.Session[UploadedCustomersSessionKey] as List<UploadedCustomer>; }
            private set { HttpContext.Current.Session[UploadedCustomersSessionKey] = value; }
        }

        public string FacebookUrl
        {
            get
            {                               
                var callBackUrl = _appSettings.GetValue(FacebookConfigKey);
                var url = _fbAuth.AuthorizationLinkGet(callBackUrl);
                return url;
            }
        }

        public void GetContactsFromCSVFile(HttpPostedFileBase file)
        {
            ResetUploadedCustomers();

            if (file == null)
                throw new Exception(Resources.GetString("ImportCustomerIndexcsvFileUnspecified_ErrorMessage"));
            else if (file.ContentLength > MaxCSVFileSize)
                throw new Exception(Resources.GetString("ImportCustomerIndexcsvFileSizeInvalid_ErrorMessage"));

            Exception error = null;
            try
            {
                var strUpload         = file.InputStream;
                var srUpload          = new StreamReader(strUpload, true);
                var csvContent        = srUpload.ReadToEnd();
                var csvParser         = new CsvParser(csvContent);
                var columns           = csvParser.Table.Columns;
                var uploadedCustomers = ParseUploadedCustomers(csvParser.Table);

                if (uploadedCustomers != null)
                    SetUploadedCustomers(uploadedCustomers);
                else
                    error = new Exception(Resources.GetString("ImportCustomerIndexcsvContentInvalid_ErrorMessage"));
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                error = new Exception(Resources.GetString("ImportCustomerIndexcsvContentInvalid_ErrorMessage"));
            }

            if (error != null)
                throw error;
        }

        public void GetFacebookContacts(string authToken)
        {
            ResetUploadedCustomers();

            var friendsList = new List<UploadedCustomer>();

            _fbAuth.AccessTokenGet(authToken, _appSettings.GetValue(FacebookConfigKey));

            if (_fbAuth.Token.Length > 0)
            {
                var query = "SELECT uid, first_name, middle_name, last_name, pic_big_with_logo, email, profile_url, birthday_date, languages FROM user WHERE uid IN (SELECT uid2 FROM friend WHERE uid1 = me()) ORDER BY last_name";

                var response = _fbAuth.FQLWebRequest(Method.GET, query, string.Empty);

                var fbObj = FacebookObject.CreateFromString(response);
                var friends = fbObj.Dictionary["data"].Array;

                foreach (var friend in friends)
                {
                    if (friend.IsDictionary)
                    {
                        var dict = friend.Dictionary;

                        var newFriend = new UploadedCustomer();
                        newFriend.ContactInformation.FirstName = dict["first_name"].String;
                        newFriend.ContactInformation.MiddleName = dict["middle_name"].String;
                        newFriend.ContactInformation.LastName = dict["last_name"].String;
                        if (!string.IsNullOrEmpty(dict["pic_big_with_logo"].String))
                            newFriend.ExternalPictureUrl = dict["pic_big_with_logo"].String;

                        if (!string.IsNullOrEmpty(dict["email"].String))
                        {
                            var newEmail = new Quartet.Entities.EmailAddress();
                            newEmail.Address = dict["email"].String;
                            newEmail.EmailAddressStatus = Quartet.Entities.EmailAddressStatus.Valid;
                            newFriend.EmailAddress = newEmail;
                        }

                        if (dict["birthday_date"].String != null)
                        {
                            var birthDate = DateTime.Parse(dict["birthday_date"].String);
                            var newDate = new Quartet.Entities.SpecialOccasion();
                            newDate.SpecialOccasionKey = Guid.NewGuid();
                            newDate.Day = Convert.ToByte(birthDate.Day);
                            newDate.Month = Convert.ToByte(birthDate.Month);
                            newDate.Year = Convert.ToInt16(birthDate.Year);
                            newDate.SpecialOccasionType = Quartet.Entities.SpecialOccasionType.Birthday;
                            newFriend.SpecialOccasions.Add(Quartet.Entities.SpecialOccasionType.Anniversary, newDate);
                        }


                        if (dict["profile_url"].String != null)
                        {
                            var newLink = new Quartet.Entities.HyperLink();
                            newLink.HyperLinkKey = Guid.NewGuid();
                            newLink.HyperLinkType = Quartet.Entities.HyperLinkType.Facebook;
                            var newUri = new Uri(dict["profile_url"].String);
                            newLink.Url = newUri;
                            newFriend.Hyperlinks.Add(Quartet.Entities.HyperLinkType.Facebook, newLink);
                        }

                        friendsList.Add(newFriend);
                    }
                }
            }

            if (friendsList != null && friendsList.Count > 0)
                SetUploadedCustomers(friendsList);
        }

        private CustomerModel Map(UploadedCustomer uploadedCustomer)
        {
            var model = new CustomerModel
            {
                CustomerId   = uploadedCustomer.CustomerId,
                FirstName    = uploadedCustomer.FirstName,
                LastName     = uploadedCustomer.LastName,
                EmailAddress = uploadedCustomer.EmailAddress != null ? uploadedCustomer.EmailAddress.Address : null
            };

            return model;
        }

        public ImportedCustomer[] ImportUploadedCustomers()
        {
            var consultant     = _consultantContext.Consultant;
            var consultantKey  = consultant.ConsultantKey ?? Guid.Empty;
            var commandService = _quartetClientFactory.GetCommandServiceClient();
            var queryService   = _quartetClientFactory.GetCustomersQueryServiceClient();

            foreach (var uploadedCustomer in UploadedCustomers)
            {
                switch (uploadedCustomer.Action)
                {
                    case UploadedCustomer.ActionType.Skip:
                        uploadedCustomer.IsProcessed = true;
                        break;

                    case UploadedCustomer.ActionType.Add:
                        var customerToAdd = uploadedCustomer.GetCustomerToAdd(consultantKey);
                        //PhoneFormatter.ToStorageInPlace(customerToAdd);
                        var cmdImport = new QuartetCommands.ImportCustomer
                        {
                            Customer = customerToAdd
                        };
                        try
                        {
                            commandService.Execute(cmdImport);
                            uploadedCustomer.IsProcessed = true;

                            try
                            {
                                var model = Map(uploadedCustomer);
                                // Did the user enter an email address for this customer?
                                if (!string.IsNullOrWhiteSpace(model.EmailAddress))
                                {
                                    _emailService.SendOptInMail(model);
                                    _logger.Info("Send Opt-in email for new imported customer with email address.");

                                    _emailService.SendInviteToRegister(model);
                                    _logger.Info("Send InviteToRegister email for new imported customer with email address.");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex);
                            }
                        }
                        catch (Exception ex)
                        {
                            uploadedCustomer.IsFaulted = true;
                            _logger.Error(ex);
                        }
                        break;

                    case UploadedCustomer.ActionType.Merge:
                        var customerToMerge = uploadedCustomer.GetCustomerToMerge(consultantKey);
                        //PhoneFormatter.ToStorageInPlace(customerToMerge);
                        var cmdMerge = new QuartetCommands.MergeCustomer()
                        {
                            Customer = customerToMerge,
                            CustomerId = uploadedCustomer.ExistingCustomerId
                        };
                        try
                        {
                            var sendEmail = false;

                            // IMPORTANT - We have to first check to see if the imported customer email address is changing before saving the customer
                            // Are we removing an existing address
                            // model is null - do nothing
                            if (!string.IsNullOrWhiteSpace(uploadedCustomer.EmailAddressText))
                            {
                                // don't send email if customer has no email address or the email address has not changed
                                var customer = queryService.GetCustomer(customerToMerge.CustomerId);

                                if (customer.EmailAddress == null || string.IsNullOrWhiteSpace(customer.EmailAddress.Address))
                                {
                                    //adding a new address
                                    sendEmail = true;
                                    _logger.Info("Send Opt-in email for an existing customer with out email address.");
                                }
                                else if (customer.EmailAddress != null
                                        && !string.IsNullOrWhiteSpace(customer.EmailAddress.Address)
                                        && !string.IsNullOrWhiteSpace(uploadedCustomer.EmailAddressText)
                                        && !customer.EmailAddress.Address.Equals(uploadedCustomer.EmailAddressText, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    // changing existing address
                                    // customer has address, model has address but they are different - send email
                                    sendEmail = true;
                                    _logger.Info("Send Opt-in email for an existing customer with changing email address.");
                                }
                            }

                            commandService.Execute<QuartetCommands.MergeCustomer>(cmdMerge);
                            uploadedCustomer.IsProcessed = true;

                            try
                            {
                                if (sendEmail)
                                {
                                    var model = Map(uploadedCustomer);
                                    _emailService.SendOptInMail(model);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex);
                            }
                        }
                        catch (Exception)
                        {
                            uploadedCustomer.IsFaulted = true;
                        }
                        break;
                }
            }

            MatchUploadedCustomers(UploadedCustomers);
            return UploadedCustomers.Select(u => _customerMappingService.Map(u)).ToArray();
        }

        public void SubmitSubscriptions(ImportedCustomer[] importedCustomers)
        {
            var commandService = _quartetClientFactory.GetCommandServiceClient();
            var queryService   = _quartetClientFactory.GetCustomersQueryServiceClient();

            foreach (var importedCustomer in importedCustomers)
            {
                var customer = queryService.GetCustomer(importedCustomer.CustomerId);
                if (customer == null) 
                    continue;

                foreach (var subscription in importedCustomer.Subscriptions)
                {
                    var customerSubscription = customer.Subscriptions.Select(s => s.Value).Where(s => s.SubscriptionType == subscription.SubscriptionType).FirstOrDefault();
                    if
                        (customerSubscription != null &&
                        (customerSubscription.SubscriptionStatus != subscription.SubscriptionStatus) ||
                        (subscription.SubscriptionStatus == Quartet.Entities.SubscriptionStatus.OptedIn &&
                        customerSubscription == null))
                    {
                        var command = new QuartetCommands.UpdateCustomerSubscription
                        {
                            CustomerId         = subscription.CustomerId,
                            SubscriptionType   = subscription.SubscriptionType,
                            SubscriptionStatus = subscription.SubscriptionStatus,
                            SubscribedCulture  = customer.ContactPreferences.PreferredLanguage ?? "en-US"
                        };

                        commandService.Execute<QuartetCommands.UpdateCustomerSubscription>(command);
                    }
                }
            }
        }

        private List<UploadedCustomer> ParseUploadedCustomers(DataTable table)
        {
            var config = _configService.GetConfigXml(UploadedCustomerGeneratorConfigName);

            var generator = new UploadedCustomerGenerator(config, table.Columns);
            if (generator.IsFunctional)
                return generator.GeneratedList(table);

            return null;
        }

        public void ResetUploadedCustomers()
        {
            UploadedCustomers = new List<UploadedCustomer>();
        }

        private void SetUploadedCustomers(List<UploadedCustomer> uploadedCustomers)
        {
            MatchUploadedCustomers(uploadedCustomers);
            UploadedCustomers = uploadedCustomers;
        }

        private static int CommonPrefixLength(string s1, string s2)
        {
            int commonPrefixLength = 0;
            int s1Length = (s1 != null) ? s1.Length : 0;
            int s2Length = (s2 != null) ? s2.Length : 0;
            int commonLength = (s1Length < s2Length) ? s1Length : s2Length;
            for (int index = 0; index < commonLength; index += 1)
            {
                if (s1.Substring(index, 1).Equals(s2.Substring(index, 1), StringComparison.InvariantCultureIgnoreCase))
                {
                    commonPrefixLength += 1;
                }
                else
                {
                    break;
                }
            }
            return commonPrefixLength;
        }

        private void MatchUploadedCustomers(List<UploadedCustomer> uploadedCustomers)
        {
            var commandService = _quartetClientFactory.GetCommandServiceClient();
            var queryService = _quartetClientFactory.GetCustomersQueryServiceClient();

            var queryParameters = new QuartetServices.QueryParameters();
            var queryResults = queryService.GetCustomerList(queryParameters);
            var customers = queryResults.Data.ToList();

            while (customers.Count < queryResults.TotalCount)
            {
                queryParameters.Page++;
                queryResults = queryService.GetCustomerList(queryParameters);
                customers.AddRange(queryResults.Data.ToList());
            }

            var customersByEmailAddress = new Dictionary<string, QuartetViewEntities.CustomerListItem>(customers.Count, StringComparer.InvariantCultureIgnoreCase);
            var customersByLastName = new Dictionary<string, List<QuartetViewEntities.CustomerListItem>>(customers.Count, StringComparer.InvariantCultureIgnoreCase);

            foreach (var customer in customers)
            {
                var emailAddress = customer.EmailAddress;
                if (!string.IsNullOrWhiteSpace(emailAddress))
                {
                    customersByEmailAddress[emailAddress] = customer;
                }
                var lastName = customer.LastName ?? string.Empty;
                List<QuartetViewEntities.CustomerListItem> customersWithLastName;
                if (customersByLastName.ContainsKey(lastName))
                {
                    customersWithLastName = customersByLastName[lastName];
                    customersWithLastName.Add(customer);
                }
                else
                {
                    customersWithLastName = new List<QuartetViewEntities.CustomerListItem>(5);
                    customersWithLastName.Add(customer);
                    customersByLastName[lastName] = customersWithLastName;
                }
            }

            foreach (var uploadedCustomer in uploadedCustomers)
            {
                if (!uploadedCustomer.IsProcessed)
                {
                    string emailAddress = "";
                    if (uploadedCustomer.EmailAddress != null)
                    {
                        emailAddress = uploadedCustomer.EmailAddress.Address;
                    }
                    if (!string.IsNullOrWhiteSpace(emailAddress) && customersByEmailAddress.ContainsKey(emailAddress))
                    {
                        uploadedCustomer.Action = UploadedCustomer.ActionType.Merge;
                        QuartetViewEntities.CustomerListItem existingCustomer = customersByEmailAddress[emailAddress];
                        uploadedCustomer.CustomerId = existingCustomer.CustomerId;
                        uploadedCustomer.ExistingCustomerId = existingCustomer.CustomerId;
                        uploadedCustomer.ExistingCustomerFirstName = existingCustomer.FirstName;
                        uploadedCustomer.ExistingCustomerLastName = existingCustomer.LastName;
                        uploadedCustomer.ExistingCustomerEmailAddress = existingCustomer.EmailAddress;
                        uploadedCustomer.IsAddable = false;
                        uploadedCustomer.IsMergeable = true;
                    }
                    else
                    {
                        if (customersByLastName.ContainsKey(uploadedCustomer.LastName))
                        {
                            int bestMatchLength = 0;
                            foreach (QuartetViewEntities.CustomerListItem existingCustomer in customersByLastName[uploadedCustomer.LastName])
                            {
                                int matchLength = CommonPrefixLength(existingCustomer.FirstName, uploadedCustomer.FirstName);
                                if (matchLength > bestMatchLength)
                                {
                                    uploadedCustomer.Action = UploadedCustomer.ActionType.Merge;
                                    uploadedCustomer.CustomerId = existingCustomer.CustomerId;
                                    uploadedCustomer.ExistingCustomerId = existingCustomer.CustomerId;
                                    uploadedCustomer.ExistingCustomerFirstName = existingCustomer.FirstName;
                                    uploadedCustomer.ExistingCustomerLastName = existingCustomer.LastName;
                                    uploadedCustomer.ExistingCustomerEmailAddress = existingCustomer.EmailAddress;
                                    uploadedCustomer.IsAddable = true;
                                    uploadedCustomer.IsMergeable = true;
                                    bestMatchLength = matchLength;
                                }
                            }

                            if (bestMatchLength == 0)
                            {
                                uploadedCustomer.Action = UploadedCustomer.ActionType.Skip;
                                uploadedCustomer.ExistingCustomerId = Guid.Empty;
                                uploadedCustomer.ExistingCustomerFirstName = null;
                                uploadedCustomer.ExistingCustomerLastName = null;
                                uploadedCustomer.ExistingCustomerEmailAddress = null;
                                uploadedCustomer.IsAddable = true;
                                uploadedCustomer.IsMergeable = false;
                            }
                        }
                        else
                        {
                            uploadedCustomer.Action = UploadedCustomer.ActionType.Skip;
                            uploadedCustomer.ExistingCustomerId = Guid.Empty;
                            uploadedCustomer.ExistingCustomerFirstName = null;
                            uploadedCustomer.ExistingCustomerLastName = null;
                            uploadedCustomer.ExistingCustomerEmailAddress = null;
                            uploadedCustomer.IsAddable = true;
                            uploadedCustomer.IsMergeable = false;
                        }
                    }
                }
            }
        }
    }
}