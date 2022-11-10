using HttpServer.Models;

namespace HttpServer.DatabasePatterns
{
    public class Repository : IRepository<Account>
    {
        static public List<Account> SelectAll()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                return db.Accounts.ToList();
            }
        }
        static public void Delete(int id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var accountToDelete = db.Accounts.Where(a => a.Id == id).FirstOrDefault();
                if (accountToDelete != null)
                {
                    db.Accounts.Remove(accountToDelete);
                    db.SaveChanges();
                    Console.WriteLine("Аккаунт удален!");
                }
            }
        }

        static public void Insert(Account entity)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Accounts.Add(entity);
                db.SaveChanges();
                Console.WriteLine("Аккаунт сохранен!");
            }
        }

        static public Account? Select(int id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                return db.Accounts.Where(p => p.Id == id).FirstOrDefault();
            }
        }

        static public void Update(int id, string propertyName, string newValue)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var account = db.Accounts.Where(a => a.Id == id).FirstOrDefault();

                if (account == null)
                {
                    Console.WriteLine("Нет такого id!");
                    return;
                }

                var properties = account.GetType().GetProperties();
                var propertyToChange = properties.Where(p => p.Name == propertyName).FirstOrDefault();

                if (propertyToChange != null)
                {
                    propertyToChange.SetValue(account, newValue);
                    Console.WriteLine("Свойство изменено.");
                }
                else
                    Console.WriteLine("Нету такого свойства!");
            }
        }
    }
}
