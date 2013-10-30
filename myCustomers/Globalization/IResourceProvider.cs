using System.Globalization;

namespace myCustomers.Globalization
{
    public interface IResourceProvider
    {
        IResourceSet GetResourceSet(string subsidiaryCode, string resourceSetName, CultureInfo culture = null);
        IResourceSet GetCachedResourceSet(string subsidiaryCode, string resourceSetName, CultureInfo culture = null);
    }
}
