using System.Collections.Generic;

namespace myCustomers.Services
{
    public interface IInventoryService
    {
        HashSet<string> GetUnavailableParts(string subsidiaryCode);
    }

    public sealed class NullInventoryService : IInventoryService
    {
        static readonly HashSet<string> Empty = new HashSet<string>();

        public HashSet<string> GetUnavailableParts(string subsidiaryCode)
        {
            return Empty;
        }
    }
}
