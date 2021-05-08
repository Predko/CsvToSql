using log4net;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CSVtoDataBase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// Программа предназначена для преобразования банковской выписки интернет-банкинга Белагропромбанка в формате CSV
    /// В SQL скрипт, заносящий новые данные в базу данных.
    /// Также программа определяет Id плательщика.
    /// Для этого подготавливается текстовый файл с данными клиентов в формате:
    /// Id\tUNP\tNameCompany
    /// Колонким разделены одинарной табуляцией.
    /// Данные о клиентах хранятся в отдельной таблице Customers
    /// Перед чтением файлов выписок, таблица Customers должна быть заполнена.
    /// Для упрощения доступа к данным клиентов, используется класс ListCustomers
    /// Данные выписки хранятся в таблице Report.
    /// Последовательность работы:
    /// - Создание файла xml с данными клиентов из текстового файла в формате: Id \t УНП(или пробелы) \t Название.
    /// - Конвертируем банковскую выписку в формате csv в xml файл выписки.
    /// - Из xml файла выписки и данных клиентов(из xml файла с данными клиентов(загружается автоматически, если есть)),
    ///   созаётся sql скрипт, обновляющий данные в базе данных.
    /// Последовательность тестовая.
    /// После тестирования, для работы программы, достаточно файл выписки.
    /// Остальные данные будут получены непосредственно из базы данных и обновление данных будет производиться из программы,
    /// без скрипта.
    /// 
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Имя таблицы клиентов.
        /// </summary>
        private const string customerNameTable = "Customers";

        /// <summary>
        /// Имя таблицы выплат.
        /// </summary>
        private const string expensesTable = "Expenses";

        /// <summary>
        /// Имя таблицы поступлений.
        /// </summary>
        private const string incomeTable = "Income";

        private ListCustomers listCustomers;

        private StorageDataBase storage;

        private static readonly ILog log = LogManager.GetLogger(typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();

            InitializeTextBoxWaterMark();

            InitProgramFromArguments();

            InitStorage();

            InitDataTables();

            DataContext = this;
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
        /// Читает аргументы из командной строки 
        /// и инициализирует список файлов выписки,
        /// если файлы указаны.
        /// </summary>
        /// <returns></returns>
        private void InitProgramFromArguments()
        {
            if (App.Args.Length == 0 || App.Args[0].Trim().Length == 0)
            {
                return;
            }

            string firstArgument = App.Args[0].Trim();

            string s = firstArgument.ToLower().Trim('\\', '/', '-');

            if (s[0] == '?' || s == "help" || s == "h")
            {
                MessageBox.Show("Использование:\nCSVtoSQL.exe [Имя файла выписки.csv] [Имя файла выписки.csv] [Имя файла выписки.csv]...\n" +
                    "Где: [Имя файла выписки.csv] - имя файла банковской выписки.\n" +
                    "Может быть указано несколько файлов.\n" +
                    "Если имя файла содержит пробелы, его надо заключить в ковычки \"Имя файла с пробелами.csv\"\n" +
                    "Если аргументы не указаны, файлы можно выбрать в программе.");

                return;
            }

            // Заполняем список файлов выписки из аргументов коммандной строки.
            foreach (string fileName in App.Args)
            {
                if (File.Exists(fileName) == true)
                {
                    FileNamesCSV.Add(fileName);
                }
            }
        }

        /// <summary>
        /// Инициализация таблиц для хранения данных.
        /// </summary>
        private void InitDataTables()
        {
            DataTable dt = new DataTable("report");

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

            DgReport.ItemsSource = dt.DefaultView;
        }
    }
}
