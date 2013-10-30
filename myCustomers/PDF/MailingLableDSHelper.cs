using System;
using iTextSharp.text;
using iTextSharp.text.pdf;
#if TEST
using NUnit.Framework;
#endif

namespace myCustomers.Pdf
{
	
#if TEST
	[TestFixture]
#endif
	public class MailingLableDSHelper
	{
		private MailingLabelDS _ds;

		public MailingLabelDS DataSet
		{
			get
			{
				return _ds;	
			}
		}

	
#if TEST
		public MailingLableDSHelper(){}
#endif

		public MailingLableDSHelper(MailingLabelDS data)
		{
			_ds = data;
		}

        public void AddAddress(string name, string streetUnitNumber, string address, string country)
		{
			var labelRow = _ds.Label.NewLabelRow();

            if (string.IsNullOrEmpty(name))
			{
				labelRow.SetLine1Null();
			}
			else
			{
                labelRow.Line1 = name;
			}

            if (string.IsNullOrEmpty(streetUnitNumber))
			{
				labelRow.SetLine2Null();
			}
			else
			{
                labelRow.Line2 = streetUnitNumber;
			}

            if (string.IsNullOrEmpty(address))
            {
                labelRow.SetLine3Null();
            }
            else
            {
                labelRow.Line3 = address;
            }

            if (string.IsNullOrEmpty(country))
            {
                labelRow.SetLine4Null();
            }
            else
            {
                labelRow.Line4 = country;
            }

			_ds.Label.AddLabelRow(labelRow);
		}
		
#if TEST
		#region Tests
		MailingLabelDS _data = null;	
		MailingLableDSHelper _obj = null;

		//
		// Setup
		//
		[SetUp]
		public void Setup() 
		{
			_data = new MailingLabelDS();
			_obj = new MailingLableDSHelper(_data);
		}

		//
		// Teardown
		//
		[TearDown]
		public void Teardown() 
		{
			_data = null;
			_obj = null;
		}

		[Test]
		public void NewTest() 
		{
			Assert.IsNotNull(_obj);		
		}

		[Test]
		public void DataSetTest()
		{
			Assert.AreSame(_data,_obj.DataSet);
		}

		[Test]
		public void AddUSAddressTest()
		{
			_obj.AddUSAddress("Susie Consultant", "123 Main Street", string.Empty, "Springfield", "TX", "90210");
			MailingLabelDS.LabelRow r = _data.Label[0];
            Assert.AreEqual("Susie Consultant",r.Line1);
			Assert.AreEqual("123 Main Street",r.Line2);
			Assert.IsTrue(r.IsLine3Null());
			Assert.AreEqual("Springfield, TX 90210",r.Line4);
		}

		#endregion
#endif

	}
}
