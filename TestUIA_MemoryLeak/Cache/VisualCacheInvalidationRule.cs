namespace TestUIA.Cache
{
    public class VisualCacheInvalidationRule : ICacheInvalidationRule
    {
        public ICacheInvalidationExecutant CreateExecutant()
        {
            return new VisualCacheInvalidationExecutant();
        }
    }
}