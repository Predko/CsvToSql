using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;

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
        
        public List<string> keywords;

        public Customer(DataRow dr) => customerRow = dr;

        public Customer() { }

        /// <summary>
        /// Добавляет ключевое слово в список ключевых слов названия данного клиента
        /// </summary>
        /// <param name="word"></param>
        public void AddKeyword(string word)
        {
            if (keywords.Contains(word) == false)
            {
                keywords.Add(word);
            }
        }

        /// <summary>
        /// Удаляет ключевое слово из списка ключевых слов названия данного клиента
        /// </summary>
        /// <param name="word"></param>
        public void RemoveKeyword(string word)
        {
            if (keywords.Contains(word) == true)
            {
                keywords.Remove(word);
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

            string[] words = nameCompany.ToLower().Split(new char[] { ' ', '\"', ',', '.', '-' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var kw in keywords)
            {
                int numberOfMatches = 0;

                foreach (string word in words)
                {
                    if (kw == word)
                    {
                        numberOfMatches++;
                    }
                }

                if (numberOfMatches == words.Length)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Идентификатор клиента.
        /// </summary>
        public int Id 
        { 
            get => (int)customerRow[_id]; 
            set => customerRow[_id] = value; 
        }

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
    /// Содержит словарь ключевых слов имён клиентов, который создаётся при инициализации объекта класса.
    /// Обеспечивает поиск клиента по имени(с использованием словаря ключевых слов) или УНП в таблице.
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
        /// Словарь списков ключевых слов для идентификации клиентов.
        /// Ключом является Id клиента.
        /// </summary>
        private readonly Dictionary<int, List<string>> keywords = new Dictionary<int, List<string>>();

        /// <summary>
        /// Список неуникальных слов в названиях клиентов.
        /// </summary>
        private readonly List<string> nonUniqueWords = new List<string>();


        /// <summary>
        /// Создаёт новый список клиентов для данной таблицы.
        /// </summary>
        /// <param name="dtCustomers">Таблица данных клиента.</param>
        public ListCustomers(DataTable dtCustomers)
        {
            customers = dtCustomers;

            InitKeywords();
        }

        /// <summary>
        /// Генерирует список уникальных ключевых слов для каждого клиента.
        /// Все списки помещаются в словарь с ключом равным Id клиента.
        /// </summary>
        private void InitKeywords()
        {
            keywords.Clear();

            nonUniqueWords.Clear();

            foreach (Customer current in this)
            {
                string[] words = current.Name.ToLower().Split(new char[] { ' ', '\"', ',', '.', '-' }, StringSplitOptions.RemoveEmptyEntries);

                // Проверяем наличие всех ключевых слов в словаре.
                // Если такого слова нет добавляем его в список слов с Id данного клиента.
                // Если такое слово есть, значит оно не уникально и удаляем его из словаря.
                foreach (string word in words)
                {
                    // Считаем, что слово уникально.
                    bool uniqueWord = true;

                    // Проверяем наличие данного слова у всех клиентов.
                    foreach (List<string> customerListKeywords in keywords.Values)
                    {
                        // Проверяем наличие данного слова в списке слов данного клиента.
                        if (customerListKeywords.Contains(word) == true)
                        {
                            customerListKeywords.Remove(word);

                            nonUniqueWords.Add(word);

                            // Слово не уникально.
                            uniqueWord = false;

                            break;
                        }
                    }

                    // Проверяем не было ли это слово помещено в список неуникальных.
                    if (nonUniqueWords.Contains(word) == true)
                    {
                        // Слово не уникально.
                        uniqueWord = false;
                    }

                    // Если слово уникально - добавляем его в словарь данного клиента.
                    if (uniqueWord == true && word.Length != 1)
                    {
                        current.keywords.Add(word);
                    }
                }
            }
        }

        /// <summary>
        /// Указывает, что клиент не найден.
        /// </summary>
        public const Customer NotFound = null;

        /// <summary>
        /// Возвращает число записей в таблице.
        /// </summary>
        /// <returns>Число записей в таблице.</returns>
        public int Count => customers.Rows.Count;


        /// <summary>
        /// Поиск клиента по УНП
        /// </summary>
        /// <param name="unpCustomer">УНП клиента.</param>
        /// <returns>Экземпляр класса Customer для данного клиента, если поиск успешен, иначе ListCustomers.NotFound = null</returns>
        public Customer FindCustomerByUNP(string unpCustomer)
        {
            foreach (Customer customer in this)
            {
                string unp = customer.UNP;

                if (unp.Length != 0 && unp == unpCustomer)
                {
                    return customer;
                }
            }

            return NotFound;
        }


        /// <summary>
        /// Поиск клиента по названию.
        /// </summary>
        /// <param name="nameCustomer">Название клиента.</param>
        /// <returns>Экземпляр класса Customer для данного клиента, если поиск успешен, иначе ListCustomers.NotFound = null</returns>
        public Customer FindCustomerByName(string nameCustomer)
        {
            foreach (Customer customer in this)
            {
                if (customer.ThisCustomer(nameCustomer) == true)
                {
                    return customer;
                }
            }

            return NotFound;
        }

        /// <summary>
        /// Поиск клиента по УНП и названию.
        /// </summary>
        /// <param name="nameCustomer">Название клиента.</param>
        /// <param name="unpCustomer">УНП клиента.</param>
        /// <returns>Экземпляр класса Customer для данного клиента, если поиск успешен, иначе ListCustomers.NotFound = null</returns>
        public Customer FindCustomer(string nameCustomer, string unpCustomer)
        {
            Customer customer = FindCustomerByUNP(unpCustomer);

            if (customer == NotFound)
            {
                customer = FindCustomerByName(nameCustomer);

                if (customer == NotFound)
                {
                    return NotFound;
                }
                
                if (customer.UNP.Length == 0)
                {
                    // В записи данного клиента не было УНП - добавляем.
                    customer.UNP = unpCustomer;
                }
            }

            return customer;
        }

        /// <summary>
        /// Возвращает объект типа Customer.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private Customer GetCustomers(DataRow row)
        {
            Customer customer = new Customer(row);

            if (keywords.TryGetValue(customer.Id, out List<string> listKeywords) == false)
            {
                listKeywords = new List<string>();

                keywords[customer.Id] = listKeywords;
            }

            customer.keywords = listKeywords;

            return customer;
        }

        public Customer this[DataRow row]
        {
            get
            {
                return GetCustomers(row);
            }
        }

        public Customer this[int i]
        {
            get
            {
                return GetCustomers(customers.Rows[i]);
            }
        }

        /// <summary>
        /// Возвращает клиента по id.
        /// </summary>
        /// <param name="id">Id клиента.</param>
        /// <returns>Экземпляр клиента или null - если не найден.</returns>
        public Customer GetCustomerAtId(int id)
        {
            foreach (Customer current in this)
            {
                if (current.Id == id)
                {
                    return current;
                }
            }

            return NotFound;
        }

        public IEnumerator<Customer> GetEnumerator()
        {
            foreach (DataRow row in customers.Rows)
            {
                yield return this[row];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
