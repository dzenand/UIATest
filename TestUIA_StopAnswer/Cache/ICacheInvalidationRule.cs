namespace TestUIA.Cache
{
    public interface ICacheInvalidationRule
    {
        ICacheInvalidationExecutant CreateExecutant();
    }
}