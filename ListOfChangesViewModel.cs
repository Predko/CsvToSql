using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace CSVtoDataBase
{
    public partial class ListOfChangesViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Имя таблицы клиентов.
        /// </summary>
        public static string CustomersTable = "Customers";

        /// <summary>
        /// Имя таблицы выплат.
        /// </summary>
        public static string ExpensesTable = "Expenses";

        /// <summary>
        /// Имя таблицы поступлений.
        /// </summary>
        public static string IncomeTable = "Income";

        private const string reportsTable = "Reports";

        private ListCustomers listCustomers;

        private StorageDataBase storage;

        private int progressMaximum;
        private string progressText;
        private int progressValue;

        public int ProgressMaximum
        {
            get => progressMaximum;

            set
            {
                if (progressMaximum == value)
                {
                    return;
                }

                progressMaximum = value;

                OnPropertyChanged();
            }
        }

        public string ProgressText
        {
            get => progressText;
            set
            {
                if (progressText == value)
                {
                    return;
                }

                progressText = value;

                OnPropertyChanged();
            }
        }

        public int ProgressValue
        {
            get => progressValue;
            set
            {
                if (progressValue == value)
                {
                    return;
                }

                progressValue = value;

                OnPropertyChanged();
            }
        }

        private bool isDatabaseReadyToUpdated = false;

        public bool IsDatabaseReadyToUpdated
        {
            get => isDatabaseReadyToUpdated;

            set
            {
                if (isDatabaseReadyToUpdated != value)
                {
                    isDatabaseReadyToUpdated = value;

                    OnPropertyChanged();
                }
            }
        }

        private bool isDatabaseUpdated = false;

        public bool IsDatabaseUpdated
        {
            get => isDatabaseUpdated;

            set
            {
                if (isDatabaseUpdated != value)
                {
                    isDatabaseUpdated = value;

                    OnPropertyChanged();
                }
            }
        }

        private bool isReportsRead = false;

        public bool IsReportsRead
        {
            get => isReportsRead;

            set
            {
                if (isReportsRead != value)
                {
                    isReportsRead = value;

                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }


        public ListOfChangesViewModel()
        {
            InitStorage();

            InitDataTables();

            ProgressValue = 0;
            ProgressMaximum = 1;
        }

        /// <summary>
        /// Инициализация хранилища данных.
        /// </summary>
        private void InitStorage()
        {
            // Читаем строку подключения к базе данных Customers из файла конфигурации
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            storage = new StorageDataBase(connectionString);

        }

        /// <summary>
        /// Инициализация таблиц для хранения данных.
        /// </summary>
        private void InitDataTables()
        {
            DataTable dt = new DataTable(reportsTable);

            dt.Columns.Add(new DataColumn("CustomerId", Type.GetType("System.Int32")));

            dt.Columns.Add(new DataColumn("BankCode", Type.GetType("System.String")));

            dt.Columns.Add(new DataColumn("CorrAccount", Type.GetType("System.String")));

            dt.Columns.Add(new DataColumn("Number", Type.GetType("System.Int32")));

            dt.Columns.Add(new DataColumn("Debit", Type.GetType("System.Decimal")));

            dt.Columns.Add(new DataColumn("Credit", Type.GetType("System.Decimal")));

            dt.Columns.Add(new DataColumn("Equivalent", Type.GetType("System.Decimal")));

            dt.Columns.Add(new DataColumn("Date", Type.GetType("System.DateTime")));

            dt.Columns.Add(new DataColumn("Purpose", Type.GetType("System.String")));

            dt.Columns.Add(new DataColumn("NameCompany", Type.GetType("System.String")));

            dt.Columns.Add(new DataColumn("UNP", Type.GetType("System.String")));

            storage.Add(dt);
        }

        public DataView ChangesInExpenses
        {
            get
            {
                DataTable dt = storage[ExpensesTable];

                if (dt == null)
                {
                    return null;
                }

                return new DataView(dt, "", "Date", DataViewRowState.Added);
            }
        }

        public DataView ChangesInIncome
        {
            get
            {
                DataTable dt = storage[IncomeTable];

                if (dt == null)
                {
                    return null;
                }

                return new DataView(dt, "", "Date", DataViewRowState.Added);
            }
        }

        public DataView ChangesInCustomers
        {
            get
            {
                DataTable dt = storage[CustomersTable];

                if (dt == null)
                {
                    return null;
                }

                return new DataView(dt, "", "NameCompany", DataViewRowState.Added);
            }
        }

        private OperationCommands readReports;

        public OperationCommands ReadReports => readReports ??= new OperationCommands(
            obj =>
            {
                FileNamesCSV = GetFileNamesToOpen();

                if (FileNamesCSV == null)
                {
                    MessageBox.Show("Файлы не выбраны. Операция отменена.");

                    return;
                }

                if (ConvertDataFromCSVToReportDataTable(FileNamesCSV) == false)
                {
                    return;
                }

                IsDatabaseReadyToUpdated = false;

                IsReportsRead = true;

                OnPropertyChanged(nameof(ChangesInCustomers));

                IsDatabaseReadyToUpdated = PrepareToUpdateDatabase();

                if (IsDatabaseReadyToUpdated == true)
                {
                    OnPropertyChanged(nameof(ChangesInExpenses));

                    OnPropertyChanged(nameof(ChangesInIncome));
                }
            }, null);

        private OperationCommands updateDatabase;

        public OperationCommands UpdateDataBase => updateDatabase ??= new OperationCommands(
            obj =>
            {
                UpdateDatabase();
            }, obj =>
            {
                DataTable dt = storage?[reportsTable];

                return ((IsReportsRead == true) && (IsDatabaseReadyToUpdated == true) && (dt != null && dt.Rows.Count != 0));
            });


        /// <summary>
        /// Создаёт диалоговое окно выбора файла/файлов и возвращает массив выбранных файлов.
        /// </summary>
        /// <param name="multiselect">Признак множественного(true) или одиночного(false) выбора</param>
        /// <returns></returns>
        private string[] GetFileNamesToOpen(bool multiselect = true)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV|*.csv|All files *.*|*.*|TXT|*.txt|XML|*.xml",
                InitialDirectory = Directory.GetCurrentDirectory(),
                Multiselect = multiselect
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileNames;
            }

            return null;
        }
    }
}
