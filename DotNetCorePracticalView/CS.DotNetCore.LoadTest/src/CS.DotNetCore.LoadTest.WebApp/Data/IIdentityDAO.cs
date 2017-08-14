namespace CS.DotNetCore.LoadTest.WebApp.Data
{
    using Business;
    using System.Threading.Tasks;

    public interface IIdentityDAO
    {
        void Insert(Identity identity);

        int DeleteAll();
    }
}
