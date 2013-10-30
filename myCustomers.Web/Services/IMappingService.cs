namespace myCustomers.Web.Services
{
    public interface IMappingService<TSource, TResult>
    {
        TResult Map(TSource source);
    }
}
