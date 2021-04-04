using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSVtoSQL.ClientsFileName
{
    /// <summary>
    /// Логика взаимодействия для WindowMakeClientsNameFile.xaml
    /// </summary>
    public partial class WindowMakeClientsNameFile : Window
    {
        private const string emptyCreateXMLfileName = "Введите имя файла XML для записи данных клиентов...";

        private const string emptyInputfileName = "Введите имя файла с данными клиентов...";

        public string OriginalFile { get; set; }

        public string XmlClientsNameFile { get; set; }

        private MainWindow mainWimdow;


        
        public WindowMakeClientsNameFile()
        {
            InitializeComponent();
        }

        public void InitializeWaterMark(MainWindow mw)
        {
            mainWimdow = mw;

            mainWimdow.SetWaterMarkString(tbClientsNameFileIn, emptyCreateXMLfileName);

            mainWimdow.SetWaterMarkString(tbClientsNameFileOut, emptyInputfileName);
        }

        private void TbWm_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => mainWimdow.TbWm_GotKeyboardFocus(sender, e);

        private void TbWm_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => mainWimdow.TbWm_LostKeyboardFocus(sender, e);


        /// <summary>
        /// Создаёт диалоговое окно выбора/создания файла
        /// Путь и имя файла записывает в контент вызвавшего TextBox/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbWm_MouseDoubleClick(object sender, MouseButtonEventArgs e) => mainWimdow.TbWm_MouseDoubleClick(sender, e);

        private void BtnCreateXMLNamesClientFile_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, введены ли имена файлов.
            if (mainWimdow.IsWaterMarkTextBoxEmpty(tbClientsNameFileIn) == true)
            {
                MessageForEmptyTextBox messageBox = new MessageForEmptyTextBox(tbClientsNameFileIn);

                messageBox.Show("Укажите исходный текстовый файл с данными клиентов.");

                return;
            }
            else
            if (mainWimdow.IsWaterMarkTextBoxEmpty(tbClientsNameFileOut) == true)
            {
                MessageForEmptyTextBox messageBox = new MessageForEmptyTextBox(tbClientsNameFileOut);

                messageBox.Show("Укажите файл для записи данных клиентов.");

                return;
            }
            else
            {
                OriginalFile = tbClientsNameFileIn.Text.Trim();

                DialogResult = true;
            }
        }

        private void TbClientsNameFileOut_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            tbClientsNameFileOut.Text = GetFileNameToSave() ?? "";
        }

        /// <summary>
        /// Создаёт диалоговое окно выбора файла и возвращает имя выбранного файла.
        /// </summary>
        /// <returns>Имя файла или null</returns>
        private string GetFileNameToSave()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "TXT|*.txt|All files *.*",
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                XmlClientsNameFile = saveFileDialog.FileName.Trim();

                if (XmlClientsNameFile.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) == false)
                {
                    XmlClientsNameFile += ".xml";
                }

                return XmlClientsNameFile;
            }

            return null;
        }
    }
}
