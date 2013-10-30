using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using myCustomers.Web.Models;
using myCustomers.Web.Services;
using MaryKay.IBCDataServices.Entities;
using System.Web;
using System.IO;

namespace myCustomers.Tests
{
    [TestClass]
    public class CustomerTests
    {
        [TestMethod]
        public void ImportUploadedCustomersTest()
        {
            var service = A.Fake<IImportCustomerService>();
            A.CallTo(() => service.ImportUploadedCustomers()).Returns(A.Dummy<ImportedCustomer[]>());
        }

        [TestMethod]
        public void GetContactsFromCsvFileTest()
        {
            var service = A.Fake<IImportCustomerService>();

            A.CallTo(() => service.GetContactsFromCSVFile(A.Dummy<HttpPostedFileBase>()));
        }
    }
}
