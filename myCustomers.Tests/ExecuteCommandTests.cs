using System;
using System.ServiceModel;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quartet.Client;
using Quartet.Entities;
using Quartet.Entities.Commands;

namespace myCustomers.Tests
{
    [TestClass]
    public class ExecuteCommandTests
    {
        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void CommandExecuteCanHandleException()
        {
            //Arrange
            var internalClient = A.Fake<ICommandServiceClient>();
            A.CallTo(() => internalClient.Execute(A<AddCustomer>.Ignored)).Throws
            (
                new Exception("Exception Message")
            );

            var client = new myCustomers.QuartetClientFactory.QuartetCommandServiceWrapper(internalClient);

            var command = new AddCustomer();

            //Act
            client.Execute(command);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void CommandExecuteCanHandleFaultException()
        {
            //Arrange
            var internalClient = A.Fake<ICommandServiceClient>();
            A.CallTo(() => internalClient.Execute(A<AddCustomer>.Ignored)).Throws
            (
                new FaultException("ValidationResult fault exception message")
            );

            var client = new myCustomers.QuartetClientFactory.QuartetCommandServiceWrapper(internalClient);

            var command = new AddCustomer();

            //Act
            client.Execute(command);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void CommandExecuteCanHandleFaultExceptionNullValidationMessage()
        {
            //Arrange
            var internalClient = A.Fake<ICommandServiceClient>();
            A.CallTo(() => internalClient.Execute(A<AddCustomer>.Ignored)).Throws
            (
                new FaultException<ValidationResult>(new ValidationResult { Errors = null }, "Null ValidationResult fault exception message")
            );

            var client = new myCustomers.QuartetClientFactory.QuartetCommandServiceWrapper(internalClient);

            var command = new AddCustomer();

            //Act
            client.Execute(command);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void CommandExecuteCanHandleFaultExceptionEmptyValidationMessage()
        {
            //Arrange
            var internalClient = A.Fake<ICommandServiceClient>();
            A.CallTo(() => internalClient.Execute(A<AddCustomer>.Ignored)).Throws
            (
                new FaultException<ValidationResult>(new ValidationResult { Errors = { } }, "Empty ValidationResult fault exception message")
            );

            var client = new myCustomers.QuartetClientFactory.QuartetCommandServiceWrapper(internalClient);

            var command = new AddCustomer();

            //Act
            client.Execute(command);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void CommandExecuteCanHandleFaultExceptionOneValidationMessage()
        {
            //Arrange
            var internalClient = A.Fake<ICommandServiceClient>();
            A.CallTo(() => internalClient.Execute(A<AddCustomer>.Ignored)).Throws
            (
                new FaultException<ValidationResult>(new ValidationResult { Errors = new string[] { "AddCustomer Failed validation due to reasons specified right here" } }, "One ValidationResult fault exception message")
            );

            var client = new myCustomers.QuartetClientFactory.QuartetCommandServiceWrapper(internalClient);

            var command = new AddCustomer();

            //Act
            client.Execute(command);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void CommandExecuteCanHandleFaultExceptionManyValidationMessages()
        {
            //Arrange
            var internalClient = A.Fake<ICommandServiceClient>();
            A.CallTo(() => internalClient.Execute(A<AddCustomer>.Ignored)).Throws
            (
                new FaultException<ValidationResult>(new ValidationResult { Errors = new string[] { "Validation message 1", "Validation message 2" } }, "Many ValidationResult fault exception message")
            );

            var client = new myCustomers.QuartetClientFactory.QuartetCommandServiceWrapper(internalClient);

            var command = new AddCustomer();

            //Act
            client.Execute(command);
        }
    }
}