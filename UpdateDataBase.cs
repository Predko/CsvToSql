using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using log4net;
using System.Globalization;
using System.Data.Odbc;
using System.Linq;
using System.Windows.Documents;
using System.Threading;

namespace CSVtoSQL
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Обработчик кнопки запуска создания sql скрипта
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnUpdateDataBase_Click(object sender, RoutedEventArgs e)
        {
            if (waterMark.WaterMarkTextBoxIsEmpty(tbFilesCSV) == true)
            {
                MessageForEmptyTextBox messageBox = new MessageForEmptyTextBox(tbFilesCSV);

                messageBox.Show("Укажите файлы выписки");

                return;
            }

            ConvertDataFromCSVToReportDataTable(FileNamesCSV.ToArray());
            
            UpdateDataBase();
        }

        #region Имеющиеся таблицы и их поля.
        /*        Имеющиеся таблицы и их поля.
                TABLE "Expenses"(
                 "Id" INTEGER NOT NULL IDENTITY PRIMARY KEY,
                 "Date" DATE,
                 "Summ" DECIMAL(15, 2),
                 "Number" INTEGER,
                 "Comment" VARCHAR(10))

                 TABLE "Clients"(
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
                 "ClientId" INTEGER,
                 "Date" DATE,
                 "Number" INTEGER,
                 "Summ" DECIMAL(15, 2),
                 "Prepayment" DECIMAL(15, 2),
                 "Available" BOOLEAN,
                 "File" VARCHAR(255),
                 "Comment" VARCHAR(20),

                 TABLE "Income"(
                 "Id" INTEGER NOT NULL IDENTITY PRIMARY KEY,
                 "ClientId" INTEGER,
                 "Summ" DECIMAL(15, 2),
                 "Date" DATE,
                 "Number" INTEGER,
                 "TypePaiment" BOOLEAN,

                 INSERT INTO "Clients"("Id","NameCompany","Account","City","Account1","Region","PhoneNumber","Fax","Mail","File","UNP")
                 INSERT INTO "Expenses"("Id","Date","Summ","Number","Comment")
                 INSERT INTO "Contracts"("Id","ClientId","Date","Number","Summ","Prepayment","Available","File","Comment")
                 INSERT INTO "Income"("Id","ClientId","Summ","Date","Number","TypePaiment")

                 Колонки таблицы Report:
                 (Id ClientId BankCode CorrAccount Number Debit Credit Equivalent Date Purpose NameCompany UNP)*/
        #endregion

        /// <summary>
        /// Создаёт SQL скрипт из данных банковской выписки для обновления базы данных.
        /// </summary>
        private void UpdateDataBase()
        {
            static bool DataTableIsEmpty(DataTable dt) => dt.Rows.Count == 0;

            #region Проверка, указаны ли имена файлов и готова ли таблица данных для формирования sql скрипта

            DataTable report = storage["report"];

            if (DataTableIsEmpty(report))
            {
                MessageBox.Show("Введите имя файла XML для чтения выписки.");

                return;
            }
            #endregion

            DataTable reportChanges = storage["report"].GetChanges();

            // Проверка на наличие изменений данных
            if (DataTableIsEmpty(reportChanges))
            {
                MessageBox.Show("Таблица данных выписок не содержит изменений");

                return;
            }

            #region Сортировка по дате
            List<DataRow> rows = new List<DataRow>();

            foreach (DataRow dr in reportChanges.Rows)
            {
                rows.Add(dr);
            }

            rows.Sort((dr1, dr2) => ((DateTime)dr1["Date"]).CompareTo((DateTime)dr2["Date"]));
            #endregion

            try
            {
                using StreamWriter streamWriter = new StreamWriter("1.sql"); //"TbSqlScriptFile.Text);

                string sqlHeader = $"USE \"{TbNameDataBase.Text}\"\n\n" +
                                   "GO\n\n";

                #region Строка создания таблицы выписок
                string sqlCreateTable = "IF OBJECT_ID('Report','U') IS NOT NULL\n" +
                                    "BEGIN\n" +
                                       "\tDROP TABLE \"Report\"\n" +
                                    "END\n\n" +
                                     "CREATE TABLE \"Report\"\n(\n" +
                                     "\"Id\" INTEGER NOT NULL IDENTITY PRIMARY KEY,\n" +
                                    "\"ClientId\" INTEGER,\n" +
                                    "\"BankCode\" VARCHAR(11),\n" +
                                    "\"CorrAccount\" VARCHAR(28),\n" +
                                    "\"Number\" VARCHAR(10),\n" +
                                    "\"Debit\" DECIMAL(15, 2),\n" +
                                    "\"Credit\" DECIMAL(15, 2),\n" +
                                    "\"Equivalent\" DECIMAL(15, 2),\n" +
                                    "\"date\" DATE,\n" +
                                    "\"Purpose\" VARCHAR(500),\n" +
                                    "\"NameCompany\" VARCHAR(100),\n" +
                                    "\"UNP\" VARCHAR(9)\n);\n\nGO\n\n";
                #endregion

                #region Строка вставки данных выписок в таблицу
                string sqlInsertStringHeader = "INSERT INTO \"Report\"(\"ClientId\",\"BankCode\",\"CorrAccount\",\"Number\",\"Debit\"," +
                    "\"Credit\",\"Equivalent\",\"Date\",\"Purpose\",\"NameCompany\",\"UNP\")\n" +
                                               "VALUES";
                #endregion

                #region Строка шаблона вставки записей
                string sqlInsertString = "({0}," +
                                         "'{1}','{2}','{3}'," +
                                         "{4},{5},{6},'{7:yyyy'-'MM'-'dd}'," +
                                         "'{8}'," +
                                         "'{9}'," +
                                         "'{10}')";
                #endregion

                #region Изменение формата записи сумм
                NumberFormatInfo numberFormatInfo = CultureInfo.CurrentCulture.NumberFormat;

                numberFormatInfo = (NumberFormatInfo)numberFormatInfo.Clone();

                numberFormatInfo.NumberDecimalSeparator = ".";
                #endregion

                #region Запись заголовка скрипта - создание таблицы
                streamWriter.WriteLine(sqlHeader);

                Paragraph paragraph = new Paragraph();

                FdMakeSqlScriptText.Blocks.Add(paragraph);

                paragraph.Inlines.Add(new Run(sqlHeader));

                streamWriter.WriteLine(sqlCreateTable);
                paragraph.Inlines.Add(new Run(sqlCreateTable));

                streamWriter.WriteLine(sqlInsertStringHeader);
                paragraph.Inlines.Add(new Run(sqlInsertStringHeader));
                #endregion

                string preString = "";

                bool isFirstString = true;

                // Запись данных выписки в скрипт
                foreach (DataRow dr in rows)
                {
                    string sqlString = string.Format(numberFormatInfo, preString + sqlInsertString, dr[0],
                                                                          dr[1], dr[2], dr[3],
                                                                          dr[4], dr[5], dr[6], dr[7],
                                                                          dr[8],
                                                                          dr[9],
                                                                          dr[10]);

                    streamWriter.Write(sqlString);

                    paragraph.Inlines.Add(new Run(sqlString));

                    if (isFirstString == true)
                    {
                        preString = ",\n";

                        isFirstString = false;
                    }
                }

                #region Запись скрипта обновления базы данных
                streamWriter.Write(";\n\nGO\n\n");
                paragraph.Inlines.Add(new Run(";\n"));

                UpdateDataBase(streamWriter,paragraph);
                #endregion

                MessageBox.Show("Скрипт успешно создан.");
            }
            catch (Exception ex)
            {
                MessageAndLogException("Не удалось преобразовать или записать файл SQL", ex);
            }
        }

        /// <summary>
        /// Скрипт обновления базы данных и записи в указанный параграф
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="paragraph"></param>
        private void UpdateDataBase(StreamWriter sw, Paragraph paragraph)
        {
            UpdateClientsTable(sw, paragraph);

            InsertIntoIncomeTable(sw, paragraph);

            InsertIntoExpensesTable(sw, paragraph);
        }

        /// <summary>
        /// Скрипт обновления таблицы Clients и записи в указанный параграф
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="paragraph"></param>
        private void UpdateClientsTable(StreamWriter sw, Paragraph paragraph)
        {
            string s = "UPDATE Clients\n" +
                        "SET UNP = Report.UNP\n" +
                        "FROM Report\n" +
                        "WHERE Report.UNP IS NOT NULL AND Clients.Id= Report.ClientID\n\n" +
                        "GO\n\n";

            sw.WriteLine(s);
            paragraph.Inlines.Add(new Run(s));
        }

        /// <summary>
        /// Скрипт обновления таблицы Income и записи в указанный параграф
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="paragraph"></param>
        private void InsertIntoIncomeTable(StreamWriter sw, Paragraph paragraph)
        {
            string s =
@"IF OBJECT_ID('newTable', 'U') IS NOT NULL
 BEGIN
    DROP TABLE newTable
END

IF OBJECT_ID('IncomeIds', 'U') IS NOT NULL
BEGIN
    DROP TABLE IncomeIds
END

DECLARE @LastDate DATE
DECLARE @FirstDate DATE

SELECT @LastDate = MAX(Income.[Date])
FROM Income

SELECT @FirstDate = MIN(Report.[date])
FROM Report

CREATE TABLE newTable(
ReportId INT
)

--
-- Insert into Income table
--
IF @FirstDate <= @LastDate
BEGIN
    SELECT ClientId,Number,Summ
    INTO IncomeIds
    FROM Income
    WHERE [Date] BETWEEN @FirstDate AND @LastDate

    INSERT INTO newTable(ReportId)
    SELECT Report.Id
    FROM Report
    WHERE(SELECT Id
        FROM IncomeIds
        WHERE Report.ClientId = ClientId
            AND Report.Number = Number
            AND Report.Credit = Summ) IS NOT NULL
END

INSERT INTO Income(ClientId, Summ, [Date], Number)
SELECT ClientId, Credit,[date],Number
FROM  Report
WHERE Report.Credit != 0 AND
    (SELECT ReportId
    FROM newTable
    WHERE Report.Id = ReportId) IS NULL

";
            
            sw.WriteLine(s);
            paragraph.Inlines.Add(new Run(s));
        }

        /// <summary>
        /// Скрипт обновления таблицы Expenses и записи в указанный параграф
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="paragraph"></param>
        private void InsertIntoExpensesTable(StreamWriter sw, Paragraph paragraph)
        {
            string s =
@"--
-- Insert into Expenses table
--
SELECT @LastDate = MAX(Expenses.[Date])
FROM Expenses

DELETE newTable

IF @FirstDate <= @LastDate
BEGIN
    SELECT Number,Summ,[Date]
    INTO ExpensesData
    FROM Expenses
    WHERE [Date] BETWEEN @FirstDate AND @LastDate

    INSERT INTO newTable(ReportId)
    SELECT Report.Id
    FROM Report
    WHERE(SELECT Id
        FROM ExpensesData
        WHERE Report.ClientId = ClientId
            AND Report.Number = Number
            AND Report.Credit = Summ
			AND Report.[date] = [Date]) IS NOT NULL
END

INSERT INTO Expenses(Summ, [Date], Number)
SELECT Debit,[date],Number
FROM  Report
WHERE  Report.Debit != 0 AND
	(SELECT ReportId
    FROM newTable
    WHERE Report.Id = ReportId) IS NULL

GO

DROP TABLE newTable
IF OBJECT_ID('IncomeIds', 'U') IS NOT NULL
BEGIN
    DROP TABLE IncomeIds
END
";

            sw.WriteLine(s);
            paragraph.Inlines.Add(new Run(s));
        }

    }
}
