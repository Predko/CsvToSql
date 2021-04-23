using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CSVtoSQL
{
    class StorageDataBase
    {
        private SqlConnection sqlConnection;

        private DataSet dataSet;

        private string connectionString;
        /// <summary>
        /// Строка подключения.
        /// </summary>
        public string ConnectionString { get => connectionString; set => connectionString = value; }

        public string ClientsString;

        public StorageDataBase(string connectionString)
        {
            ConnectionString = connectionString;

            sqlConnection = new SqlConnection(ConnectionString);

            ClientsString = "SELECT Id, NameCompany, UNP FROM Clients";

            dataSet = new DataSet();

            SqlDataAdapter dataAdapter = new SqlDataAdapter(ClientsString, sqlConnection);

            DataTable dt = new DataTable("Clients");

            dataAdapter.Fill(dt);

            dataSet.Tables.Add(dt);
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

    }
}
