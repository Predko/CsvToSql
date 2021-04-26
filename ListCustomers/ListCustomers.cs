using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace CSVtoDataBase
{
    /// <summary>
    /// Класс - обёртка строки DataRow, содержащей данные о клиенте:
    ///  Id - клиента,
    ///  UNP - УНП клиента,
    ///  Name - название,
    ///  Multiple - признак того, что несколько клиентов может соответствовать этому УНП.
    /// </summary>
    public class Customer : IComparable<Customer>, IComparable
    {
        private const string _nameCompany = "NameCompany";
        private const string _unp = "UNP";
        private const string _id = "Id";

        private readonly DataRow customerRow;

        private List<string> keyWords = new List<string>();

        public Customer(DataRow dr) => customerRow = dr;

        public Customer() { }

        /// <summary>
        /// Добавляет ключевое слово в список ключевых слов названия данного клиента
        /// </summary>
        /// <param name="keyWord"></param>
        public void AddKeyWord(string keyWord)
        {
            if (keyWords.Contains(keyWord) == false)
            {
                keyWords.Add(Name);
            }
        }

        /// <summary>
        /// Удаляет ключевое слово из списка ключевых слов названия данного клиента
        /// </summary>
        /// <param name="keyWord"></param>
        public void RemoveKeyWord(string keyWord)
        {
            if (keyWords.Contains(keyWord) == true)
            {
                keyWords.Remove(Name);
            }
        }

        /// <summary>
        /// Ищет ключевые слова в названии клиента. 
        /// Если нет хотя бы одного слова в названии, возвращает false
        /// </summary>
        /// <param name="nameCompany">Название организации.</param>
        /// <returns>True - если все ключевые слова в названии есть, иначе false.</returns>
        public bool ThisCustomer(string nameCompany)
        {
            string name = nameCompany.ToLower();

            if (Name.ToLower() == name)
            {
                return true;
            }

            if (keyWords.Count == 0)
            {
                return false;
            }

            foreach (string kw in keyWords)
            {
                if (name.Contains(kw) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Идентификатор клиента.
        /// </summary>
        public int Id { get => (int)customerRow[_id]; set => customerRow[_id] = value; }

        /// <summary>
        /// Название клиента.
        /// </summary>
        public string Name
        {
            get
            {
                if (customerRow[_nameCompany] is string name)
                {
                    return name;
                }

                return "";
            }

            set => customerRow[_nameCompany] = value ?? "";
        }

        /// <summary>
        /// Унп клиента.
        /// </summary>
        public string UNP
        {
            get
            {
                if (customerRow[_unp] is string unp)
                {
                    return unp;
                }

                return "";
            }

            set => customerRow[_unp] = value ?? "";
        }

        public static int GetId(DataRow dr)
        {
            return (int)dr[_id];
        }

        public static string GetName(DataRow dr)
        {
            if (dr[_nameCompany] is string name)
            {
                return name;
            }

            return "";
        }

        public static void SetName(DataRow dr, string name)
        {
            dr[_nameCompany] = name;
        }

        public static string GetUNP(DataRow dr)
        {
            if (dr[_unp] is string unp)
            {
                return unp;
            }

            return "";
        }

        public static void SetUNP(DataRow dr, string unp)
        {
            dr[_unp] = unp;
        }

        public override string ToString()
        {
            string sUNP = (UNP.Length <= 1) ? "         " : UNP;

            return $"{sUNP} {Name}";
        }

        #region IComparable<Customer>

        public int CompareTo(object obj)
        {
            if (obj is Customer c)
            {
                return Name.CompareTo(c.Name);
            }

            return Name.CompareTo(obj.ToString());

        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }

            if (obj is Customer c)
            {
                return this == c;
            }

            return (object)this == obj;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        int IComparable<Customer>.CompareTo(Customer other)
        {
            return Name.CompareTo(other.Name);
        }

        public static bool operator ==(Customer left, Customer right)
        {
            if (left is null)
            {
                return right is null;
            }

            if (right == null)
            {
                return false;
            }

            return left.ThisCustomer(right.Name);
        }

        public static bool operator !=(Customer left, Customer right)
        {
            return !(left == right);
        }

        public static bool operator <(Customer left, Customer right)
        {
            return (left is null) ? (right is object) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(Customer left, Customer right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(Customer left, Customer right)
        {
            return left is object && left.CompareTo(right) > 0;
        }

        public static bool operator >=(Customer left, Customer right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
        #endregion IComparable<Customer>
    }

    /// <summary>
    /// Класс - обеспечивающий доступ к списку клиентов.
    /// У каждого клиента есть Id, УНП и наименование
    /// Можно заполнять и сохранять список из файла или потока.
    /// Можно искать клиента в таблице.
    /// </summary>
    public class ListCustomers : IEnumerable<Customer>
    {
        /// <summary>
        /// Таблица с данными о клиентах в формате:
        /// - ID клиента - int,
        /// - UNP клиента - string,
        /// - Name клиента - string.
        /// </summary>
        /// 
        private readonly DataTable customers;

        /// <summary>
        /// УНП бюджетных организаций.
        /// </summary>
        public string UnpBudget;

        public ListCustomers(DataTable dtCustomers)
        {
            customers = dtCustomers;
        }

        public const Customer NotFound = null;

        /// <summary>
        /// Возвращает число записей в таблице.
        /// </summary>
        /// <returns>Число записей в таблице.</returns>
        public int Count => customers.Rows.Count;

        /// <summary>
        /// Заполняет список информацией о клиентах из DataTable с удалением имеющейся информации
        /// </summary>
        /// <param name="dt"></param>
        public void Set(DataTable dt)
        {
            Clear();

            Add(dt);

            customers.AcceptChanges();
        }

        /// <summary>
        /// Добавляет в список таблицу с данными о клиентах.
        /// </summary>
        /// <param name="dt">Данные о клиентах.</param>
        public void Add(DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                Add(row);
            }
        }

        /// <summary>
        /// Добавляет в список данные об одном клиенте.
        /// </summary>
        /// <param name="row">Информация о клиенте.</param>
        public void Add(DataRow row) => Add(Customer.GetId(row), Customer.GetName(row), Customer.GetUNP(row));

        /// <summary>
        /// Добавляет в список данные об одном клиенте.
        /// </summary>
        /// <param name="customer">Объект типа Customer</param>
        public void Add(Customer customer) => Add(customer.Id, customer.Name, customer.UNP);

        /// <summary>
        /// Добавляет наименование клиента с данным id.
        /// </summary>
        /// <param name="id">Id клиента</param>
        /// <param name="name">Наименование</param>
        /// <param name="unp">УНП</param>
        public void Add(object id, object name, object unp)
        {
            DataRow res = customers.Rows.Find(id);

            // Если такой записи нет, добавляем.
            if (res == null)
            {
                DataRow row = customers.NewRow();

                row.ItemArray = new object[] { id, name, unp };

                customers.Rows.Add(row);

                return;
            }

            // Такая запись есть, переписываем её значения.
            Customer.SetName(res, (string)name);
            Customer.SetUNP(res, (string)unp);
        }

        /// <summary>
        /// Полностью очищает список клиентов.
        /// </summary>
        private void Clear() => customers.Clear();

        /// <summary>
        /// Поиск клиента по УНП и названию.
        /// </summary>
        /// <param name="nameCustomer">Название клиента.</param>
        /// <param name="unpCustomer">УНП клиента.</param>
        /// <returns>Экземпляр класса Customer для данного клиента, если поиск успешен, иначе ListCustomers.NotFound = null</returns>
        public Customer Find(string nameCustomer, string unpCustomer)
        {
            //if (unpCustomer == UnpBudget)
            //{
            //    // Этот УНП не может идентифицировать плательщика.
            //    return NotFound;
            //}

            for (int i = 0; i != customers.Rows.Count; i++)
            {
                Customer customer = new Customer(customers.Rows[i]);

                string unp = customer.UNP;

                if (unp.Length == 0)
                {
                    if (customer.ThisCustomer(nameCustomer) == true)
                    {
                        // В записи данного клиента не было УНП - добавляем.
                        customer.UNP = unpCustomer;

                        return customer;
                    }
                }
                else
                if (unp == unpCustomer)
                {
                    return customer;
                }
            }

            return NotFound;
        }

        /// <summary>
        /// Возвращает клиента по id.
        /// </summary>
        /// <param name="id">Id клиента.</param>
        /// <returns>Экземпляр клиента или null - если не найден.</returns>
        public Customer GetCustomerAtId(int id)
        {
            foreach (DataRow dr in customers.Rows)
            {
                if ((int)dr["Id"] == id)
                {
                    return new Customer(dr);
                }
            }

            return null;
        }

        public IEnumerator<Customer> GetEnumerator()
        {
            foreach (DataRow row in customers.Rows)
            {
                yield return new Customer(row);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
