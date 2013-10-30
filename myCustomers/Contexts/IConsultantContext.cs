using MaryKay.IBCDataServices.Entities;

namespace myCustomers.Contexts
{
    public interface IConsultantContext
    {
        Consultant Consultant { get; }
        void Clear();
    }
}
