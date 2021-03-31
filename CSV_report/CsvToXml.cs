using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfAppConvertation.CSV_report;
using WpfAppConvertation.ClientsFileName;

namespace WpfAppConvertation
{
    public partial class MainWindow : Window
    {
        string[] FileNames;
        string FilePath;
        string ClientsFileName;


        /// <summary>
        /// Создаёт диалоговое окно для выбора текстового файла с информацией о клиентах и конечного XML файла
        /// Возвращает имена файлов или признак отмены операции. 
        /// </summary>
        /// <param name="fileIn">Текстовый файл с информацией о клиентах.</param>
        /// <param name="clientsName">Формируемый XML файл </param>
        /// <returns>true - если файлы выбраны, false - отмена операции.</returns>
        private bool? OpenDialogMakeClientsNameFile(out string fileIn, out string clientsName)
        {
            WindowMakeClientsNameFile wndMakeClientsNameFile = new WindowMakeClientsNameFile();

            wndMakeClientsNameFile.Owner = this;

            wndMakeClientsNameFile.InitializeWaterMark(this);

            bool? res = wndMakeClientsNameFile.ShowDialog();

            fileIn = wndMakeClientsNameFile.OriginalFile;

            clientsName = wndMakeClientsNameFile.XmlClientsNameFile;

            return res;
        }

        private void ButtonConvertFile_Click(object sender, RoutedEventArgs e)
        {
            if (OpenDialogMakeClientsNameFile(out string fileIn, out ClientsFileName) == false)
            {
                return;
            }

            ConvertIdNameInfoToXML(fileIn, ClientsFileName);
        }

        private void BtnOpenClientsNameFile_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Преобразует текстовые данные (ID NameCompany) из файла fileIn
        /// в XML формат и записывает в файл fileOut
        /// </summary>
        /// <param name="fileIn"></param>
        /// <param name="fileOut"></param>
        private void ConvertIdNameInfoToXML(string fileIn, string fileOut)
        {
            DataTable dtIdName = new DataTable("Clients");

            dtIdName.Columns.Add(new DataColumn("id", Type.GetType("System.Int32")));

            DataColumn column = new DataColumn("nameCompany", Type.GetType("System.String"));

            dtIdName.Columns.Add(column);

            using (StreamReader sr = new StreamReader(fileIn))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] s = line.Split('\t');

                    if (s[0] == "")
                    {
                        break;
                    }

                    int id = int.Parse(s[0]);

                    DataRow dr = dtIdName.NewRow();

                    dr.ItemArray = new object[] { id, s[1] };

