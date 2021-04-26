using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CSVtoDataBase
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Переменная, хранящая объект, управляющий водяными знаками текстовых полей.
        /// </summary>
        private WaterMarkForTextBox waterMark;

        #region Строки водяных знаков для соответствующих текстовых полей.
        private const string emptyReportsFileName = "Введите имена файлов выписок...";

        private const string emptyDataBaseName = "Введите имя базы данных...";
        #endregion

        /// <summary>
        /// Список имён файлов выписок. Выбирается пользователем.
        /// </summary>
        private readonly List<string> FileNamesCSV = new List<string>();

        private void InitializeTextBoxWaterMark()
        {
            waterMark = new WaterMarkForTextBox();

            waterMark.AddWaterMark(tbFilesCSV, emptyReportsFileName, Brushes.Gray, FileNamesCSV);

            waterMark.AddWaterMark(TbNameDataBase, emptyDataBaseName, Brushes.Gray, null);
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
    }
}
