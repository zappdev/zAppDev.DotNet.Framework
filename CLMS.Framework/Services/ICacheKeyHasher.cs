namespace CLMS.Framework.Services
{
    public interface ICacheKeyHasher
    {
        string ApiName { get; set; }
        string Operation { get; set; }
        string OriginalKey { get; set; }
        string UserName { get; set; }

        string GetHashedKey();
        ICacheKeyHasher SplitToObject(string hashedKey, string deliminator = "|", bool throwOnException = false);
    }
}