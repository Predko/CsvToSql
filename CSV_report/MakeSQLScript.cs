using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CSVtoSQL
{
    public partial class MainWindow : Window
    {

        private void BtnMakeSQLScript_Click(object sender, RoutedEventArgs e)
        {
            MakeSQLScript();
        }

        private void MakeSQLScript()
        {
            if (IsWaterMarkTextBoxEmpty(tbSqlScriptFile) == true)
            {
                MessageForEmptyTextBox messageBox = new MessageForEmptyTextBox(tbSqlScriptFile);

                messageBox.Show("Укажите конечный файл для SQL скрипта");

                return;
            }

        }

    }
}
