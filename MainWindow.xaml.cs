using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();

            InitializeTextBoxWaterMark();

            DataContext = new ListOfChangesViewModel();
        }
    }
}
