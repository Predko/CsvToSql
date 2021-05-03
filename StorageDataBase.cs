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
        /// <param name="queryString">Строка запроса для загрузки базы данных или её части.</param>
        public void DataBaseUpdate(string nameTable, string queryString = null)
        {
            if (queryString == null)
            {
                queryString = $"SELECT * FROM {nameTable}";
            }

            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);

            SqlDataAdapter dataAdapter = new SqlDataAdapter(queryString, sqlConnection);

            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

            string s = commandBuilder.GetInsertCommand().CommandText;

            dataAdapter.Update(dataSet.Tables[nameTable]);
        }

        /// <summary>
        /// Асинхронно обновляет базу данных из соответствующей таблицы данных.
        /// </summary>
        /// <param name="nameTable">Имя таблицы данных.</param>
        /// <param name="queryString">Строка запроса для загрузки базы данных или её части.</param>
        public void AsynchronousDataBaseUpdate(string nameTable, string queryString = null)
        {
            Task updateTask = new Task(() =>
                        {
                            if (queryString == null)
                            {
                                queryString = $"SELECT * FROM {nameTable}";
                            }

                            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);

                            SqlDataAdapter dataAdapter = new SqlDataAdapter(queryString, sqlConnection);

                            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                            string s = commandBuilder.GetInsertCommand().CommandText;
                            dataAdapter.Update(dataSet.Tables[nameTable]);
                        });

            updateTask.Start();
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

            dataAdapter.Fill(dataSet.Tables[nameTable]);
        }
    }
}
