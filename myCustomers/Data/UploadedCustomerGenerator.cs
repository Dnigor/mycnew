using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using QuartetEntities = Quartet.Entities;

namespace myCustomers.Data
{
    public class UploadedCustomerGenerator : BaseGenerator<UploadedCustomer>
    {
        public UploadedCustomerGenerator(XElement xeConfiguration, DataColumnCollection columns)
        {
            var xeAddresses = xeConfiguration.Element("Addresses");
            if (xeAddresses != null)
            {
                AddressGenerators = new List<AddressGenerator>(
                    from XElement xeAddress in xeAddresses.Elements("Address")
                    select new AddressGenerator(xeAddress, columns)
                    );
            }
            else
            {
                AddressGenerators = new List<AddressGenerator>(0);
            }

            var xeContactInformation = xeConfiguration.Element("ContactInformation");
            ContactInformationGenerator = new ContactInformationGenerator(xeContactInformation, columns);

            var xeEmailAddresses = xeConfiguration.Element("EmailAddresses");
            if (xeEmailAddresses != null)
            {
                EmailAddressGenerators = new List<EmailAddressGenerator>(
                    from XElement xeEmailAddress in xeEmailAddresses.Elements("EmailAddress")
                    select new EmailAddressGenerator(xeEmailAddress, columns)
                    );
            }
            else
            {
                EmailAddressGenerators = new List<EmailAddressGenerator>(0);
            }

            var xeNotes = xeConfiguration.Element("Notes");
            NotesGenerator = new TextGenerator(xeNotes, columns);

            var xePhoneNumbers = xeConfiguration.Element("PhoneNumbers");
            if (xePhoneNumbers != null)
            {
                PhoneNumberGenerators = new List<PhoneNumberGenerator>(
                    from XElement xePhoneNumber in xePhoneNumbers.Elements("PhoneNumber")
                    select new PhoneNumberGenerator(xePhoneNumber, columns)
                    );
            }
            else
            {
                PhoneNumberGenerators = new List<PhoneNumberGenerator>(0);
            }

            var xeSpecialOccasions = xeConfiguration.Element("SpecialOccasions");
            if (xeSpecialOccasions != null)
            {
                SpecialOccasionGenerators = new List<SpecialOccasionGenerator>(
                    from XElement xeSpecialOccasion in xeSpecialOccasions.Elements("SpecialOccasion")
                    select new SpecialOccasionGenerator(xeSpecialOccasion, columns)
                    );
            }
            else
            {
                SpecialOccasionGenerators = new List<SpecialOccasionGenerator>(0);
            }

            var xeSpouse = xeConfiguration.Element("Spouse");
            SpouseGenerator = new SpouseGenerator(xeSpouse, columns);

            IsFunctional = ContactInformationGenerator.IsFunctional;
        }

        public override UploadedCustomer GeneratedInstance(DataRow row)
        {
            UploadedCustomer instance = null;
            if ((row != null) && ContactInformationGenerator.IsFunctional)
            {
                QuartetEntities.ContactInformation contactInformation = ContactInformationGenerator.GeneratedInstance(row);
                if (contactInformation != null)
                {
                    instance                               = new UploadedCustomer();
                    instance.ContactInformation.FirstName  = contactInformation.FirstName;
                    instance.ContactInformation.MiddleName = contactInformation.MiddleName;
                    instance.ContactInformation.LastName   = contactInformation.LastName;
                    instance.ContactInformation.Employer   = contactInformation.Employer;
                    instance.ContactInformation.Occupation = contactInformation.Occupation;

                    foreach (var addressGenerator in AddressGenerators)
                    {
                        if (addressGenerator.IsFunctional)
                        {
                            QuartetEntities.Address address = addressGenerator.GeneratedInstance(row);
                            if (address != null)
                            {
                                instance.Addresses.Add(address.AddressType, address);
                            }
                        }
                    }

                    foreach (var emailAddressGenerator in EmailAddressGenerators)
                    {
                        if (emailAddressGenerator.IsFunctional)
                        {
                            QuartetEntities.EmailAddress emailAddress = emailAddressGenerator.GeneratedInstance(row);
                            if (emailAddress != null)
                            {
                                instance.EmailAddress = emailAddress;
                            }
                        }
                    }

                    if (NotesGenerator.IsFunctional)
                    {
                        instance.Notes = NotesGenerator.GeneratedInstance(row);
                    }

                    foreach (var phoneNumberGenerator in PhoneNumberGenerators)
                    {
                        if (phoneNumberGenerator.IsFunctional)
                        {
                            QuartetEntities.PhoneNumber phoneNumber = phoneNumberGenerator.GeneratedInstance(row);
                            if (phoneNumber != null)
                            {
                                instance.PhoneNumbers.Add(phoneNumber.PhoneNumberType, phoneNumber);
                            }
                        }
                    }

                    foreach (var specialOccasionGenerator in SpecialOccasionGenerators)
                    {
                        if (specialOccasionGenerator.IsFunctional)
                        {
                            QuartetEntities.SpecialOccasion specialOccasion = specialOccasionGenerator.GeneratedInstance(row);
                            if (specialOccasion != null)
                            {
                                instance.SpecialOccasions.Add(specialOccasion.SpecialOccasionType, specialOccasion);
                            }
                        }
                    }

                    if (SpouseGenerator.IsFunctional)
                    {
                        instance.Spouse = SpouseGenerator.GeneratedInstance(row);
                    }
                }
            }
            return instance;
        }

        protected List<AddressGenerator> AddressGenerators { get; set; }

        protected ContactInformationGenerator ContactInformationGenerator { get; set; }

        protected List<EmailAddressGenerator> EmailAddressGenerators { get; set; }

        protected TextGenerator NotesGenerator { get; set; }

        protected List<PhoneNumberGenerator> PhoneNumberGenerators { get; set; }

        protected List<SpecialOccasionGenerator> SpecialOccasionGenerators { get; set; }

        protected SpouseGenerator SpouseGenerator { get; set; }
    }
}
