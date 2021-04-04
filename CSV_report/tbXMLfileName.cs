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

namespace CSVtoSQL
{
    public partial class MainWindow : Window
    {
        private const string emptyXMLfileName = "Введите имя файла XML для записи выписки...";

        private const string emptyReportsfileName = "Введите имена файлов выписок...";

        private const string emptySqlScriptfileName = "Введите имя файла Sql скрипта...";

        /// <summary>
        /// Словарь, хранящий строки "водяных знаков" текстовых полей и признак множественного выбора файлов.
        /// </summary>
        private Dictionary<TextBox, string> emptyLinesOfTextBoxs = new Dictionary<TextBox, string>();

        /// <summary>
        /// XML файл для формируемой выписки
        /// </summary>
        public string XmlReportfileName { get; set; } = "";

        private void InitializeTextBoxWaterMark()
        {
            
            SetWaterMarkString(TbXMLfileName, emptyXMLfileName);

            SetWaterMarkString(tbFilesCSV, emptyReportsfileName);

            SetWaterMarkString(tbSqlScriptFile, emptySqlScriptfileName);
        }
        
        public void SetWaterMarkString(TextBox tb, string es)
        {
            tb.Foreground = Brushes.Gray;

            tb.Text = es;

            emptyLinesOfTextBoxs.Add(tb, es);
        }

        public string GetWaterMarkString(TextBox tb) => emptyLinesOfTextBoxs[tb];

        public bool IsWaterMarkTextBoxEmpty(TextBox tb) => (emptyLinesOfTextBoxs[tb] == tb.Text);
        
        #region События для обработки водяного знака
        public void TbWm_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
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

        public void TbWm_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
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
        #endregion

        /// <summary>
        /// Создаёт диалоговое окно выбора/создания файла
        /// Путь и имя файла записывает в контент вызвавшего TextBox/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TbWm_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string[] files = GetNameFiles();

            if (sender is TextBox tb)
            {
                if (files == null)
                {
                    tb.Text = "";

                    return;
                }

                //string fs = files[0];

                //FilePath = fs.Substring(0, fs.LastIndexOf('\\') + 1);

                tb.Text = "";

                foreach (var s in files)
                {
                    tb.Text += s + "\n";
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
                InitialDirectory = Directory.GetCurrentDirectory(),
                Multiselect = multiselect
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileNames;
            }

            return null;
        }

        /// <summary>
        /// Создаёт диалоговое окно выбора/создания файла
        /// Путь и имя файла записывает в контент вызвавшего TextBox/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TbWmXMLfileName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "XML|*.xml|All files *.*|*.*",
                InitialDirectory = Directory.GetCurrentDirectory()
            };

            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }
            
            string file = saveFileDialog.FileName;

            if (file.ToLower().Contains(".xml") == false)
            {
                file += ".xml";
            }

            XmlReportfileName = file;

            if (sender is TextBox tb)
            {
                FilePath = file.Substring(0, file.LastIndexOf('\\') + 1);

                tb.Text = file;
            }
        }

    }
}
