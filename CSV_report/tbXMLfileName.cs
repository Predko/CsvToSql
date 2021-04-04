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
        private WaterMarkForTextBox waterMark;
        
        private const string emptyXmlFileName = "Введите имя файла XML для записи выписки...";

        private const string emptyReportsFileName = "Введите имена файлов выписок...";

        private const string emptySqlScriptFileName = "Введите имя файла Sql скрипта...";

        /// <summary>
        /// Словарь, хранящий строки "водяных знаков" текстовых полей и признак множественного выбора файлов.
        /// </summary>
        private Dictionary<TextBox, string> emptyLinesOfTextBoxs = new Dictionary<TextBox, string>();

        /// <summary>
        /// XML файл для формируемой выписки
        /// </summary>
        public string XmlReportfileName { get; set; } = "";

        /// <summary>
        /// Список имён файлов выписок. Выбирается пользователем.
        /// </summary>
        private readonly List<string> FileNamesCSV = new List<string>();

        /// <summary>
        /// Имя файла SQL скрипта.
        /// </summary>
        private string SqlScriptFileName;

        private void InitializeTextBoxWaterMark()
        {
            waterMark = new WaterMarkForTextBox();

            waterMark.AddWaterMark(TbXMLfileName, emptyXmlFileName, Brushes.Gray, XmlReportfileName);
            
            waterMark.AddWaterMark(tbFilesCSV, emptyReportsFileName, Brushes.Gray, FileNamesCSV);
            
            waterMark.AddWaterMark(tbSqlScriptFile, emptySqlScriptFileName, Brushes.Gray, XmlReportfileName);
        }

        #region События для обработки водяного знака
        public void TbWm_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
            => waterMark.TbWm_GotKeyboardFocus(sender, e);

        public void TbWm_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
            => waterMark.TbWm_LostKeyboardFocus(sender, e);
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
                if (files == null || files.Length == 0)
                {
                    tb.Text = "";

                    return;
                }

                if (waterMark.IsContains(tb) == true && waterMark.GetObject(tb) is List<string> ls)
                {
                    ls.Clear();

                    foreach( string s in files)
                    {
                        ls.Add(s);
                    }
                }
                
                tb.Text = "";

                int i;
                
                for (i = 0; i != files.Length - 1; i++)
                {
                    tb.Text += files[i] + "\n";
                }

                tb.Text += files[i];
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
