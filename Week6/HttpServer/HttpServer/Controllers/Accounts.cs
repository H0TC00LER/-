using HttpServer.Attributes;
using System.Data.SqlClient;
using System.Security.Principal;

namespace HttpServer.Controllers
{
    [HttpController("accounts")]
    public class Accounts
    {
        [HttpGET]
        public List<Account> GetAccounts()
        {
            var accounts = new List<Account>();

            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";

            string sqlExpression = "SELECT * FROM Accounts";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows) // если есть данные
                {
                    while (reader.Read()) // построчно считываем данные
                    {
                        accounts.Add(new Account
                            (
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2)
                            ));
                    }
                }

                reader.Close();
            }

            return accounts;
        }

        [HttpGET]
        public Account GetAccountById(int id)
        {
            Account account = null;

            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";
            string sqlExpression = $"SELECT * FROM Accounts WHERE Id = {id}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows) 
                {
                    reader.Read();

                    account = new Account
                        (
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2)
                        );
                }

                reader.Close();
            }

            return account;
        }
        [HttpPOST]
        public void SaveAccount(string login, string password)
        {
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";
            string sqlExpression = $"INSERT INTO Accounts (Login, Password) VALUES ('{login}', '{password}')";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteNonQuery();
            }
        }
    }

    public class Account
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public Account(int id, string login, string password)
        {
            Id = id;
            Login = login;
            Password = password;
        }
    }
}
