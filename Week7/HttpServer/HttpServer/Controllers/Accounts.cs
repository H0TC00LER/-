using HttpServer.Attributes;
using System.Data.SqlClient;
using HttpServer.Models;
using System.Security.AccessControl;
using HttpServer.DatabasePatterns;

namespace HttpServer.Controllers
{
    [HttpController("accounts")]
    public class Accounts
    {
        [HttpGET]
        public List<Account> GetAccounts()
        {
            return Repository.SelectAll();
        }

        [HttpGET]
        public Account? GetAccountById(int id)
        {
            return Repository.Select(id);
        }

        [HttpPOST]
        public void SaveAccount(string login, string password)
        {
            Repository.Insert(new Account(login, password));
        }
    }
}
