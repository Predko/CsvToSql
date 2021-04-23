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
        /// <summary>
        /// Переменная, хранящая объект, управляющий водяными знаками текстовых полей.
        /// </summary>
        private WaterMarkForTextBox waterMark;

        #region Строки водяных знаков для соответствующих текстовых полей.
        private const string emptyReportsFileName = "Введите имена файлов выписок...";

        #endregion

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
        private readonly StringBuilder SqlScriptFileName = new StringBuilder("");

        private void InitializeTextBoxWaterMark()
        {
            waterMark = new WaterMarkForTextBox();

            waterMark.AddWaterMark(tbFilesCSV, emptyReportsFileName, Brushes.Gray, FileNamesCSV);
        }

        #region События для обработки водяного знака
        public void TbWm_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
            => waterMark.TbWm_GotKeyboardFocus(sender, e);

        public void TbWm_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
            => waterMark.TbWm_LostKeyboardFocus(sender, e);
        #endregion

        /// <summary>
        /// Создаёт диалоговое окно выбора файлов
        /// Путь и имена файлов записывает в контент вызвавшего TextBox-а с разделителем '\n'.
        /// Если объект, хранящийся в waterMark для данного TextBox-а, имеет тип List<string>, 
        /// то массив файлов переписывается в этот список.
        /// Если объект, хранящийся в waterMark для данного TextBox-а, имеет тип StringBuilder, 
        /// то первая строка из массива строк записывается в этот объект.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TbWm_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string[] files = GetFileNamesToOpen();

            if (sender is TextBox tb)
            {
                if (files == null || files.Length == 0)
                {
                    return;
                }

                if (waterMark.IsContains(tb) == true)
                {
                    if (waterMark.GetObject(tb) is List<string> ls)
                    {
                        ls.Clear();

                        foreach (string s in files)
                        {
                            ls.Add(s);
                        }
                    }
                    else
                    if (waterMark.GetObject(tb) is StringBuilder sb)
                    {
                        sb.Clear();
                        sb.Append(files[0]);
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
        /// Обрабатывает выбор sql файла из каталога или создание нового.
        /// Расширение, если его нет, добавляется автоматически.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbSqlScriptFile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.Text = GetFileNameToSave("SQL|*.sql|All files *.*|*.*", ".sql") ?? "";
            }
        }

        /// <summary>
        /// Создаёт диалоговое окно выбора файла и возвращает имя выбранного файла.
        /// </summary>
        /// <returns>Имя файла или null</returns>
        private string GetFileNameToSave(string filter, string ext)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = filter
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string fileName = saveFileDialog.FileName.Trim();

                if (fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase) == false)
                {
                    fileName += ext;
                }

                return fileName;
            }

            return null;
        }

        /// <summary>
        /// Создаёт диалоговое окно выбора файла/файлов и возвращает массив выбранных файлов.
        /// </summary>
        /// <param name="multiselect">Признак множественного(true) или одиночного(false) выбора</param>
        /// <returns></returns>
        private string[] GetFileNamesToOpen(bool multiselect = true)
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
        /// Создаёт диалоговое окно выбора/создания файла.
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
