using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using MaryKay.Configuration;
using myCustomers.Contexts;
using myCustomers.Features;
using myCustomers.Web.Models;
using myCustomers.Web.Services;
using NLog;
using Quartet.Entities;
using Quartet.Entities.Commands;
using Quartet.Entities.Questionnaire;
using Quartet.Entities.Search;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class CustomersController : ApiController
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        readonly IAppSettings           _appSettings;
        readonly IQuartetClientFactory  _clientFactory;
        readonly IConsultantContext     _consultantContext;
		readonly IMailingService        _emailService;
        readonly IFeaturesConfigService _featuresConfig; 
        
        public CustomersController
        (
            IAppSettings appSettings,
            IQuartetClientFactory clientFactory,
            IConsultantContext consultantContext,
            IMappingService<Customer, CustomerDetailViewModel> customerMappingService,
            IMailingService emailService,
            IFeaturesConfigService featuresConfig
        )
        {
            _appSettings       = appSettings;
            _clientFactory     = clientFactory;
            _consultantContext = consultantContext;
			_emailService	   = emailService;
            _featuresConfig    = featuresConfig;
        }

        [AcceptVerbs("GET")]
        public HttpResponseMessage Search(CustomerSearchModel model)
        {
            if (ModelState.IsValid)
            {
                var criteria = new CustomerCriteria
                {
                    QueryString          = model.q,
                    LastNamePrefix       = model.ln,
                    MinAnniversaryMonth  = model.ams,
                    MaxAnniversaryMonth  = model.ame,
                    MinBirthMonth        = model.bdms,
                    MaxBirthMonth        = model.bdme,
                    MinDateAddedUtc      = model.das,
                    MaxDateAddedUtc      = model.dae,
                    MinDateRegisteredUtc = model.drs,
                    MaxDateRegisteredUtc = model.dre,
                    MinLastOrderDateUtc  = model.lods,
                    MaxLastOrderDateUtc  = model.lode,
                    IsDeleted            = model.d,
                    IsArchived           = model.a,
                    QueryFields          = model.qf,
                    ResultFields         = model.rf,
                    Page                 = model.i - 1,
                    PageSize             = model.s,
                    MinProfileDateUtc    = model.pds,
                    MaxProfileDateUtc    = model.pde
                };

                if (!string.IsNullOrWhiteSpace(model.sf))
                {
                    // REVIEW: Consider updating Quartet SearchModel to have an array of sorts
                    // along with associated sort directions
                    var sorts = model.sf.Split(new char[]{'|'}, StringSplitOptions.RemoveEmptyEntries);
                    criteria.Sort = sorts.Select(s => new Sort { Field = s, Descending = model.sd ?? false }).ToArray();
                }

                var client = _clientFactory.GetCustomerSearchClient();

                var json = client.Search(criteria);

                return ApiHelpers.JsonResponseMessage(json);
            }

            throw ApiHelpers.ServerError(ModelState);
        }

        // GET ~/api/customers/{id}
        [AcceptVerbs("GET")]
        public dynamic GetCustomer(Guid id, string part = null)
        {
            var client   = _clientFactory.GetCustomersQueryServiceClient();
            var customer = client.GetCustomer(id);

            if (customer == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            Guid questionnaireId;
            if (!Guid.TryParse(_appSettings.GetValue("QuestionnaireID"), out questionnaireId))
                throw new ArgumentException("QuestionnaireID is missing in the appSettings and is required to load the questionnaire");

            var result = customer.ToCustomerModel();

            // if part parameter is passed then return just part of the customer detail
            switch (part)
            {
                case "addresses":
                    return result.Addresses;
                case "phonenumbers":
                    return result.PhoneNumbers;
            }

            result.ProfileQuestionGroups = GetProfileQuestionsAndAnswers(customer.QuestionnaireItems);

            // return the full detail
            return result;
        }

        // POST ~/api/customers - creates a new customer
        [AcceptVerbs("POST")]
        public dynamic CreateCustomer(CustomerModel model)
        {
            // TODO: validate model state

            var command = model.ToCreateCommand();

            try
            {
                _clientFactory.GetCommandServiceClient().Execute(command);
                model.CustomerId = command.CustomerId;

                try
                {
                    // REVIEW: this check is redundant. The email service does all validaiton of the model
                    // Did the user enter an email address for this customer?
                    if (!string.IsNullOrWhiteSpace(model.EmailAddress))
                    {
                        _emailService.SendOptInMail(model);
                        _logger.Info("Send Opt-in email for new customer with email address.");
                        
                        _emailService.SendInviteToRegister(model);
                        _logger.Info("Send InviteToRegister email for new customer with email address.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                // REST style returns id and hyperlinks in the response
                return new
                {
                    id = command.CustomerId,
                    links = new
                    {
                        self   = new { href = Url.Route("DefaultApi", new { id = command.CustomerId }) },
                        photo  = new { href = Url.Route("Default", new { controller = "customerimages", action = "profile", id = command.CustomerId }) },
                        detail = new { href = Url.Route("Default", new { controller = "customers", action = "detail", id = command.CustomerId }) }
                    }
                };
            }
            catch (CommandException ex)
            {
                throw ApiHelpers.ServerError(ex.Message, string.Join("\r\n", ex.Errors));
            }
        }

        // POST ~/api/customers/{id} - updates an existing customer
        [AcceptVerbs("POST")]
        public dynamic UpdateCustomer(Guid id, CustomerModel model)
        {
            // TODO: validate model state

            var customer = _clientFactory.GetCustomersQueryServiceClient().GetCustomer(model.CustomerId);
            var command  = model.ToSaveCommand(customer);

            try
            {
                var sendEmail = false;

                // REVIEW: This logic doesnt belong here in the controller and should be moved back behind the email service.
                // IMPORTANT - We have to first check to see if the model email address is changing before saving the customer
                // Are we removing an existing address
                // model is null - do nothing
                if (!string.IsNullOrWhiteSpace(model.EmailAddress))
                {
                   // don't send email if customer has no email address or the email address has not changed
                   if (customer.EmailAddress == null || string.IsNullOrWhiteSpace(customer.EmailAddress.Address))
                    {
                        //adding a new address
                        sendEmail = true;
                        _logger.Info("Send Opt-in email for an existing customer with out email address.");
                    }
                    else if (customer.EmailAddress != null
                            && !string.IsNullOrWhiteSpace(customer.EmailAddress.Address)
                            && !string.IsNullOrWhiteSpace(model.EmailAddress)
                            && !customer.EmailAddress.Address.Equals(model.EmailAddress, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // changing existing address
                        // customer has address, model has address but they are different - send email
                        sendEmail = true;
                        _logger.Info("Send Opt-in email for an existing customer with changing email address.");
                    }
                }

                _clientFactory.GetCommandServiceClient().Execute(command);

                try
                {
                    if (sendEmail)
                        _emailService.SendOptInMail(model);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                // REST style returns id and hyperlinks in the response
                return new
                {
                    id = command.CustomerId,
                    links = new
                    {
                        self   = new { href = Url.Route("DefaultApi", new { id = command.CustomerId }) },
                        photo  = new { href = Url.Route("Default", new { controller = "customerimages", action = "profile", id = command.CustomerId }) },
                        detail = new { href = Url.Route("Default", new { controller = "customers", action = "detail", id = command.CustomerId }) }
                    }
                };
            }
            catch (CommandException ex)
            {
                throw ApiHelpers.ServerError(ex.Message, string.Join("\r\n", ex.Errors));
            }
        }

        ProfileQuestionGroup[] GetProfileQuestionsAndAnswers(IDictionary<string, string> questionnaireItems)
        {
            Guid questionnaireId;
            if (!Guid.TryParse(_appSettings.GetValue("QuestionnaireID"), out questionnaireId))
                throw new ArgumentException("QuestionnaireID is missing in the appSettings and is required to load the questionnaire");

            Uri imageRootUrl;
            if (!(Uri.TryCreate(Request.RequestUri, _appSettings.GetValue("Questionnaire.ImageRootUrl"), out imageRootUrl)))
                throw new ArgumentException("Questionnaire.ImageRootUrl is missing or invalid in App.config and is required to load the questionnaire");

            Func<string, Uri> MakeImageUrl = path =>
            {
                if (string.IsNullOrEmpty(path))
                    return null;

                Uri url = null;
                Uri.TryCreate(imageRootUrl, path, out url);
                return url;
            };

            var globalQueryClient = _clientFactory.GetGlobalQueryServiceClient();
            var questionnaire = globalQueryClient.GetQuestionnaireItemById(questionnaireId);

            var answers = questionnaireItems
                .Where(a => !string.IsNullOrWhiteSpace(a.Value))
                .Select(a => new { Key = a.Key, Value = a.Value, Values = a.Value.Split('|') })
                .ToDictionary(a => a.Key, StringComparer.InvariantCultureIgnoreCase);

            Func<Question, string> mapSingle = q =>
            {
                var k = q.QuestionReferenceCode;
                if (answers.ContainsKey(k))
                    if (q.QuestionType != QuestionAnswerTypes.MultipleOptions)
                        return answers[k].Value;
                    else
                        return answers[k].Values.Where
                        (
                            a => !q.QuestionAnswerOptions.Any
                            (
                                o => o.AnswerReferenceCode.Equals(a, StringComparison.InvariantCultureIgnoreCase)
                            )
                        ).FirstOrDefault();

                return null;
            };

            Func<Question, string[]> mapMulti = q =>
            {
                var res = new string[0];
                var k = q.QuestionReferenceCode;
                if (answers.ContainsKey(k))
                {
                    if (q.QuestionType == QuestionAnswerTypes.MultipleOptions)
                        res = answers[k].Values.Where
                        (
                            a => q.QuestionAnswerOptions.Any
                            (
                                o => o.AnswerReferenceCode.Equals(a, StringComparison.InvariantCultureIgnoreCase)
                            )
                        ).ToArray();

                    // Add 'other' key to answers[] if answer length doesnt match
                    if (res.Length != answers[k].Values.Length)
                    {
                        var other = q.QuestionAnswerOptions.FirstOrDefault(o => o.AllowFreeText);
                        if (other != null)
                            res = res.Concat(new[] { other.AnswerReferenceCode }).ToArray();
                    }
                }

                return res;
            };

            var result =
            (
                from g in questionnaire.QuestionGroups
                where g.IsActive
                select new ProfileQuestionGroup
                {
                    Key = g.Id,
                    Text = g.Name,
                    Questions =
                    (
                        from q in g.Questions
                        where q.IsActive
                        let single = mapSingle(q)
                        let multi = mapMulti(q)
                        select new ProfileQuestion
                        {
                            Key = q.QuestionReferenceCode,
                            Type = q.QuestionType,
                            Text = q.Name,
                            Answer = single,
                            Answers = multi,
                            Options =
                            (
                                from o in q.QuestionAnswerOptions
                                where o.IsActive
                                select new ProfileQuestionOption
                                {
                                    Key = o.AnswerReferenceCode,
                                    Text = o.Name,
                                    ImageUrl = MakeImageUrl(o.ImagePath),
                                    AllowFreeText = o.AllowFreeText,
                                }
                            )
                            .ToArray()
                        }
                    )
                    .ToArray()
                }
            )
            .Where(g => g.Questions.Any())
            .ToArray();

            return result;
        }
    }

    [FromUri]
    public class CustomerSearchModel
    {
        public string q { get; set; }       // search terms
        public bool? d { get; set; }        // deleted
        public bool? a { get; set; }        // archived
        public string ln { get; set; }      // last name prefix (rolodex)
        public int? bdms { get; set; }      // birthday month start
        public int? bdme { get; set; }      // birthday month end
        public int? ams { get; set; }       // anniversary month start
        public int? ame { get; set; }       // anniversary month end
        public DateTime? das { get; set; }  // date added start
        public DateTime? dae { get; set; }  // date added end
        public DateTime? pds { get; set; }  // profile date start
        public DateTime? pde { get; set; }  // profile date end
        public DateTime? drs { get; set; }  // date registered start
        public DateTime? dre { get; set; }  // date registered end
        public DateTime? lods { get; set; } // last order date start
        public DateTime? lode { get; set; } // last order date end
        public string[] qf { get; set; }    // the fields to search in (null uses default _all field)
        public string[] rf { get; set; }    // the fields to include in the result (null returns all available fields)
        public string sf { get; set; }      // the field to sort on
        public bool? sd { get; set; }       // sort descending flag
        public int i { get; set; }          // page number
        public int s { get; set; }          // page size

        public CustomerSearchModel()
        {
            i = 1;
            s = 10;
        }
    }

    [FromBody]
    public class CustomerModel
    {
        public Guid CustomerId { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string EmailAddress { get; set; }

        public string Occupation { get; set; }

        public string Employer { get; set; }

        public ContactMethod[] PreferredContactMethods { get; set; }

        public ContactTime[] PreferredContactTimes { get; set; }

        public ContactFrequency[] PreferredContactFrequencies { get; set; }

        public string PreferredLanguage { get; set; }

        public ContactDay[] PreferredContactDays { get; set; }

        public bool CanSendSMSToCell { get; set; }

        public string ReferredBy { get; set; }

        public string Comments { get; set; }

        public Spouse Spouse { get; set; }

        public Gender? Gender { get; set; }

        public Address[] Addresses { get; set; }

        public string Note { get; set; }

        public PhoneNumber[] PhoneNumbers { get; set; }

        public DateTime? PictureLastUpdatedDateUtc { get; set; }

        public SocialNetwork[] SocialNetworks { get; set; }

        public ImportantDate Birthday { get; set; }

        public ImportantDate Anniversary { get; set; }

        public ImportantDate[] ImportantDates { get; set; }

        public string[] LearningTopicsOfInterest { get; set; }

        public Subscription[] Subscriptions { get; set; }

        public DateTime DateAddedUtc { get; set; }

        public DateTime? ProfileDateUtc { get; set; }

        public DateTime? DateRegisteredUtc { get; set; }

        public bool IsRegistered { get; set; }

        public ShippingMethod? PreferredShippingMethod { get; set; }

        public ShoppingMethod[] PreferredShoppingMethods { get; set; }

        public CustomerDeliveryPreference? PreferredDeliveryMethod { get; set; }

        public string PreferredDeliveryMethodDetailsIfOther { get; set; }

        public ProfileQuestionGroup[] ProfileQuestionGroups { get; set; }

        public class ImportantDate
        {
            public Guid Key { get; set; }
            public SpecialOccasionType? Type { get; set; }
            public int Month { get; set; }
            public int Day { get; set; }
            public int? Year { get; set; }
            public string Description { get; set; }
        }

        public class SocialNetwork
        {
            public Guid Key { get; set; }
            public HyperLinkType Type { get; set; }
            public string Url { get; set; }
        }
    }

    public class ProfileQuestionGroup
    {
        public Guid Key { get; set; }
        public string Text { get; set; }
        public ProfileQuestion[] Questions { get; set; }
    }

    public class ProfileQuestion
    {
        public string Key { get; set; }
        public QuestionAnswerTypes Type { get; set; }
        public string Text { get; set; }
        public ProfileQuestionOption[] Options { get; set; }
        public string Answer { get; set; }
        public string[] Answers { get; set; }
    }

    public class ProfileQuestionOption
    {
        public string Key { get; set; }
        public string Text { get; set; }
        public Uri ImageUrl { get; set; }
        public bool AllowFreeText { get; set; }
    }

    public static class ExtendCustomerModel
    {
        public static void Process(this CustomerModel model)
        {
            if (model.PhoneNumbers == null)
                model.PhoneNumbers = new PhoneNumber[0];

            if (model.Addresses == null)
                model.Addresses = new Address[0];

            if (model.ImportantDates == null)
                model.ImportantDates = new CustomerModel.ImportantDate[0];

            if (model.SocialNetworks == null)
                model.SocialNetworks = new CustomerModel.SocialNetwork[0];

            if (model.ProfileQuestionGroups == null)
                model.ProfileQuestionGroups = new ProfileQuestionGroup[0];

            if (model.PreferredShoppingMethods == null)
                model.PreferredShoppingMethods = new ShoppingMethod[0];

            if (model.PreferredContactTimes == null)
                model.PreferredContactTimes = new ContactTime[0];

            if (model.PreferredContactMethods == null)
                model.PreferredContactMethods = new ContactMethod[0];

            if (model.PreferredContactFrequencies == null)
                model.PreferredContactFrequencies = new ContactFrequency[0];

            if (model.PreferredContactDays == null)
                model.PreferredContactDays = new ContactDay[0];

            // filter out empty collection items
            model.PhoneNumbers   = model.PhoneNumbers.Where(p => !string.IsNullOrWhiteSpace(p.Number)).ToArray();
            model.Addresses      = model.Addresses.Where(a => !a.IsEmpty()).ToArray();
            model.ImportantDates = model.ImportantDates.Where(i => i.Month != 0 && i.Day != 0).ToArray();
            model.SocialNetworks = model.SocialNetworks.Where(sn => !string.IsNullOrWhiteSpace(sn.Url)).ToArray();

            // default the addressee field
            foreach (var a in model.Addresses.Where(a => string.IsNullOrWhiteSpace(a.Addressee)))
                a.Addressee = string.Format("{0} {1}", model.FirstName, model.LastName);

            // generate guids for new items
            if (model.Birthday != null && model.Birthday.Key == Guid.Empty)
                model.Birthday.Key = Guid.NewGuid();

            if (model.Anniversary != null && model.Anniversary.Key == Guid.Empty)
                model.Anniversary.Key = Guid.NewGuid();

            foreach (var p in model.PhoneNumbers)
                p.PhoneNumberKey = p.PhoneNumberKey != Guid.Empty ? p.PhoneNumberKey : Guid.NewGuid();

            foreach (var a in model.Addresses)
            {
                a.AddressType = AddressType.Other; // Address type isnt used in this context. Always set to 'Other'
                a.AddressKey = a.AddressKey != Guid.Empty ? a.AddressKey : Guid.NewGuid();
            }

            foreach (var i in model.ImportantDates)
                i.Key = i.Key != Guid.Empty ? i.Key : Guid.NewGuid();

            foreach (var sn in model.SocialNetworks)
                sn.Key = sn.Key != Guid.Empty ? sn.Key : Guid.NewGuid();
        }

        public static bool IsEmpty(this Address a)
        {
            return
                string.IsNullOrWhiteSpace(a.Street) &&
                string.IsNullOrWhiteSpace(a.UnitNumber) &&
                string.IsNullOrWhiteSpace(a.City) &&
                string.IsNullOrWhiteSpace(a.RegionCode) &&
                string.IsNullOrWhiteSpace(a.PostalCode) &&
                string.IsNullOrWhiteSpace(a.CountryCode) &&
                string.IsNullOrWhiteSpace(a.Telephone);
        }

        public static CreateCustomer ToCreateCommand(this CustomerModel model)
        {
            Process(model);

            var command = new CreateCustomer
            {
                CustomerId                  = Guid.NewGuid(),
                FirstName                   = model.FirstName,
                LastName                    = model.LastName,
                MiddleName                  = model.MiddleName,
                EmailAddress                = String.IsNullOrWhiteSpace(model.EmailAddress) ? null : model.EmailAddress,
                ProfileDateUtc              = model.ProfileDateUtc.Value.ToUniversalTime(),
                PreferredLanguage           = model.PreferredLanguage,
                Gender                      = model.Gender,
                CanSendSMSToCell            = model.CanSendSMSToCell,
                Spouse                      = model.Spouse,
                ReferredBy                  = model.ReferredBy,
                Employer                    = model.Employer,
                Occupation                  = model.Occupation,
                LearningTopicsOfInterest    = new HashSet<string>(model.LearningTopicsOfInterest),
                PreferredShoppingMethods    = new HashSet<ShoppingMethod>(model.PreferredShoppingMethods),
                PreferredContactDays        = new HashSet<ContactDay>(model.PreferredContactDays),
                PreferredContactMethods     = new HashSet<ContactMethod>(model.PreferredContactMethods),
                PreferredContactFrequencies = new HashSet<ContactFrequency>(model.PreferredContactFrequencies),
                PreferredContactTimes       = new HashSet<ContactTime>(model.PreferredContactTimes),
                PhoneNumbers                = model.PhoneNumbers,
                Addresses                   = model.Addresses,
                Note                        = String.IsNullOrWhiteSpace(model.Note) ? null : model.Note
            };

            command.HyperLinks = model.SocialNetworks
                .Select
                (
                    sn =>
                    new HyperLink
                    {
                        CustomerId    = command.CustomerId, // This is lame. Need to fix in Quartet
                        HyperLinkKey  = sn.Key,
                        HyperLinkType = sn.Type,
                        Url           = new UriBuilder(sn.Url).Uri
                    }
                )
                .ToArray();

            command.EmailSubscriptions = model.Subscriptions
                .Select
                (
                    sb =>
                    new CreateCustomer.EmailSubscription
                    {
                        SubscriptionType = sb.SubscriptionType,
                        Status           = sb.SubscriptionStatus
                    }
                )
                .ToArray();

            var specialOccasions = model.ImportantDates
                .Select
                (
                    i => new SpecialOccasion
                    {
                        SpecialOccasionKey  = i.Key,
                        SpecialOccasionType = i.Type.Value,
                        Month               = i.Month,
                        Day                 = i.Day,
                        Year                = i.Year,
                        Description         = i.Description
                    }
                );

            if (model.Birthday != null && model.Birthday.Month != 0 && model.Birthday.Day != 0)
                specialOccasions = specialOccasions.Concat
                (
                    new[] 
                    { 
                        new SpecialOccasion
                        {
                            SpecialOccasionKey  = model.Birthday.Key,
                            SpecialOccasionType = model.Birthday.Type.Value,
                            Month               = model.Birthday.Month,
                            Day                 = model.Birthday.Day,
                            Year                = model.Birthday.Year
                        }
                    }
                );

            if (model.Anniversary != null && model.Anniversary.Month != 0 && model.Anniversary.Day != 0)
                specialOccasions = specialOccasions.Concat
                (
                    new[] 
                    { 
                        new SpecialOccasion
                        {
                            SpecialOccasionKey  = model.Anniversary.Key,
                            SpecialOccasionType = model.Anniversary.Type.Value,
                            Month               = model.Anniversary.Month,
                            Day                 = model.Anniversary.Day,
                            Year                = model.Anniversary.Year
                        }
                    }
                );

            command.SpecialOccasions = specialOccasions.ToArray();

            command.QuestionnaireItems = MapQuestionnaireAnswers(model.ProfileQuestionGroups);

            return command;
        }

        public static SaveCustomer ToSaveCommand(this CustomerModel model, Customer customer)
        {
            Process(model);

            // TODO: add new note and subscriptions
            var command = new SaveCustomer
            {
                CustomerId                  = model.CustomerId,
                FirstName                   = model.FirstName,
                LastName                    = model.LastName,
                MiddleName                  = model.MiddleName,
                EmailAddress                = string.IsNullOrWhiteSpace(model.EmailAddress) ? null : model.EmailAddress,
                ProfileDateUtc              = model.ProfileDateUtc.HasValue ? (DateTime?)model.ProfileDateUtc.Value.ToUniversalTime() : null,
                PreferredLanguage           = model.PreferredLanguage,
                Gender                      = model.Gender,
                CanSendSMSToCell            = model.CanSendSMSToCell,
                Spouse                      = model.Spouse,
                ReferredBy                  = model.ReferredBy,
                Employer                    = model.Employer,
                Occupation                  = model.Occupation,
                LearningTopicsOfInterest    = new HashSet<string>(model.LearningTopicsOfInterest),
                PreferredShoppingMethods    = new HashSet<ShoppingMethod>(model.PreferredShoppingMethods),
                PreferredContactDays        = new HashSet<ContactDay>(model.PreferredContactDays),
                PreferredContactMethods     = new HashSet<ContactMethod>(model.PreferredContactMethods),
                PreferredContactFrequencies = new HashSet<ContactFrequency>(model.PreferredContactFrequencies),
                PreferredContactTimes       = new HashSet<ContactTime>(model.PreferredContactTimes),
                PhoneNumbers                = model.PhoneNumbers,
                Addresses                   = model.Addresses,
                Note                        = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note
            };

            var bookMarks = customer.HyperLinks.Values.Where(h => h.HyperLinkType == HyperLinkType.Bookmark);
            
            command.HyperLinks = model.SocialNetworks
                                 .Select
               (
                   sn =>
                   new HyperLink
                   {
                       CustomerId = model.CustomerId, // This is lame. Need to fix in Quartet
                       HyperLinkKey = sn.Key,
                       HyperLinkType = sn.Type,
                       Url = new UriBuilder(sn.Url).Uri
                   }
               )
               .Concat(bookMarks).ToArray();


            command.EmailSubscriptions = model.Subscriptions
                .Select
                (
                    sb =>
                    new SaveCustomer.EmailSubscription
                    {
                        SubscriptionType = sb.SubscriptionType,
                        Status           = sb.SubscriptionStatus
                    }
                )
                .ToArray();

            var specialOccasions = model.ImportantDates
                .Select
                (
                    i => new SpecialOccasion
                    {
                        SpecialOccasionKey  = i.Key,
                        SpecialOccasionType = i.Type.Value,
                        Month               = i.Month,
                        Day                 = i.Day,
                        Year                = i.Year,
                        Description         = i.Description
                    }
                );

            if (model.Birthday != null && model.Birthday.Month != 0 && model.Birthday.Day != 0)
                specialOccasions = specialOccasions.Concat
                (
                    new[] 
                    { 
                        new SpecialOccasion
                        {
                            SpecialOccasionKey  = model.Birthday.Key,
                            SpecialOccasionType = model.Birthday.Type.Value,
                            Month               = model.Birthday.Month,
                            Day                 = model.Birthday.Day,
                            Year                = model.Birthday.Year
                        }
                    }
                );

            if (model.Anniversary != null && model.Anniversary.Month != 0 && model.Anniversary.Day != 0)
                specialOccasions = specialOccasions.Concat
                (
                    new[] 
                    { 
                        new SpecialOccasion
                        {
                            SpecialOccasionKey  = model.Anniversary.Key,
                            SpecialOccasionType = model.Anniversary.Type.Value,
                            Month               = model.Anniversary.Month,
                            Day                 = model.Anniversary.Day,
                            Year                = model.Anniversary.Year
                        }
                    }
                );

            command.SpecialOccasions = specialOccasions.ToArray();

            command.QuestionnaireItems = MapQuestionnaireAnswers(model.ProfileQuestionGroups);

            return command;
        }

        public static CustomerModel ToCustomerModel(this Customer customer)
        {
            if (customer.Addresses == null)
                customer.Addresses = new Dictionary<Guid, Address>();

            if (customer.PersonalInformation == null)
                customer.PersonalInformation = new PersonalInformation();

            if (customer.EmailAddress == null)
                customer.EmailAddress = new EmailAddress();

            if (customer.LearningTopicsOfInterest == null)
                customer.LearningTopicsOfInterest = new HashSet<string>();

            if (customer.PhoneNumbers == null)
                customer.PhoneNumbers = new Dictionary<Guid, PhoneNumber>();

            if (customer.HyperLinks == null)
                customer.HyperLinks = new Dictionary<Guid, HyperLink>();

            var model = new CustomerModel
            {
                Addresses                             = customer.Addresses.Values.ToArray(),
                CanSendSMSToCell                      = customer.ContactPreferences.CanSendSMSToCell,
                Comments                              = customer.PersonalInformation.Comments,
                CustomerId                            = customer.CustomerId,
                DateAddedUtc                          = customer.DateAddedUtc,
                DateRegisteredUtc                     = customer.DateRegisteredUtc,
                PreferredContactDays                  = customer.ContactPreferences.Days,
                EmailAddress                          = customer.EmailAddress.Address,
                Employer                              = customer.ContactInformation.Employer,
                FirstName                             = customer.ContactInformation.FirstName,
                Gender                                = customer.PersonalInformation.Gender,
                IsRegistered                          = customer.IsRegisteredForPws,
                LastName                              = customer.ContactInformation.LastName,
                LearningTopicsOfInterest              = customer.LearningTopicsOfInterest.ToArray(),
                MiddleName                            = customer.ContactInformation.MiddleName,
                Occupation                            = customer.ContactInformation.Occupation,
                PhoneNumbers                          = customer.PhoneNumbers.Values.ToArray(),
                PictureLastUpdatedDateUtc             = customer.Pictures.Values.Count > 0 ? (DateTime?)customer.Pictures.Values.FirstOrDefault().LastUpdatedDateUtc : null,
                PreferredContactFrequencies           = customer.ContactPreferences.Frequencies != null ? customer.ContactPreferences.Frequencies.ToArray() : new ContactFrequency[0],
                PreferredContactMethods               = customer.ContactPreferences.Methods != null ? customer.ContactPreferences.Methods.ToArray() : new ContactMethod[0],
                PreferredDeliveryMethod               = customer.PreferredDeliveryMethod,
                PreferredDeliveryMethodDetailsIfOther = customer.PreferredDeliveryMethodDetailsIfOther,
                PreferredLanguage                     = customer.ContactPreferences.PreferredLanguage,
                PreferredShippingMethod               = customer.PreferredShippingMethod,
                PreferredShoppingMethods              = customer.PreferredShoppingMethod != null ? customer.PreferredShoppingMethod.ToArray() : new ShoppingMethod[0],
                PreferredContactTimes                 = customer.ContactPreferences.Time != null ? customer.ContactPreferences.Time.ToArray() : new ContactTime[0],
                ProfileDateUtc                        = customer.ProfileDateUtc,
                ReferredBy                            = customer.PersonalInformation != null ? customer.PersonalInformation.ReferredBy : null,
                Spouse                                = customer.PersonalInformation != null ? customer.PersonalInformation.Spouse : new Spouse(),
                Subscriptions                         = customer.Subscriptions.Values.ToArray(),
            };

            model.SocialNetworks = customer.HyperLinks.Values
                .Select
                (
                    h => new CustomerModel.SocialNetwork
                    {
                        Key  = h.HyperLinkKey,
                        Type = h.HyperLinkType,
                        Url  = h.Url.ToString()
                    }
                )
                .ToArray();

            model.Birthday = customer.SpecialOccasions.Values
                .Where(so => so.SpecialOccasionType == SpecialOccasionType.Birthday)
                .Select
                (
                    so => new CustomerModel.ImportantDate 
                    { 
                        Key   = so.SpecialOccasionKey,
                        Type  = SpecialOccasionType.Birthday,
                        Month = so.Month,
                        Day   = so.Day,
                        Year  = so.Year
                    }
                )
                .FirstOrDefault();

            model.Anniversary = customer.SpecialOccasions.Values
                .Where(so => so.SpecialOccasionType == SpecialOccasionType.Anniversary)
                .Select
                (
                    so => new CustomerModel.ImportantDate 
                    { 
                        Key   = so.SpecialOccasionKey,
                        Type  = SpecialOccasionType.Anniversary,
                        Month = so.Month,
                        Day   = so.Day,
                        Year  = so.Year
                    }
                )
                .FirstOrDefault();

            model.ImportantDates = customer.SpecialOccasions.Values
                .Where(so => so.SpecialOccasionType != SpecialOccasionType.Birthday && so.SpecialOccasionType != SpecialOccasionType.Anniversary)
                .Select
                (
                    so => new CustomerModel.ImportantDate
                    {
                        Key         = so.SpecialOccasionKey,
                        Type        = so.SpecialOccasionType,
                        Month       = so.Month,
                        Day         = so.Day,
                        Year        = so.Year,
                        Description = so.Description
                    }
                )
                .ToArray();

            return model;
        }

        static IDictionary<string, string> MapQuestionnaireAnswers(ProfileQuestionGroup[] groups)
        {
            var result = new Dictionary<string, string>();

            foreach (var group in groups)
            {
                foreach (var question in group.Questions)
                {
                    if (question.Type == QuestionAnswerTypes.MultipleOptions)
                    {
                        if (question.Answers != null)
                        {
                            var values = question.Answers.Except(question.Options.Where(o => o.AllowFreeText).Select(o => o.Key));
                            if (!string.IsNullOrWhiteSpace(question.Answer))
                                values = values.Concat(new[] { question.Answer });
                            var value = string.Join("|", values);
                            result.Add(question.Key, value);
                        }
                    }
                    else
                    {
                        result.Add(question.Key, question.Answer);
                    }
                }
            }

            return result;
        }
    }
}
