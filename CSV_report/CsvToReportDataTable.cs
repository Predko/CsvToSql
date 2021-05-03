using CSVtoDataBase.CSV_report;
using System;
using System.Collections.ObjectModel;
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

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var srcEncoding = Encoding.GetEncoding(1251);

            int totalRecords = 0;

            decimal debit = 0, credit = 0;

            PbProgress.Maximum = filesIn.Length - 1;

            int i = 0;

            string currentFile = default;

            try
            {
                storage.LoadDataTable(customerNameTable);

                listCustomers = new ListCustomers(storage[customerNameTable]);

                if (listCustomers.Count == 0)
                {
                    MessageBox.Show("Не удалось загрузить данные клиентов.");

                    return false;
                }

                DataTable dtReports = storage["report"];

                dtReports.Clear();

                dtReports.AcceptChanges();

                foreach (var file in filesIn)
                {
                    currentFile = file;

                    TblProgressText.Text = $"Converting file: {file}";

                    PbProgress.Value = i++;
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
                            Customer customer = ListCustomers.NotFound;

                            string sUNP = s[UNP];
                            
                            // Пробуем найти УНП в назначении платежа.
                            string UNPstring = "УНП";

                            int indexUNP = s[Purpose].IndexOf(UNPstring);

                            bool UnpFromPurpose = false;

                            if (indexUNP != -1)
                            {
                                indexUNP = s[Purpose].IndexOfAny("0123456789".ToCharArray(), indexUNP);
                                
                                sUNP = s[Purpose].Substring(indexUNP, 9);

                                // Проверяем, является ли строка УНП.
                                if (int.TryParse(sUNP, out int _) == true)
                                {
                                    UnpFromPurpose = true;

                                    customer = listCustomers.FindCustomer(s[Purpose], sUNP);
                                }
                                else
                                {
                                    sUNP = s[UNP];
                                }
                            }
                            else
                            {
                                customer = listCustomers.FindCustomer(s[Name], sUNP);
                            }

                            if (customer == ListCustomers.NotFound)
                            {
                                ObservableCollection<Customer> listItems = new ObservableCollection<Customer>(listCustomers);

                                //Предложить изменить название организации при выборе.
                                // Клиент не найден, предлагаем пользователю выбрать клиента из списка.
                                WindowChoosingCustomerName choosingCustomerName = new WindowChoosingCustomerName(listItems, s[Name].Trim());

                                choosingCustomerName.DescriptionOfPurpose.Text = $"УНП: {sUNP}, {s[Name].Trim()}, Номер ПП: {s[Number].Trim('\"', '=')} " +
                                                $"Дата: {s[Date]} Сумма: {s[Equivalent]}\n" + s[Purpose];

                                if (choosingCustomerName.ShowDialog() == false || choosingCustomerName.SelectedCustomer == null)
                                {
                                    MessageBox.Show("Операция чтения выписки отменена.");

                                    return false;
                                }

                                customer = choosingCustomerName.SelectedCustomer;

                                if (choosingCustomerName.NeedChangeUNP() == true 
                                    && (customer.UNP.Length == 0 || UnpFromPurpose == true))
                                {
                                    customer.UNP = sUNP;
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

                // Обновляем базу данных клиентов.
                storage.DataBaseUpdate(customerNameTable);
            }
            catch (Exception ex)
            {
                MessageAndLogException($"Конвертация файла {currentFile} не удалась.\nОперация прервана", ex);

                return false;
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
