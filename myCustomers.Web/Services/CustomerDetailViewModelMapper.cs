using System;
using System.Linq;
using MaryKay.Configuration;
using myCustomers.Web.Models;
using Quartet.Entities;

namespace myCustomers.Web.Services
{
    public class CustomerDetailViewModelMapper : IMappingService<Customer, CustomerDetailViewModel>
    {
        IMappingService<Address, AddressViewModel> _addressMapping;
        IMappingService<PhoneNumber, PhoneNumberViewModel> _phoneNumberMapping;
        IMappingService<HyperLink, HyperLinkViewModel> _hyperLinkMapping;
        IAppSettings _appSettings;

        public CustomerDetailViewModelMapper
        (
            IMappingService<Address, AddressViewModel> addressMapping,
            IMappingService<PhoneNumber, PhoneNumberViewModel> phoneNumberMapping,
            IMappingService<HyperLink, HyperLinkViewModel> hyperLinkMapping,
            IAppSettings appSettings
        )
        {
            _addressMapping     = addressMapping;
            _phoneNumberMapping = phoneNumberMapping;
            _hyperLinkMapping   = hyperLinkMapping;
            _appSettings = appSettings;
        }
        
        public CustomerDetailViewModel Map(Customer source)
        {
            var sortedAddresses = source.Addresses.Values.Select(a => _addressMapping.Map(a))
                .OrderByDescending(a => a.IsPrimary).ToArray();

            var sortedNotes = source.Notes.Values.OrderByDescending(n => n.DateCreatedUtc).ToArray();

            return new CustomerDetailViewModel
            {
                AccountStatus                           = source.AccountStatus,
                Addresses                               = sortedAddresses,
                BillingAccounts                         = source.BillingAccounts.Values.ToArray(),
                CanSendSMSToCell                        = source.ContactPreferences.CanSendSMSToCell,
                Comments                                = source.PersonalInformation != null ? source.PersonalInformation.Comments : null,
                ConsultantKey                           = source.ConsultantKey,
                CustomerGroups                          = source.CustomerGroups.ToArray(),
                CustomerId                              = source.CustomerId,
                DateAddedUtc                            = source.DateAddedUtc,
                DateRegisteredUtc                       = source.DateRegisteredUtc,
                PreferredContactDays                    = source.ContactPreferences.Days,
                DirectMailIsOptedOut                    = source.DirectMailIsOptedOut,
                EmailAddress                            = source.EmailAddress != null ? source.EmailAddress.Address : null,
                Employer                                = source.ContactInformation.Employer,
                FirstName                               = source.ContactInformation.FirstName,
                Gender                                  = source.PersonalInformation != null ? source.PersonalInformation.Gender : null,
                HyperLinks                              = source.HyperLinks.Values.Where(a => a.HyperLinkType != HyperLinkType.Bookmark).Select(a => _hyperLinkMapping.Map(a)).ToArray(),
                IsDeleted                               = source.IsDeleted,
                IsArchived                              = source.IsArchived,
                IsRegisteredForPws                      = source.IsRegisteredForPws,
                LastName                                = source.ContactInformation.LastName,
                LearningTopicsOfInterest                = source.LearningTopicsOfInterest != null ? source.LearningTopicsOfInterest.ToArray() : new string[0],
                LegacyContactId                         = source.LegacyContactId,
                MiddleName                              = source.ContactInformation.MiddleName,
                Notes                                   = sortedNotes,
                Occupation                              = source.ContactInformation.Occupation,
                PhoneNumbers                            = source.PhoneNumbers.Values.Select(a => _phoneNumberMapping.Map(a)).ToArray(),
                PictureLastUpdatedDateUtc               = source.Pictures.Values.Count > 0 ? (DateTime?)source.Pictures.Values.FirstOrDefault().LastUpdatedDateUtc : null,
                PreferredContactFrequencies             = source.ContactPreferences.Frequencies != null ? source.ContactPreferences.Frequencies.ToArray() : new ContactFrequency[0],
                PreferredContactMethods                 = source.ContactPreferences.Methods != null ? source.ContactPreferences.Methods.ToArray() : new ContactMethod[0],
                PreferredDeliveryMethod                 = source.PreferredDeliveryMethod,
                PreferredDeliveryMethodDetailsIfOther   = source.PreferredDeliveryMethodDetailsIfOther,
                PreferredLanguage                       = source.ContactPreferences.PreferredLanguage,
                PreferredShippingMethod                 = source.PreferredShippingMethod,
                PreferredShoppingMethods                = source.PreferredShoppingMethod != null ? source.PreferredShoppingMethod.ToArray() : new ShoppingMethod[0],
                PreferredContactTimes                   = source.ContactPreferences.Time != null ? source.ContactPreferences.Time.ToArray() : new ContactTime[0],
                ProfileDataCompletenessPercentage       = source.ProfileDataCompletenessPercentage,
                ProfileDateUtc                          = source.ProfileDateUtc.HasValue ? source.ProfileDateUtc.Value : (DateTime?)null,
                QuestionnaireItems                      = source.QuestionnaireItems,
                RecordSource                            = source.RecordSource,
                ReferredBy                              = source.PersonalInformation != null ? source.PersonalInformation.ReferredBy : null,
                SpecialOccasions                        = source.SpecialOccasions.Values.ToArray(),
                Spouse                                  = source.PersonalInformation != null ? source.PersonalInformation.Spouse : null,
                Subscriptions                           = source.Subscriptions.Values.ToArray(),
                WishList                                = source.WishList
            };
        }
    }
}