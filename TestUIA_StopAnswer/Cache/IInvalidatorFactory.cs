using System.Collections.Generic;
using System.Linq;

namespace TestUIA.Cache
{
    internal interface IInvalidatorFactory
    {
        ICacheInvalidationExecutant CreateExecutant();
    }

    internal class InvalidatorFactory : IInvalidatorFactory
    {
        public ICacheInvalidationExecutant CreateExecutant()
        {
            var rules = new List<ICacheInvalidationRule>
                            {
                                new WindowEventsCacheInvalidationRule(),
                                new VisualCacheInvalidationRule()
                            };

            var invalidators = rules.Select(rule => rule.CreateExecutant()).ToList();
            return new WindowCacheInvalidationExecutant(invalidators);
        }
    }
}
