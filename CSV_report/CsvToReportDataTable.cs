using CSVtoDataBase.CSV_report;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;

namespace CSVtoDataBase
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Читает текстовые данные из файлов банковской выписки
        /// интернет-банкинга Белапб в формате CSV из filesIn
        /// и записывает в DataTable
        /// В процессе считывания информации происходит идентификация клиентов по УНП и названию.
        /// Для выписки с УНП бюджета, предлагается выбрать клиента из списка.
        /// Если в данных клиента нет номера УНП и название организации не удаётся найти - предлагается выбор из списка.
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
        /// <returns>True - если чтение выписок успешно, иначе - false.</returns>
        private bool ConvertDataFromCSVToReportDataTable(string[] filesIn)
        {
            const int BankCode = 0;    // Код банка
            const int Account = 1;     // Счет-корреспондент
            const int Number = 2;      // Номер документа
            const int Debit = 3;       // Обороты: дебет
            const int Credit = 4;      // Обороты: кредит
            const int Equivalent = 5;  // В эквиваленте
            const int Date = 6;        // Дата операции
            const int Purpose = 7;     // Назначение
            const int Name = 8;        // Наименование контрагента
            const int UNP = 9;         // УНП контрагента

            DataTable dtReports = storage["report"];

            storage.LoadDataTable("Customers");

            dtReports.Clear();

            dtReports.AcceptChanges();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var srcEncoding = Encoding.GetEncoding(1251);

            int totalRecords = 0;

            decimal debit = 0, credit = 0;

            PbProgress.Maximum = filesIn.Length - 1;

            int i = 0;

            foreach (var file in filesIn)
            {
                TblProgressText.Text = $"Converting file: {file}";

                PbProgress.Value = i++;

                try
                {
                    using StreamReader sr = new StreamReader(file, encoding: srcEncoding);

                    string line;

                    while ((line = sr.ReadLine()) != null && !line.Contains("Код банка")) ;

                    if (line == null)
                    {
                        TblProgressText.Text = $"Convertation {file} failed.";

                        continue;
                    }

                    int numberRows = 0;

                    while ((line = sr.ReadLine()) != null && !line.Contains("Итого оборотов"))
                    {
                        string[] s = line.Split(';');

                        DataRow dr = dtReports.NewRow();

                        decimal d1 = string.IsNullOrEmpty(s[Debit]) ? 0 : decimal.Parse(s[Debit]);
                        decimal d2 = string.IsNullOrEmpty(s[Credit]) ? 0 : decimal.Parse(s[Credit]);
                        decimal d3 = string.IsNullOrEmpty(s[Equivalent]) ? 0 : decimal.Parse(s[Equivalent]);

                        int idCustomer = 0;

                        if (listCustomers.Count != 0)
                        {
                            Customer customer = listCustomers.Find(s[Name], s[UNP]);

                            if (customer == ListCustomers.NotFound)
                            {
                                // Клиент не найден, предлагаем пользователю выбрать клиента из списка.
                                WindowChoosingCustomerName choosingCustomerName = new WindowChoosingCustomerName(listCustomers);

                                choosingCustomerName.DescriptionOfPurpose.Text = $"УНП: {s[UNP]}, Номер ПП: {s[Number].Trim('\"', '=')} " +
                                                $"Дата: {s[Date]} Сумма: {s[Equivalent]}\n" + s[Purpose];

                                if (choosingCustomerName.ShowDialog() == false || choosingCustomerName.SelectedCustomer == null)
                                {
                                    MessageBox.Show("Операция чтения выписки отменена.");

                                    return false;
                                }

                                customer = choosingCustomerName.SelectedCustomer;

                                if (customer.UNP.Length == 0)
                                {
                                    customer.UNP = s[UNP];
                                }
                            }

                            idCustomer = customer.Id;
                        }

                        ///    0 Код банка; 1 Счет-корреспондент; 2 Номер документа;
                        ///    3 Обороты: дебет; 4 Обороты: кредит; 5 В эквиваленте; 6 Дата операции;
                        ///    7 Назначение; 8 Наименование контрагента; 9 УНП контрагента;
                        dr.ItemArray = new object[] { idCustomer, s[BankCode], s[Account].Trim('\"', '='), int.Parse(s[Number].Trim('\"', '=')),
                                                  d1, d2, d3, DateTime.Parse(s[Date]).Date,
                                                  s[Purpose], s[Name], s[UNP]
                                                };

                        dtReports.Rows.Add(dr);

                        TblProgressText.Text += $".";

                        numberRows++;

                        debit += d1;

                        credit += d2;
                    }

                    TblProgressText.Text = $"Done. {numberRows} records.";

                    totalRecords += numberRows;
                }
                catch (Exception ex)
                {
                    MessageAndLogException($"Конвертация файла {file} не удалась.\nОперация прервана", ex);

                    return false;
                }
            }

            TblProgressText.Text = $"Total {totalRecords} records. Debit = {debit}, Credit = {credit}";

            PbProgress.Value = i;

            return true;
        }

        /// <summary>
        /// Вывод сообщения и запись в лог файл.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        private void MessageAndLogException(string msg, Exception ex)
        {
            MessageBox.Show(msg);

            log.Warn(msg + "\n" + ex.ToString());
        }
    }
}
