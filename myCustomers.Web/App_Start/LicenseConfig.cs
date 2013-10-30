namespace myCustomers.Web
{
    public class LicenseConfig
    {
        public static void RegisterLicenses()
        {
            var license = new Aspose.Cells.License();
            license.SetLicense(typeof(LicenseConfig).Assembly.GetManifestResourceStream("myCustomers.Web.Properties.Aspose.Cells.lic"));
        }
    }
}