using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

namespace CSVtoSQL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// Программа предназначена для преобразования банковской выписки интернет-банкинга Белагропромбанка в формате CSV
    /// В SQL скрипт, заносящий новые данные в базу данных.
    /// Идентификация записи в выписке(чтобы избежать дубликатов) выполняется по "Номеру документа", УНП и дате операции.
    /// Также программа определяет Id плательщика.
    /// Для этого подготавливается текстовый файл с данными клиентов в формате:
    /// Id\tUNP\tNameCompany
    /// Колонким разделены одинарной табуляцией.
    /// Данные о клиентах хранятся в отдельной таблице Clients
    /// Перед чтением файлов выписок, таблица Clients должна быть заполнена.
    /// Для упрощения доступа к данным клиентов, используется класс ListClients
    /// Данные выписки хранятся в таблице Report.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ListClients listClients;

        private readonly DataSet report;

        /// <summary>
        /// Путь к рабочему каталогу.
        /// </summary>
        string FilePath;

        /// <summary>
        /// Имя файла с данными клиентов.
        /// </summary>
        string ClientsFileName;

        public MainWindow()
        {
            InitializeComponent();

            InitializeTextBoxWaterMark();
            
            ClientsFileName = InitFileName();

            report = new DataSet("Report");

            InitDataTables();

            listClients = new ListClients(report.Tables["clients"], ClientsFileName);

            if (listClients.Load() == false)
            {
                // Не удалось загрузить файл с данными о клиентах - делаем кнопку чтения выписок неактивной.
                TBlkBtnConvertCSVtoXMLToolTip.IsEnabled = false;
            }

            ChangeBtnConvertCSVtoXMLToolTip();
        }

        /// <summary>
        /// Читает аргументы из командной строки и инициализирует переменные FilePath
        /// </summary>
        /// <returns></returns>
        private string InitFileName()
        {
            string fname = null, filePath = null;

            if (App.Args.Length != 0)
            {
                fname = App.Args[0].Trim();

                if (fname.Length != 0)
                {
                    string s = fname.ToLower().Trim('\\', '/', '-');

                    if (s[0] == '?' || s == "help" || s == "h")
                    {
                        MessageBox.Show("Использование:\nCSVtoSQL.exe [fileClientsInfo]\n" +
                            "Где: [fileClientsInfo.txt] - не обязательное имя файла содержащего записи с данными о клиентах\n" +
                            "в формате:\n Id<Tab>УНП<Tab>Название организации[<Tab>1]- признак бюджетной организации.\n" +
                            "Колонки разделены одинарными табуляциями.\n" +
                            "Подключить файл с данными о клиентах можно в самой программе.");

                        fname = null;
                    }
                    else
                    {
                        filePath = fname.Substring(0, fname.LastIndexOf('\\') + 1);


                        if (File.Exists(fname) == false)
                        {
                            fname = null;
                            
                            if (Directory.Exists(filePath) == false)
                            {
                                filePath = null;
                            }
                        }
                    }
                }
            }

            if (filePath == null)
            {
                FilePath = Directory.GetCurrentDirectory();
            }

            if (fname == null)
            {
                fname = $"{FilePath}\\CsvToXml_clients.xml";
            }

            return fname;
        }

        /// <summary>
        /// Инициализация таблиц для хранения данных.
        /// </summary>
        private void InitDataTables()
        {
            DataTable dt = new DataTable("clients");
            
            {
                dt.Columns.Add(new DataColumn("Id", Type.GetType("System.Int32")) { Unique = true });

                dt.Columns.Add(new DataColumn("UNP", Type.GetType("System.String")));

                dt.Columns.Add(new DataColumn("NameCompany", Type.GetType("System.String")));

                dt.Columns.Add(new DataColumn("Multiple", Type.GetType("System.Boolean")));

                dt.PrimaryKey = new DataColumn[] { dt.Columns["Id"] };

                report.Tables.Add(dt);
            }

            dt = new DataTable("income");

            {
                dt.Columns.Add(new DataColumn("ClientId", Type.GetType("System.Int32")));

                dt.Columns.Add(new DataColumn("BankCode", Type.GetType("System.String")));

                dt.Columns.Add(new DataColumn("CorrAccount", Type.GetType("System.String")));

                dt.Columns.Add(new DataColumn("Number", Type.GetType("System.String")));

                dt.Columns.Add(new DataColumn("Debit", Type.GetType("System.Decimal")));

                dt.Columns.Add(new DataColumn("Credit", Type.GetType("System.Decimal")));

                dt.Columns.Add(new DataColumn("Equivalent", Type.GetType("System.Decimal")));

                dt.Columns.Add(new DataColumn("Date", Type.GetType("System.DateTime")));

                dt.Columns.Add(new DataColumn("Purpose", Type.GetType("System.String")));

                dt.Columns.Add(new DataColumn("NameCompany", Type.GetType("System.String")));

                dt.Columns.Add(new DataColumn("UNP", Type.GetType("System.String")));

                report.Tables.Add(dt);
            }
        }
    }
}
