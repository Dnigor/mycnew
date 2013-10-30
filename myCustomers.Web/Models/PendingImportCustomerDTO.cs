using System;
using System.Linq;
using myCustomers.Data;

namespace myCustomers.Web.Models
{
    public class PendingImportCustomerDTO
    {
        public Guid CustomerId { get; set; }

        public UploadedCustomer.ActionType ImportAction { get; set; }

        public bool IsMergeAvailable { get; set; }

        public bool IsAddAvailable { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public string ExistingCustomerFirstName { get; set; }
        public string ExistingCustomerLastName { get; set; }
        public string ExistingCustomerEmail { get; set; }
    }

    public static class UploadedCustomerExtensions
    {
        public static PendingImportCustomerDTO ToPendingImportCustomerDTO(this UploadedCustomer uploadedCustomer)
        {
            var dto = new PendingImportCustomerDTO
                          {
                              CustomerId = uploadedCustomer.CustomerId,
                              ImportAction = UploadedCustomer.ActionType.Skip,
                              FirstName = uploadedCustomer.FirstName,
                              LastName = uploadedCustomer.LastName,
                              Email = uploadedCustomer.EmailAddressText,
                              ExistingCustomerFirstName = uploadedCustomer.ExistingCustomerFirstName,
                              ExistingCustomerLastName = uploadedCustomer.ExistingCustomerLastName,
                              ExistingCustomerEmail = uploadedCustomer.ExistingCustomerEmailAddress,
                          };

            var propertiesToHideAddOption = new[]
                                                {
                                                    dto.ExistingCustomerEmail,
                                                    dto.ExistingCustomerFirstName,
                                                    dto.ExistingCustomerLastName
                                                };

            if (propertiesToHideAddOption.Any(p => string.IsNullOrWhiteSpace(p) == false))
            {
                dto.IsMergeAvailable = true;
            }

            if (String.Compare(dto.Email, dto.ExistingCustomerEmail, StringComparison.CurrentCultureIgnoreCase) != 0 ||
                !dto.IsMergeAvailable)
            {
                dto.IsAddAvailable = true;
            }

            return dto;
        }
    }
}