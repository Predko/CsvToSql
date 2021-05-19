using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace CSVtoDataBase
{
    public partial class ListOfChangesViewModel : INotifyPropertyChanged
    {
        #region Имеющиеся таблицы и их поля.
        /*        Имеющиеся таблицы и их поля.
                TABLE "Expenses"(
                 "Id" INTEGER NOT NULL IDENTITY PRIMARY KEY,
                 "Date" DATE,
                 "Value" DECIMAL(15, 2),
                 "Number" INTEGER,
                 "Comment" VARCHAR(10))

                 TABLE "Customers"(
                 "Id" INTEGER NOT NULL IDENTITY PRIMARY KEY,
                 "NameCompany" VARCHAR(100),
                 "Account" VARCHAR(28), "city" VARCHAR(50),
                 "Account1" VARCHAR(28),
                 "region" VARCHAR(20),
                 "PhoneNumber" VARCHAR(30),
                 "Fax" VARCHAR(30),
                 "Mail" VARCHAR(50),
                 "File" VARCHAR(255),
                 "UNP" VARCHAR(10))

                 TABLE "Contracts"(
                 "Id" INTEGER NOT NULL IDENTITY PRIMARY KEY,
                 "CustomerId" INTEGER,
                 "Date" DATE,
                 "Number" INTEGER,
                 "Price" DECIMAL(15, 2),
                 "Prepayment" DECIMAL(15, 2),
                 "Available" BOOLEAN,
                 "File" VARCHAR(255),
                 "Comment" VARCHAR(20),

                 TABLE "Income"(
                 "Id" INTEGER NOT NULL IDENTITY PRIMARY KEY,
                 "CustomerId" INTEGER,
                 "Value" DECIMAL(15, 2),
                 "Date" DATE,
                 "Number" INTEGER,
                 "TypePaiment" BOOLEAN,

                 INSERT INTO "Customers"("Id","NameCompany","Account","City","Account1","Region","PhoneNumber","Fax","Mail","File","UNP")
                 INSERT INTO "Expenses"("Id","Date","Value","Number","Comment")
                 INSERT INTO "Contracts"("Id","CustomerId","Date","Number","Value","Prepayment","Available","File","Comment")
                 INSERT INTO "Income"("Id","CustomerId","Value","Date","Number","TypePaiment")

                 Колонки таблицы Report:
                 (Id CustomerId BankCode CorrAccount Number Debit Credit Equivalent Date Purpose NameCompany UNP)*/
        #endregion

        /// <summary>
        /// Подготавливает данные для обновления таблиц базы данных.
        /// </summary>
        private bool PrepareToUpdateDatabase()
        {
            static bool DataTableIsEmpty(DataTable dt) => (dt == null) || dt.Rows.Count == 0;

            #region Проверка, указаны ли имена файлов и готова ли таблица данных для обновления базы данных.

            DataTable report = storage[reportsTable];

            if (DataTableIsEmpty(report))
            {
                return false;
            }

            DataTable reportChanges = storage[reportsTable].GetChanges();

            // Проверка на наличие изменений данных
            if (DataTableIsEmpty(reportChanges))
            {
                MessageBox.Show("Таблица данных выписок не содержит изменений");

                return false;
            }
            #endregion

            #region Сортировка таблицы по дате.
            DataView dv = reportChanges.DefaultView;

            dv.Sort = "Date";

            reportChanges = dv.ToTable();
            #endregion

            try
            {
                #region Определяем начальную дату выписки для Debit и Credit.

                DateTime beginDateExpenses = (DateTime)reportChanges.Rows[0]["Date"];
                DateTime beginDateIncome = beginDateExpenses;

                storage.LoadDataTable(incomeTable, $"SELECT TOP 1 * FROM [{incomeTable}] ORDER BY [Date] DESC");

                storage.LoadDataTable(expensesTable, $"SELECT TOP 1 * FROM [{expensesTable}] ORDER BY [Date] DESC");

                if (storage[incomeTable].Rows.Count != 0)
                {
                    beginDateIncome = (DateTime)storage[incomeTable].Rows[0]["Date"];
                }

                if (storage[expensesTable].Rows.Count != 0)
                {
                    beginDateExpenses = (DateTime)storage[expensesTable].Rows[0]["Date"];
                }

                #endregion

                // Строки запроса для загрузки и обновления базы данных.
                string queryExpensesFromDate = $"SELECT * FROM [{expensesTable}] WHERE [Date] >= '{beginDateExpenses}' ORDER BY [Date] DESC";
                string queryIncomeFromDate = $"SELECT * FROM [{incomeTable}] WHERE [Date] >= '{beginDateIncome}' ORDER BY [Date] DESC";

                #region Загружаем данные из таблиц начиная с соответствующих начальных дат.

                storage.LoadDataTable(expensesTable, queryExpensesFromDate);

                storage.LoadDataTable(incomeTable, queryIncomeFromDate);

                storage.AcceptChanges(expensesTable);

                storage.AcceptChanges(incomeTable);
                #endregion

                #region Запись данных выписки в таблицы с проверкой на повтор.

                // Число обновлённых записей бызы данных
                int numberOfEntriesAdded = 0;

                ProgressMaximum = reportChanges.Rows.Count;
                ProgressValue = 0;

                int numberCurrentRecord = 0;

                for (int i = 0; i != reportChanges.Rows.Count; i++)
                {
                    ProgressValue = numberCurrentRecord++;

                    DataRow currentRow = reportChanges.Rows[i];

                    if ((decimal)currentRow["Debit"] != 0)
                    {
                        if ((DateTime)currentRow["Date"] < beginDateExpenses)
                        {
                            continue;
                        }

                        // Проверяем, нет ли такой записи в базе данных.
                        if (IsContainsOperation(expensesTable,
                                                IgnoreId,
                                                (int)currentRow["Number"],
                                                (DateTime)currentRow["Date"]) == true)
                        {
                            continue;
                        }

                        DataRow newRow = storage[expensesTable].NewRow();

                        // [Id], [Date],    [Value],      [Number], [Comment]
                        // int,   Date,   decimal(15.2),   int,     nvarchar[10]
                        newRow.ItemArray = new object[] { null, currentRow["Date"], currentRow["Debit"], currentRow["Number"], null };

                        storage[expensesTable].Rows.Add(newRow);
                    }
                    else
                    if ((decimal)currentRow["Credit"] != 0)
                    {
                        if ((DateTime)currentRow["Date"] < beginDateIncome)
                        {
                            continue;
                        }

                        // Проверяем, нет ли такой записи в базе данных.
                        if (IsContainsOperation(incomeTable,
                                                (int)currentRow["CustomerId"],
                                                (int)currentRow["Number"],
                                                (DateTime)currentRow["Date"]) == true)
                        {
                            continue;
                        }

                        DataRow newRow = storage[incomeTable].NewRow();

                        // [Id], [CustomerId],    [Value],    [Date], [Number], [TypePayment]
                        // int,    int,     decimal(15.2),  Date,    int,       bit
                        newRow.ItemArray = new object[] { null, currentRow["CustomerId"], currentRow["Credit"], currentRow["Date"], currentRow["Number"], true };

                        storage[incomeTable].Rows.Add(newRow);
                    }

                    numberOfEntriesAdded++;
                }
                #endregion
            }
            catch (Exception ex)
            {
                StatusText += "Не удалось обновить таблицы данных.";

                MessageAndLogException("Не удалось обновить таблицы данных.", ex);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Обновляет базу данных из подготовленных данных.
        /// </summary>
        /// <returns></returns>
        private bool UpdateDatabase()
        {
            DateTime beginDateExpenses = (DateTime)storage[expensesTable].Rows[0]["Date"];
            DateTime beginDateIncome = (DateTime)storage[incomeTable].Rows[0]["Date"];

            // Строки запроса для загрузки и обновления базы данных.
            string queryExpensesFromDate = $"SELECT * FROM [{expensesTable}] WHERE [Date] >= '{beginDateExpenses}' ORDER BY [Date] DESC";
            string queryIncomeFromDate = $"SELECT * FROM [{incomeTable}] WHERE [Date] >= '{beginDateIncome}' ORDER BY [Date] DESC";

            int numberOfEntriesAdded;

            try
            {
                // Обновляем базу данных
                numberOfEntriesAdded = storage.AsynchronousDataBaseUpdate(expensesTable, queryExpensesFromDate);

                numberOfEntriesAdded += storage.AsynchronousDataBaseUpdate(incomeTable, queryIncomeFromDate);
            }
            catch (Exception ex)
            {
                StatusText += "Не удалось обновить базу данных.";

                MessageAndLogException("Не удалось обновить базу данных.", ex);

                return false;
            }

            StatusText += "\t" + $"Добавлено {numberOfEntriesAdded} записей в базу данных.";

            MessageBox.Show((numberOfEntriesAdded == 0)
                                ? "База данных не изменена."
                                : "База данных обновлена.\n" + $"Добавлено {numberOfEntriesAdded} записей в базу данных.");

            return true;
        }

        /// <summary>
        /// Значение параметра idCustomer функции IsContainsOperation, при котором его следует игнорировать.
        /// </summary>
        private const int IgnoreId = -1;

        /// <summary>
        /// Проверяет наличие записи в таблице по Id клиента, номеру операции и дате операции.
        /// </summary>
        /// <param name="nameTable">Имя таблицы.</param>
        /// <param name="idCustomer">Id клиента. -1 - если не надо учитывать.</param>
        /// <param name="numberOperation">Номер операции(платёжного поручения).</param>
        /// <param name="dateOperation">Дата операции.</param>
        /// <returns>True - если такая операция найдена иначе false</returns>
        bool IsContainsOperation(string nameTable, int idCustomer, int numberOperation, DateTime dateOperation)
        {
            foreach (DataRow dr in storage[nameTable].Rows)
            {
                if (idCustomer != IgnoreId)
                {
                    if ((int)dr["CustomerId"] != idCustomer)
                    {
                        continue;
                    }
                }

                // У одного клиента не может быть более одной операции с одним номером и датой.
                if ((int)dr["Number"] == numberOperation && (DateTime)dr["Date"] == dateOperation)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
