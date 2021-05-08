using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CSVtoDataBase
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Saves the state of the status bar
        /// </summary>
        struct BackupProgressBar
        {
            public string progressText;

            public double progressBarValue;
        }

        /// <summary>
        /// Current tab tag
        /// </summary>
        private string currentKey;

        /// <summary>
        /// Saves the states of all tabs.
        /// </summary>
        private Dictionary<string, BackupProgressBar> buckupProgressBar = new Dictionary<string, BackupProgressBar>();

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            DataTable dt;

            TabItem item = (sender as TabControl).SelectedItem as TabItem;

            ChangeStatusPanelContent((string)item.Tag);

            switch (item.Tag)
            {
                case "ReportReading":

                    break;

                case "Income":

                    dt = storage[incomeTable];
                    if (dt != null)
                    {
                        DgIncome.ItemsSource = dt.DefaultView;
                    }
                    else
                    {
                        DgIncome.ItemsSource = null;
                    }

                    break;

                case "Expenses":

                    dt = storage[expensesTable];

                    if (dt != null)
                    {
                        DgExpenses.ItemsSource = dt.DefaultView;
                    }
                    else
                    {
                        DgExpenses.ItemsSource = null;
                    }

                    break;

                case "Customers":

                    if (listCustomers != null && listCustomers.Count != 0)
                    {
                        DgCustomers.ItemsSource = listCustomers;
                    }
                    else
                    {
                        DgCustomers.ItemsSource = null;
                    }

                    break;

                case "Reports":

                    dt = storage["report"];

                    if (dt != null)
                    {
                        DgReport.ItemsSource = dt.DefaultView;
                    }
                    else
                    {
                        DgReport.ItemsSource = null;
                    }

                    break;
            }
        }

        /// <summary>
        /// Changes the content of the status bar when the tab changes
        /// </summary>
        /// <param name="key">New tab tag.</param>
        private void ChangeStatusPanelContent(string key)
        {
            BackupProgressBar bpb = new BackupProgressBar()
            {
                progressBarValue = PbProgress.Value,

                progressText = TblProgressText.Text
            };

            if (currentKey != null)
            {
                if (buckupProgressBar.ContainsKey(currentKey))
                {
                    buckupProgressBar[currentKey] = bpb;
                }
                else
                {
                    buckupProgressBar.Add(currentKey, bpb);
                }
            }

            if (buckupProgressBar.ContainsKey(key))
            {
                PbProgress.Value = buckupProgressBar[key].progressBarValue;

                TblProgressText.Text = buckupProgressBar[key].progressText;
            }
            else
            {
                PbProgress.Value = PbProgress.Minimum;

                TblProgressText.Text = "";
            }

            currentKey = key;
        }

        //private void GReports_Loaded(object sender, RoutedEventArgs e)
        //{
        //    GReports.Loaded += new RoutedEventHandler(SetMinWidths);
        //}

        private void DgReport_Loaded(object sender, RoutedEventArgs e)
        {
            SetMinWidths(sender as DataGrid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dg"></param>
        public static void SetMinWidths(DataGrid dg)
        {
            foreach (var column in dg.Columns)
            {
                column.MinWidth = column.ActualWidth;
                //column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }

        private void BtnReadingReports_Click(object sender, RoutedEventArgs e)
        {
            if (waterMark.WaterMarkTextBoxIsEmpty(tbFilesCSV) == true)
            {
                MessageForEmptyTextBox messageBox = new MessageForEmptyTextBox(tbFilesCSV);

                messageBox.Show("Укажите файлы выписки");

                return;
            }

            PbProgress.Value = 0;

            if (ConvertDataFromCSVToReportDataTable(FileNamesCSV.ToArray()) == false)
            {
                return;
            }

            BtnUpdateDataBase.IsEnabled = true;
        }
    }
}
