using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CSVtoDataBase
{
    class StorageDataBase
    {
        private readonly DataSet dataSet;

        private string connectionString;
        /// <summary>
        /// Строка подключения.
        /// </summary>
        public string ConnectionString { get => connectionString; set => connectionString = value; }

        public StorageDataBase(string connectionString)
        {
            ConnectionString = connectionString;

            dataSet = new DataSet();
        }

        public void Add(DataTable dt) => dataSet.Tables.Add(dt);

        /// <summary>
        /// Возвращает таблицу с именем name.
        /// </summary>
        /// <param name="name">Имя таблицы.</param>
        /// <returns>Таблица DataTable.</returns>
        public DataTable this[string name]
        {
            get
            {
                return dataSet.Tables[name];
            }
        }

        /// <summary>
        /// Обновляет базу данных из соответствующей таблицы данных.
        /// </summary>
        /// <param name="nameTable">Имя таблицы данных.</param>
        public async void UpdateDataBaseAsync(string nameTable)
        {
            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);

            SqlDataAdapter dataAdapter = new SqlDataAdapter($"SELECT * FROM {nameTable}", sqlConnection);

            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

            string s = commandBuilder.GetInsertCommand().CommandText;

            await Task.Run(() => dataAdapter.Update(dataSet.Tables[nameTable]));
        }

        /// <summary>
        /// Ассинхронно загружает указанную таблицу из базы данных.
        /// </summary>
        /// <param name="nameTable">Имя таблицы.</param>
        /// <param name="queryString">Строка sql запроса данных таблицы. 
        /// Если null - будет загружена вся таблица.
        /// </param>
        public void LoadDataTableAsync(string nameTable, string queryString = null)
        {
            PrepareForLoadDataTable(nameTable, queryString,
                        async (dataAdapter, nameTable) =>
                                { await Task.Run(() => dataAdapter.Fill(dataSet.Tables[nameTable])); });
        }

        /// <summary>
        /// Загружает указанную таблицу из базы данных.
        /// </summary>
        /// <param name="nameTable">Имя таблицы.</param>
        /// <param name="queryString">Строка sql запроса данных таблицы. 
        /// Если null - будет загружена вся таблица.
        /// </param>
        public void LoadDataTable(string nameTable, string queryString = null)
        {
            PrepareForLoadDataTable(nameTable, queryString,
                        (dataAdapter, nameTable) => dataAdapter.Fill(dataSet.Tables[nameTable]));
        }

        private void PrepareForLoadDataTable(string nameTable, string queryString, Action<SqlDataAdapter, string> load)
        {
            string newQueryString = queryString;

            if (newQueryString == null)
            {
                newQueryString = $"SELECT * FROM {nameTable}";
            }
            SqlConnection sqlConnection = new SqlConnection(ConnectionString);

            SqlDataAdapter dataAdapter = new SqlDataAdapter(newQueryString, sqlConnection);

            if (dataSet.Tables.Contains(nameTable) == true)
            {
                dataSet.Tables[nameTable].Clear();
            }
            else
            {
                dataSet.Tables.Add(nameTable);
            }

            load(dataAdapter, nameTable);
        }
    }
}
