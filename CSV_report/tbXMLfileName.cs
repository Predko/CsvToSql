using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfAppConvertation
{
    public partial class MainWindow : Window
    {
        private const string emptyXMLfileName = "Введите имя файла XML для записи выписки...";

        private const string emptyReportsfileName = "Введите имена файлов выписок...";

        private const string emptySqlScriptfileName = "Введите имя файла для формирования Sql скрипта...";

        /// <summary>
        /// Словарь, хранящий строки "водяных знаков" текстовых полей и признак множественного выбора файлов.
        /// </summary>
        private Dictionary<TextBox, string> emptyLinesOfTextBoxs = new Dictionary<TextBox, string>();

        private string xMLfileName = "";

        public string XMLfileName
        {
            get => xMLfileName;
            set 
            { 
                xMLfileName = value; 
                TbXMLfileName.Text = value; 
            }
        }

        private void InitializeTextBoxXMLfileName()
        {
            SetWaterMarkString(TbXMLfileName, emptyXMLfileName);

            SetWaterMarkString(tbFileCSV, emptyReportsfileName);

            SetWaterMarkString(tbSqlScriptFile, emptySqlScriptfileName);
        }
        
        public void SetWaterMarkString(TextBox tb, string es)
        {
            tb.Foreground = Brushes.Gray;

            tb.Text = es;

            emptyLinesOfTextBoxs.Add(tb, es);
        }

        public string GetWaterMarkString(TextBox tb) => emptyLinesOfTextBoxs[tb];

        public void TbWaterMark_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!(sender is TextBox tb))
            {
                return;
            }

            //If nothing has been entered yet.
            if (tb.Foreground == Brushes.Gray)
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
        }

        public void TbWaterMark_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!(sender is TextBox tb))
            {
                return;
            }

            //If nothing was entered, reset default text.
            if (tb.Text.Trim().Length == 0)
            {
                tb.Foreground = Brushes.Gray;

                tb.Text = GetWaterMarkString(tb);
            }
        }


        /// <summary>
        /// Создаёт диалоговое окно выбора/создания файла
        /// Путь и имя файла записывает в контент вызвавшего TextBox/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TbWaterMark_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string[] files = GetNameFiles();

            if (files == null)
            {
                return;
            }

            if (sender is TextBox tb)
            {
                string fs = files[0];

                FilePath = fs.Substring(0, fs.LastIndexOf('\\') + 1);

                FileNames = files;
                tb.Text = "";

                foreach (var s in FileNames)
                {
                    tb.Text += s + " ";
                }
            }
        }

        /// <summary>
        /// Создаёт диалоговое окно выбора файла/файлов и возвращает массив выбранных файлов.
        /// </summary>
        /// <param name="multiselect">Признак множественного(true) или одиночного(false) выбора</param>
        /// <returns></returns>
        private string[] GetNameFiles(bool multiselect = true)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "All files *.*|*.*|CSV|*.csv|TXT|*.txt|XML|*.xml",
                Multiselect = multiselect
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileNames;
            }

            return null;
        }
    }
}