                    dtIdName.Rows.Add(dr);
                }
            }

            using StreamWriter sw = new StreamWriter(fileOut);

            listClients.Set(dtIdName);

            listClients.Save(sw);
        }

        private DataTable Income { get; set; }

        /// <summary>
        /// Читает данные из файлов банковских выписок(имена файлов хранятся в переменной FileNames)
        /// и записывает их в таблицу.
        /// Данные из таблицы сохраняются в XML файле.
        /// В процессе считывания информации добавляются имена клиентов в listClients.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetNameCompanyFromFileCSV_Click(object sender, RoutedEventArgs e)
        {
            Income = ConvertDataFromCSVToXML(FileNames);

            using StreamWriter sw = new StreamWriter(FilePath + "report.xml");

            Income.WriteXml(sw);

            listClients.Save();
        }

        /// <summary>
        /// Читает текстовые данные из файлов банковской выписки интернет банкинга Белапб в формате CSV из filesIn
        /// и записывает в DataTable
        /// Структура файла:
        /// - Разделитель ';'.
        /// - Предварительные данные(не используются) - от начала файла до строки начинающейся с текста "Код банка".
        /// - Все записи построчно.
        /// - Заголовки записей:
        ///    Код банка; Счет-корреспондент; Номер документа;
        ///    Обороты: дебет; Обороты: кредит; В эквиваленте; Дата операции;
        ///    Назначение; Наименование контрагента; УНП контрагента;
        /// </summary>
        /// <param name="filesIn">Массив строк с именами файлов</param>
        /// <returns>Таблица DataTable с данными выписок.</returns>
        private DataTable ConvertDataFromCSVToXML(string[] filesIn)
        {
            if (filesIn == null)
            {
                MessageBox.Show($"No input files specified");

                return null;
            }

            DataTable dtIncome = new DataTable("Income");

            dtIncome.Columns.Add(new DataColumn("ClientId", Type.GetType("System.Int32")));

            dtIncome.Columns.Add(new DataColumn("BankCode", Type.GetType("System.String")));

            dtIncome.Columns.Add(new DataColumn("CorrAccount", Type.GetType("System.String")));

            dtIncome.Columns.Add(new DataColumn("Number", Type.GetType("System.String")));

            dtIncome.Columns.Add(new DataColumn("Debit", Type.GetType("System.Decimal")));

            dtIncome.Columns.Add(new DataColumn("Credit", Type.GetType("System.Decimal")));

            dtIncome.Columns.Add(new DataColumn("Equivalent", Type.GetType("System.Decimal")));

            dtIncome.Columns.Add(new DataColumn("Date", Type.GetType("System.DateTime")));

            dtIncome.Columns.Add(new DataColumn("Purpose", Type.GetType("System.String")));

            dtIncome.Columns.Add(new DataColumn("NameCompany", Type.GetType("System.String")));

            dtIncome.Columns.Add(new DataColumn("UNP", Type.GetType("System.String")));

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var srcEncoding = Encoding.GetEncoding(1251);

            int totalRecords = 0;

            decimal debit = 0, credit = 0;

            foreach (var file in filesIn)
            {
                tbMessages.Text += $"\nConverting file\n{file}\n";

                using StreamReader sr = new StreamReader(file, encoding: srcEncoding);

                string line;

                while ((line = sr.ReadLine()) != null && !line.Contains("Код банка")) ;

                if (line == null)
                {
                    tbMessages.Text += $"\nConvertation {file} failed.";

                    continue;
                }

                int numberRows = 0;

                while ((line = sr.ReadLine()) != null && !line.Contains("Итого оборотов"))
                {
                    string[] s = line.Split(';');

                    DataRow dr = dtIncome.NewRow();

                    decimal d1 = string.IsNullOrEmpty(s[3]) ? 0 : decimal.Parse(s[3]);
                    decimal d2 = string.IsNullOrEmpty(s[4]) ? 0 : decimal.Parse(s[4]);
                    decimal d3 = string.IsNullOrEmpty(s[5]) ? 0 : decimal.Parse(s[5]);

                    int id = 0;

                    if (listClients.Count() != 0)
                    {
                        id = listClients.Find(s[8]);

                        if (id == ListClientsName.NotFound)
                        {
                            WindowChoosingClientName choosingClientName = new WindowChoosingClientName(listClients);

                            choosingClientName.DescriptionOfPurpose.Text = s[7];

                            if (choosingClientName.ShowDialog() == false)
                            {
                                return null;
                            }

                            listClients.Add(id, s[8]);
                        }
                    }

                    ///    0 Код банка; 1 Счет-корреспондент; 2 Номер документа;
                    ///    3 Обороты: дебет; 4 Обороты: кредит; 5 В эквиваленте; 6 Дата операции;
                    ///    7 Назначение; 8 Наименование контрагента; 9 УНП контрагента;
                    dr.ItemArray = new object[] { id, s[0], s[1].Trim('\"', '='), s[2].Trim('\"', '='),
                                                  d1, d2, d3, DateTime.Parse(s[6]).Date,
                                                  s[7], s[8], int.Parse(s[9])
                                                };

                    dtIncome.Rows.Add(dr);

                    tbMessages.Text += $".";

                    numberRows++;

                    debit += d1;

                    credit += d2;
                }

                tbMessages.Text += $"\nDone. {numberRows} records.";

                totalRecords += numberRows;
            }

            tbMessages.Text += $"\nTotal {totalRecords} records.\nDebit = {debit}, Credit = {credit}";

            return dtIncome;
        }
    }
}
