using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using CSVtoSQL;
using System.Linq;
using System.Collections;
using log4net;
using System.Data.SqlClient;

namespace CSVtoSQL
{
    /// <summary>
    /// Класс - обёртка строки DataRow, содержащей данные о клиенте:
    ///  Id - клиента,
    ///  UNP - УНП клиента,
    ///  Name - название,
    ///  Multiple - признак того, что несколько клиентов может соответствовать этому УНП.
    /// </summary>
    public class Client : IComparable<Client>, IComparable
    {
        private readonly DataRow clientRow;

        public int Id { get => (int)clientRow[0]; set => clientRow[0] = value; }

        public string Name
        {
            get
            {
                if (clientRow[1] is string name)
                {
                    return name;
                }

                return "";
            }

            set => clientRow[1] = value ?? "";
        }

        public string UNP
        {
            get
            {
                if (clientRow[2] is string unp)
                {
                    return unp;
                }

                return "";
            }

            set => clientRow[2] = value ?? "";
        }

        public static string GetName(DataRow dr)
        {
            if (dr[1] is string name)
            {
                return name;
            }

            return "";
        }

        public static string GetUNP(DataRow dr)
        {
            if (dr[2] is string unp)
            {
                return unp;
            }

            return "";
        }

        public Client(DataRow dr) => clientRow = dr;

        public Client() {}

        override public string ToString()
        {
            string sUNP = (UNP.Length <= 1) ? "         " : UNP;

            return $"{sUNP} {Name}";
        }

        #region IComparable<Client>

