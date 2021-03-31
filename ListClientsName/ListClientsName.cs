using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using WpfAppConvertation;
using System.Linq;
using System.Collections;

namespace WpfAppConvertation
{
    /// <summary>
    /// Класс - хранилище списка клиентов.
    /// У каждого клиента есть Id и одно или несколько наименований
    /// Наименования клиента можно добавлять и удалять по Id или наименованию. 
    /// Удаление последнего наименования клиента приводит к удалению клиента из списка вместе с Id
    /// Можно заполнять и сохранять список из файла или потока.
    /// Можно искать клиента в списке.
    /// </summary>
    public class ListClientsName: IEnumerable<KeyValuePair<int, string>>
    {
        /// <summary>
        /// Словарь с данными о клиентах в формате:
        /// - ID клиента - int,
        /// - Список наименований клиента в виде строки.
        /// </summary>
        private readonly Dictionary<int, List<string>> listClients;

        public string FileName { get; set; }

        public ListClientsName(string file = null)
        {
            listClients = new Dictionary<int, List<string>>();

            FileName = file;
        }

        public const int NotFound = -1;

        /// <summary>
        /// Возвращает число записей в списке.
        /// </summary>
        /// <returns>Число записей в списке.</returns>
        public int Count() => listClients.Count;
        
        /// <summary>
        /// Заполняет список информацией о клиентах из DataTable с удалением имеющейся информации
        /// </summary>
        /// <param name="dt"></param>
        public void Set(DataTable dt)
        {
            Clear();

            Add(dt);
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
        public void Add(DataRow row)
        {
            List<string> ls = new List<string>();

            for (int i = 1; i != row.ItemArray.Length; i++)
            {
                ls.Add((string)row.ItemArray[i]);
            }

            listClients.Add((int)row[0], ls);
        }

        /// <summary>
        /// Добавляет наименование клиента с данным id.
        /// </summary>
        /// <param name="id">Id клиента</param>
        /// <param name="name">Наименование</param>
        public void Add(int id, string name)
        {
            int i = Find(name);

            if (i == id)
            {
                // Данное наименование уже есть.
                return;
            }

            listClients[id].Add(name);
        }

        /// <summary>
        /// Полностью очищает список клиентов.
        /// </summary>
        public void Clear() => listClients.Clear();

        /// <summary>
        /// Поиск имени клиента.
        /// </summary>
        /// <param name="client"></param>
        /// <returns>ID клиента, если поиск успешен, иначе ListClientsName.NotFound = -1</returns>
        public int Find(string client)
        {
            foreach (KeyValuePair<int, List<string>> keyValuePair in listClients)
            {
                if (keyValuePair.Value.Contains(client))
                {
                    return keyValuePair.Key;
                }
            }

            return NotFound;
        }

        /// <summary>
        /// Удаляет указанное имя клиента. Если имя последнее, удаляется вся запись о клиенте(вместе с Id) 
        /// </summary>
        /// <param name="nameClient"></param>
        /// <returns></returns>
        public bool Remove(string nameClient)
        {
            int id = Find(nameClient);

            if (id == NotFound)
            {
                return false;
            }

            // Последняя запись.
            if (listClients[id].Count == 1)
            {
                return Remove(id);
            }

            return listClients[id].Remove(nameClient);
        }

        /// <summary>
        /// Удаляет все записи о клиенте.
        /// </summary>
        /// <param name="id">Id удаляемого клиента</param>
        /// <returns></returns>
        public bool Remove(int id)
        {
            return listClients.Remove(id);
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

            using StreamReader streamReader = new StreamReader(file);

            return Load(streamReader);
        }

        /// <summary>
        /// Загружает список клиентов из указанного XML файла.
        /// </summary>
        /// <param name="streamReader">StreamReader XML файла.</param>
        /// <returns>True - если данные успешно загружены.</returns>
        public bool Load(StreamReader streamReader)
        {
            if (streamReader == null)
            {
                return false;
            }

            DataTable dt = new DataTable();

            dt.ReadXml(streamReader);

            Clear();

            Add(dt);

            return true;
        }

        /// <summary>
        /// Сохраняет список клиентов в файле с указанным именем
        /// Если имя не указано или равно null, сохранение происходит в файл из переменной FileName;
        /// </summary>
        /// <param name="file">Имя файла для сохранения списка.</param>
        public void Save(string file = null)
        {
            if (file == null)
            {
                file = FileName;
            }

            if (File.Exists(file) == false)
            {
                return;
            }

            using StreamWriter sw = new StreamWriter(file);

            Save(sw);
        }

        /// <summary>
        /// Сохраняет список клиентов в указанном потоке
        /// </summary>
        /// <param name="sw">Объект типа StreamWriter</param>
        public void Save(StreamWriter sw)
        {
            DataTable dtClients = new DataTable("Clients");

            // Подсчёт максимального количества имён.
            int maxCount = (from kvp in listClients
                            select kvp.Value.Count).Max(c => c);


            // Формируем колонки.
            dtClients.Columns.Add(new DataColumn("Id", Type.GetType("System.Int32")));

            for (int i = 0; i != maxCount; i++)
            {
                dtClients.Columns.Add(new DataColumn($"Name_{i}", Type.GetType("System.String")));
            }

            foreach (KeyValuePair<int, List<string>> keyValuePair in listClients)
            {
                DataRow dr = dtClients.NewRow();

                dr[0] = keyValuePair.Key;

                int i = 1;

                foreach (string s in keyValuePair.Value)
                {
                    dr[i++] = s;
                }

                dtClients.Rows.Add(dr);
            }

            dtClients.WriteXml(sw, XmlWriteMode.WriteSchema);
        }

        public IEnumerator<KeyValuePair<int,string>> GetEnumerator()
        {
            foreach (var kvp in listClients)
            {
                yield return new KeyValuePair<int, string>(kvp.Key, kvp.Value[0]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
