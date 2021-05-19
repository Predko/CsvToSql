using System.Data;

namespace CSVtoDataBase
{
    public interface IStorageDatabase
    {
        DataTable this[string name] { get; }

        string ConnectionString { get; set; }

        void AcceptChanges(string nameTable);
        void Add(DataTable dt);
        int AsynchronousDataBaseUpdate(string nameTable, string queryString = null);
        void DataBaseUpdate(string nameTable, string queryString = null);
        void LoadDataTable(string nameTable, string queryString = null);
    }
}