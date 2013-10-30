using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using myCustomers.Features;
using Quartet.Entities;

namespace myCustomers.Web.Models
{
    public class CustomerDetailViewModel
    {
        public Guid CustomerId { get; set; }

        public Guid ConsultantKey { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

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

        public AddressViewModel[] Addresses { get; set; }

        public CustomerNote[] Notes { get; set; }

        public String Note { get; set; }

        public PhoneNumberViewModel[] PhoneNumbers { get; set; }

        public DateTime? PictureLastUpdatedDateUtc { get; set; }

        public HyperLinkViewModel[] HyperLinks { get; set; }

        public SpecialOccasion[] SpecialOccasions { get; set; }

        public string EmailAddress { get; set; }

        public string[] LearningTopicsOfInterest { get; set; }

        public Subscription[] Subscriptions { get; set; }

        public IDictionary<string, string> QuestionnaireItems { get; set; }

        public IEnumerable<QuestionnaireKeyValueViewModel> QuestionnaireKeyValues { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsArchived { get; set; }

        public string WishList { get; set; }

        public byte ProfileDataCompletenessPercentage { get; set; }

        public DateTime DateAddedUtc { get; set; }

        public bool IsRegisteredForPws { get; set; }

        public ShippingMethod? PreferredShippingMethod { get; set; }

        public ShoppingMethod[] PreferredShoppingMethods { get; set; }

        public bool DirectMailIsOptedOut { get; set; }

        public DateTime? ProfileDateUtc { get; set; }

        public Guid[] CustomerGroups { get; set; }

        public CustomerDeliveryPreference? PreferredDeliveryMethod { get; set; }

        public BillingAccount[] BillingAccounts { get; set; }

        public DateTime? DateRegisteredUtc { get; set; }

        public AccountStatus AccountStatus { get; set; }

        public long? LegacyContactId { get; set; }

        public string RecordSource { get; set; }

        public string PreferredDeliveryMethodDetailsIfOther { get; set; }

        public CustomerDetailFeatures Features { get; set; }

        //Calculated properties
        [IgnoreDataMember]
        public bool HasEmailAddress { get { return !string.IsNullOrEmpty(EmailAddress); } }

        [IgnoreDataMember]
        public bool HasPhoneNumbers { get { return PhoneNumbers != null && PhoneNumbers.Length > 0; } }

        [IgnoreDataMember]
        public bool HasAddresses { get { return Addresses != null && Addresses.Length > 0; } }

        [IgnoreDataMember]
        public bool HasPrimaryAddress { get { return Addresses != null && Addresses.Length > 0 && Addresses.Any(a => a.IsPrimary); } }

        [IgnoreDataMember]
        public bool HasReferredBy { get { return !string.IsNullOrWhiteSpace(ReferredBy); } }

        [IgnoreDataMember]
        public bool HasPreferredLanguage { get { return !string.IsNullOrWhiteSpace(PreferredLanguage); } }

        [IgnoreDataMember]
        public bool HasSpouse { get { return Spouse != null && (HasSpouseEmailAddress || HasSpousePhoneNumber || HasSpouseName); } }

        [IgnoreDataMember]
        public bool HasSpouseEmailAddress { get { return Spouse != null && !string.IsNullOrWhiteSpace(Spouse.EmailAddress); } }

        [IgnoreDataMember]
        public bool HasSpousePhoneNumber { get { return Spouse != null && !string.IsNullOrWhiteSpace(Spouse.PhoneNumber); } }

        [IgnoreDataMember]
        public bool HasSpouseExtension { get { return Spouse != null && !string.IsNullOrWhiteSpace(Spouse.Extension); } }

        [IgnoreDataMember]
        public bool HasSpouseName { get { return Spouse != null && !string.IsNullOrWhiteSpace(Spouse.SpouseName); } }

        [IgnoreDataMember]
        public bool HasEmployer { get { return !string.IsNullOrWhiteSpace(Employer); } }

        [IgnoreDataMember]
        public bool HasOccupation { get { return !string.IsNullOrWhiteSpace(Occupation); } }

        [IgnoreDataMember]
        public bool HasHyperLinks { get { return HyperLinks != null && HyperLinks.Length > 0; } }

        [IgnoreDataMember]
        public bool HasSpecialOccasions { get { return SpecialOccasions != null && SpecialOccasions.Any(); } }

        [IgnoreDataMember]
        public bool HasLearningTopicsOfInterest { get { return LearningTopicsOfInterest != null && LearningTopicsOfInterest.Any(); } }

        [IgnoreDataMember]
        public bool HasPreferredContactDays { get { return PreferredContactDays != null && PreferredContactDays.Any(); } }

        [IgnoreDataMember]
        public bool HasPreferredContactMethods { get { return PreferredContactMethods != null && PreferredContactMethods.Any(); } }

        [IgnoreDataMember]
        public bool HasPreferredContactFrequencies { get { return PreferredContactFrequencies != null && PreferredContactFrequencies.Any(); } }

        [IgnoreDataMember]
        public bool HasPreferredContactTimes { get { return PreferredContactTimes != null && PreferredContactTimes.Any(); } }

        [IgnoreDataMember]
        public bool HasQuestionnaireKeyValues { get { return QuestionnaireKeyValues != null && QuestionnaireKeyValues.Any(); } }

        [IgnoreDataMember]
        public bool HasPreferredShoppingMethods { get { return PreferredShoppingMethods != null && PreferredShoppingMethods.Any(); } }

        [IgnoreDataMember]
        public bool HasNote { get { return !string.IsNullOrWhiteSpace(Note); } }

        [IgnoreDataMember]
        public bool HasProfileDate { get { return ProfileDateUtc.HasValue; } }

        [IgnoreDataMember]
        public bool HasContactOrPersonalOrProfileInfo
        {
            get
            {
                return
                    Gender.HasValue ||
                    HasPreferredShoppingMethods ||
                    HasPreferredContactMethods ||
                    HasHyperLinks ||
                    HasPreferredContactDays ||
                    HasPreferredContactTimes ||
                    HasPreferredContactFrequencies ||
                    HasPreferredLanguage ||
                    HasLearningTopicsOfInterest ||
                    HasSpecialOccasions ||
                    HasSpouse ||
                    HasOccupation ||
                    HasEmployer ||
                    HasQuestionnaireKeyValues;
            }
        }

        [IgnoreDataMember]
        public bool HasMiddleName { get { return !string.IsNullOrWhiteSpace(MiddleName); } }

        [IgnoreDataMember]
        public Subscription[] OptedInSubscriptions 
        { 
            get 
            {
                if (Subscriptions == null || Subscriptions.Length == 0)
                    return new Subscription[0];
                return Subscriptions.Where(s => s.SubscriptionStatus == SubscriptionStatus.OptedIn).ToArray();
            }
        }

        [IgnoreDataMember]
        public bool IsOptedInToEcards
        {
            get
            {
                return
                    this.OptedInSubscriptions != null &&
                    this.OptedInSubscriptions.Any(s => s.SubscriptionType == SubscriptionType.MkeCards && s.SubscriptionStatus == SubscriptionStatus.OptedIn);
            }
        }

        [IgnoreDataMember]
        public bool HasOptedInSubscriptions { get { return OptedInSubscriptions.Length > 1; } }
    }
}