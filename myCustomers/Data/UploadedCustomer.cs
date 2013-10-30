using System;
using System.Collections.Generic;
using System.Linq;
using Quartet.Entities;

namespace myCustomers.Data
{
	public class UploadedCustomer
	{
		public enum ActionType
		{
			Skip,
			Add,
			Merge
		}

		public ActionType Action { get; set; }

		public Dictionary<AddressType, Address> Addresses { get { return _addresses; } }

		public ContactInformation ContactInformation { get { return _contactInformation; } }

		public Guid CustomerId
		{
			get { return _customerId; }
			set { _customerId = value; }
		}

		public EmailAddress EmailAddress { get; set; }

		public string EmailAddressText {
			get
			{
				string value = (EmailAddress != null) ? EmailAddress.Address : null;
				return value;
			}
		}

		public Guid ExistingCustomerId { get; set; }

		public string ExistingCustomerEmailAddress { get; set; }

		public string ExistingCustomerFirstName { get; set; }

		public string ExistingCustomerLastName { get; set; }

		public string ExternalPictureUrl { get; set; }

		public string FirstName { get { return ContactInformation.FirstName; } }

		public Dictionary<HyperLinkType, HyperLink> Hyperlinks { get { return _hyperlinks; } }

		public bool IsAddable { get; set; }

		public bool IsFaulted { get; set; }

		public bool IsMergeable { get; set; }

		public bool IsProcessed { get; set; }

		public string LastName { get { return ContactInformation.LastName; } }

		public string Notes { get; set; }

		public Dictionary<PhoneNumberType, PhoneNumber> PhoneNumbers { get { return _phoneNumbers; } }

		public Dictionary<SpecialOccasionType, SpecialOccasion> SpecialOccasions { get { return _specialOccasions; } }

		public Spouse Spouse { get; set; }

        public bool SendEmail { get; set; }

		public Customer GetCustomerToAdd(Guid consultantKey)
		{
			DateTime utcNow = DateTime.UtcNow;

			Customer customer = GetCustomer(consultantKey, utcNow, true);

			Address nominalPrimaryAddress = NominalPrimaryAddress(Addresses);
			if (nominalPrimaryAddress != null)
			{
				customer.Addresses[nominalPrimaryAddress.AddressKey].IsPrimary = true;
			}

			customer.DateAddedUtc = utcNow;

			PhoneNumber nominalPrimaryPhoneNumber = NominalPrimaryPhoneNumber(PhoneNumbers);
			if (nominalPrimaryPhoneNumber != null)
			{
				customer.PhoneNumbers[nominalPrimaryPhoneNumber.PhoneNumberKey].IsPrimary = true;
			}

			return customer;
		}

		public Customer GetCustomerToMerge(Guid consultantKey)
		{
			DateTime utcNow = DateTime.UtcNow;

			Customer customer = GetCustomer(consultantKey, utcNow);

			return customer;
		}

		protected Customer GetCustomer(Guid consultantKey, DateTime utcNow, bool toAdd = false)
		{
			var customer = new Customer
            {
			    ConsultantKey      = consultantKey,
                CustomerId         = toAdd ? Guid.NewGuid() : CustomerId,
			    Addresses          = Addresses.Values.ToDictionary<Address, Guid>(address => address.AddressKey),
			    ContactInformation = ContactInformation,
			    EmailAddress       = EmailAddress
            };
			
			if (!string.IsNullOrWhiteSpace(ExternalPictureUrl))
			{
				//This is where to add the facebook picture url if there is one
			}

			if (!string.IsNullOrWhiteSpace(Notes))
			{
				var noteKey = Guid.NewGuid();
				var note = new CustomerNote
				{
					ConsultantKey   = consultantKey,
					CustomerId      = CustomerId,
					CustomerNoteKey = Guid.NewGuid(),
					NoteType        = CustomerNoteType.RegularNote,
					DateCreatedUtc  = utcNow,
					Content         = Notes
				};
				customer.Notes = new Dictionary<Guid, CustomerNote>(1);
				customer.Notes.Add(note.CustomerNoteKey, note);
			}
			customer.PhoneNumbers     = PhoneNumbers.Values.ToDictionary<PhoneNumber, Guid>(phoneNumber => phoneNumber.PhoneNumberKey);
			customer.SpecialOccasions = SpecialOccasions.Values.ToDictionary<SpecialOccasion, Guid>(specialOccasion => specialOccasion.SpecialOccasionKey);
			customer.HyperLinks       = Hyperlinks.Values.ToDictionary<HyperLink, Guid>(hyperLinks => hyperLinks.HyperLinkKey);

			foreach (KeyValuePair<Guid, HyperLink> link in customer.HyperLinks)
			{
				link.Value.CustomerId = customer.CustomerId;
			}

			// this was put in because quarted complained about it not being there even though we don't have this information to import
			if (customer.PersonalInformation == null) customer.PersonalInformation = new PersonalInformation();

			return customer;
		}

		protected static Address NominalPrimaryAddress(Dictionary<AddressType, Address> addresses)
		{
			Address nominalPrimaryAddress = null;
			if (addresses.ContainsKey(AddressType.Home))
			{
				nominalPrimaryAddress = addresses[AddressType.Home];
			}
			else if (addresses.ContainsKey(AddressType.Business))
			{
				nominalPrimaryAddress = addresses[AddressType.Business];
			}
			else if (addresses.ContainsKey(AddressType.Other))
			{
				nominalPrimaryAddress = addresses[AddressType.Other];
			}
			else if (addresses.Count > 0)
			{
				nominalPrimaryAddress = addresses[0];
			}
			return nominalPrimaryAddress;
		}

		protected static PhoneNumber NominalPrimaryPhoneNumber(Dictionary<PhoneNumberType, PhoneNumber> phoneNumbers)
		{
			PhoneNumber nominalPrimaryPhoneNumber = null;
			if (phoneNumbers.ContainsKey(PhoneNumberType.Home))
			{
				nominalPrimaryPhoneNumber = phoneNumbers[PhoneNumberType.Home];
			}
			else if (phoneNumbers.ContainsKey(PhoneNumberType.Mobile))
			{
				nominalPrimaryPhoneNumber = phoneNumbers[PhoneNumberType.Mobile];
			}
			else if (phoneNumbers.ContainsKey(PhoneNumberType.Work))
			{
				nominalPrimaryPhoneNumber = phoneNumbers[PhoneNumberType.Work];
			}
			else if (phoneNumbers.ContainsKey(PhoneNumberType.Other))
			{
				nominalPrimaryPhoneNumber = phoneNumbers[PhoneNumberType.Other];
			}
			else if (phoneNumbers.Count > 0)
			{
				nominalPrimaryPhoneNumber = phoneNumbers[0];
			}
			return nominalPrimaryPhoneNumber;
		}

		Guid _customerId = Guid.NewGuid();
		private readonly Dictionary<AddressType, Address> _addresses = new Dictionary<AddressType, Address>(5);
		private readonly ContactInformation _contactInformation = new ContactInformation();
		private readonly Dictionary<HyperLinkType, HyperLink> _hyperlinks = new Dictionary<HyperLinkType, HyperLink>(5);
		private readonly Dictionary<PhoneNumberType, PhoneNumber> _phoneNumbers = new Dictionary<PhoneNumberType, PhoneNumber>(5);
		private readonly Dictionary<SpecialOccasionType, SpecialOccasion> _specialOccasions = new Dictionary<SpecialOccasionType, SpecialOccasion>(5);
	}
}
