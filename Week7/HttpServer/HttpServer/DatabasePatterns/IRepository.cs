using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.DatabasePatterns
{
    public interface IRepository<T>
    {
        static List<T> SelectAll() { return new List<T>(); }
        static T? Select(int id) { return default(T?); }
        static void Insert(T entity) { }
        static void Update(int id, string propertyName, string newValue) { }
        static void Delete(int id) { }
    }
}
