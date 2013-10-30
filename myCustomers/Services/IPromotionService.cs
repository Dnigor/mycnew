using System;
using System.Web;
using System.Web.Caching;
using Quartet.Entities;

namespace myCustomers.Services
{
    public interface ICache
    {
        T Get<T>(string key);
        T Get<T>(string key, Func<T> create, TimeSpan duration);
        object Remove(string key);
    }

    public sealed class HttpApplicationCache : ICache
    {
        static readonly object _sync = new object();

        public T Get<T>(string key)
        {
            return (T)HttpRuntime.Cache.Get(key);
        }

        public T Get<T>(string key, Func<T> add, TimeSpan duration)
        {
            var item = HttpRuntime.Cache.Get(key);
            if (item == null)
            {
                lock (_sync)
                {
                    if (item == null)
                    {
                        item = add();
                        HttpRuntime.Cache.Insert(key, item, null, DateTime.Now.Add(duration), Cache.NoSlidingExpiration);
                    }
                }
            }

            return (T)item;
        }

        public object Remove(string key)
        {
            return HttpRuntime.Cache.Remove(key);
        }
    }

    public interface IPromotionService
    {
        Promotion[] GetActivePromotions();
    }

    public class PromotionService : IPromotionService
    {
        IQuartetClientFactory _clientFactory;
        ICache _cache;

        public PromotionService(IQuartetClientFactory clientFactory, ICache cache)
        {
            _clientFactory = clientFactory;
            _cache         = cache;
        }

        public Promotion[] GetActivePromotions()
        {
            var promotions = _cache
                .Get<Promotion[]>
                (
                    "__ACTIVEPROMOTIONS",
                    () =>
                    {
                        var query = _clientFactory.GetPromotionQueryServiceClient();
                        return query.GetActivePromotions(DateTime.UtcNow);
                    },
                    TimeSpan.FromMinutes(15) // TODO: Get cache duration from config
                );

            return promotions;
        }
    }
}
