namespace TestUIA.Cache
{
    public class WindowEventsCacheInvalidationRule : ICacheInvalidationRule
    {
        public ICacheInvalidationExecutant CreateExecutant()
        {
            return new WindowEventsCacheInvalidationExecutant();
        }
    }
}