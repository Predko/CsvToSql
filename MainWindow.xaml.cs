﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
using System.Configuration;
using log4net;

using Microsoft.Win32;

namespace CSVtoSQL
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
    /// Данные о клиентах хранятся в отдельной таблице Clients
    /// Перед чтением файлов выписок, таблица Clients должна быть заполнена.
    /// Для упрощения доступа к данным клиентов, используется класс ListClients
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
        private readonly ListClients listClients;

        private readonly DataSet reportsInfo;

        /// <summary>
        /// Путь к рабочему каталогу.
        /// </summary>
        string FilePath;

        /// <summary>
        /// Имя файла с данными клиентов.
        /// </summary>
        string ClientsFileName;

        private static readonly ILog log = LogManager.GetLogger(typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();

            InitializeTextBoxWaterMark();

            ClientsFileName = InitFileName();

            reportsInfo = new DataSet("ReportsInfo");

            InitDataTables();

            listClients = new ListClients(reportsInfo.Tables["clients"], ClientsFileName);

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

                reportsInfo.Tables.Add(dt);
            }

            dt = new DataTable("report");

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

                reportsInfo.Tables.Add(dt);
            }
        }
    }
}
