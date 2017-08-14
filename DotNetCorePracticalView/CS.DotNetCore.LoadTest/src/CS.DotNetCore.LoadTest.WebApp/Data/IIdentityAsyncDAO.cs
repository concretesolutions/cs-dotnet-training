namespace CS.DotNetCore.LoadTest.WebApp.Data
{
    using Business;
    using System.Threading.Tasks;
    
    public interface IIdentityAsyncDAO
    {
        Task InsertAsync(Identity identity);

        Task<int> DeleteAllAsync();
    }
}