        public int CompareTo(object obj)
        {
            if (obj is Client c)
            {
                return Name.CompareTo(((Client)obj).Name);
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

            if (obj is Client c)
            {
                return Name == c.Name;
            }

            return (object)this == obj;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        int IComparable<Client>.CompareTo(Client other)
        {
            return Name.CompareTo(other.Name);
        }

        public static bool operator ==(Client left, Client right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(Client left, Client right)
        {
            return !(left == right);
        }

        public static bool operator <(Client left, Client right)
        {
            return (left is null) ? (right is object) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(Client left, Client right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(Client left, Client right)
        {
            return left is object && left.CompareTo(right) > 0;
        }

        public static bool operator >=(Client left, Client right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
        #endregion IComparable<Client>
    }

    /// <summary>
    /// Класс - обеспечивающий доступ к списку клиентов.
    /// У каждого клиента есть Id, УНП и наименование
    /// Можно заполнять и сохранять список из файла или потока.
    /// Можно искать клиента в таблице.
    /// </summary>
    public class ListClients: IEnumerable<Client>
    {
        /// <summary>
        /// Таблица с данными о клиентах в формате:
        /// - ID клиента - int,
        /// - UNP клиента - string,
        /// - Name клиента - string.
        /// </summary>
        /// 
        private readonly DataTable clients;

        /// <summary>
        /// УНП бюджетных организаций.
        /// </summary>
        public string UnpBudget;

        public string FileName { get; set; }

        private static readonly ILog log = LogManager.GetLogger(typeof(ListClients));

        public ListClients(DataTable dtClients, string file = null)
        {
            clients = dtClients;

            FileName = file;
        }

        public const Client NotFound = null;

        /// <summary>
        /// Возвращает число записей в таблице.
        /// </summary>
        /// <returns>Число записей в таблице.</returns>
        public int Count => clients.Rows.Count;

        /// <summary>
        /// Заполняет список информацией о клиентах из DataTable с удалением имеющейся информации
        /// </summary>
        /// <param name="dt"></param>
        public void Set(DataTable dt)
        {
            Clear();

            Add(dt);

            clients.AcceptChanges();
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
        public void Add(DataRow row) => Add(row[0], Client.GetName(row), Client.GetUNP(row));

        /// <summary>
        /// Добавляет в список данные об одном клиенте.
        /// </summary>
        /// <param name="client">Объект типа Client</param>
        public void Add(Client client) => Add(client.Id, client.Name, client.UNP);

        /// <summary>
        /// Добавляет наименование клиента с данным id.
        /// </summary>
        /// <param name="id">Id клиента</param>
        /// <param name="name">Наименование</param>
        /// <param name="unp">УНП</param>
        public void Add(object id, object name, object unp)
        {
            DataRow res = clients.Rows.Find(id);

            // Если такой записи нет, добавляем.
            if (res == null)
            {
                DataRow row = clients.NewRow();

                row.ItemArray = new object[] { id, name, unp };

                clients.Rows.Add(row);

                return;
            }

            // Такая запись есть, переписываем её значения.
            res[1] = name;
            res[2] = unp;
        }

        /// <summary>
        /// Полностью очищает список клиентов.
        /// </summary>
        private void Clear() => clients.Clear();

        /// <summary>
        /// Поиск клиента по УНП и названию.
        /// </summary>
        /// <param name="nameClient"></param>
        /// <param name="unpClient"></param>
        /// <returns>Экземпляр класса Client для данного клиента, если поиск успешен, иначе ListClients.NotFound = null</returns>
        public Client Find(string nameClient, string unpClient)
        {
            if (unpClient == UnpBudget) //(string)clients.Rows[i][2]
            {
                // Этот УНП не может идентифицировать плательщика.
                return NotFound;
            }
                
            for(int i = 0; i != clients.Rows.Count; i++)
            {
                string name = Client.GetName(clients.Rows[i]);
                string unp = Client.GetUNP(clients.Rows[i]);

                if (unp.Length == 0)
                {
                    if (name == nameClient)
                    {
                        // В записи данного клиента не было УНП - добавляем.
                        clients.Rows[i][1] = unpClient;
                        
                        return new Client(clients.Rows[i]);
                    }
                }
                else
                if (unp == unpClient)
                {
                    return new Client(clients.Rows[i]);
                }
            }

            return NotFound;
        }

        /// <summary>
        /// Загружает список клиентов из файла FileName.
        /// </summary>
        /// <returns>True - если данные успешно загружены.</returns>
        public bool Load() => Load(FileName);

        /// <summary>
        /// Загружает список клиентов из указанного XML файла.
        /// </summary>
        /// <param name="file">XML файл.</param>
        /// <returns>True - если данные успешно загружены.</returns>
        public bool Load(string file)
        {
            if (file?.Length == 0)
            {
                return false;
            }

            if (File.Exists(file) == false)
            {
                return false;
            }

            FileName = file;

            try
            {
                using StreamReader streamReader = new StreamReader(file);

                bool res = Load(streamReader);

                return res;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка операций с файлом {file} .");

                log.Warn(ex.ToString());
            }

            return false;
        }

        /// <summary>
        /// Загружает список клиентов из указанного потока.
        /// </summary>
        /// <param name="streamReader">StreamReader XML файла.</param>
        /// <returns>True - если данные успешно загружены.</returns>
        public bool Load(StreamReader streamReader)
        {
            if (streamReader == null)
            {
                return false;
            }

            // Проверяем, соответствует ли схема данных читаемого файла.
            using (DataTable dt = new DataTable())
            {
                dt.ReadXmlSchema(streamReader);

                if (dt.TableName == clients.TableName 
                    && dt.Columns.Count == clients.Columns.Count)
                {
                    for (int i = 0; i != dt.Columns.Count; i++)
                    {
                        if (dt.Columns[i].ColumnName != clients.Columns[i].ColumnName)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }

                streamReader.BaseStream.Position = 0;

                streamReader.DiscardBufferedData();
            }
            
            clients.Clear();

            try
            {
                clients.ReadXml(streamReader);
            }
            catch(Exception ex)
            {
                string s = $"Не удалось загрузить таблицу данных из XML файла.";
                MessageBox.Show(s);

                log.Warn(s + "\n" + ex.ToString());
            }

            clients.AcceptChanges();

            return true;
        }

        /// <summary>
        /// Сохраняет список клиентов в файле с указанным именем
        /// Если имя не указано или равно null, сохранение происходит в файл из переменной FileName;
        /// </summary>
        /// <param name="file">Имя файла для сохранения списка.</param>
        public void Save(string file = null)
        {
            if (file == null || file.Length == 0)
            {
                file = FileName;
            }

            try
            {
                using StreamWriter sw = new StreamWriter(file);

                Save(sw);
            }
            catch (Exception ex)
            {
                string s = $"Не сохранить таблицу данных в XML файл.";
                MessageBox.Show(s);

                log.Warn(s + "\n" + ex.ToString());
            }
        }

        /// <summary>
        /// Сохраняет список клиентов в указанном потоке
        /// </summary>
        /// <param name="sw">Объект типа StreamWriter</param>
        public void Save(StreamWriter sw) => clients.WriteXml(sw, XmlWriteMode.WriteSchema);

        public IEnumerator<Client> GetEnumerator()
        {
            foreach (DataRow row in clients.Rows)
            {
                yield return new Client(row);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
