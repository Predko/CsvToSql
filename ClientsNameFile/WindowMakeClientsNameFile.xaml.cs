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

    public delegate void TbMouseDoubleClick(object sender, MouseButtonEventArgs e);

    /// <summary>
    /// Логика взаимодействия для WindowMakeClientsNameFile.xaml
    /// </summary>
    public partial class WindowMakeClientsNameFile : Window
    {
        private const string emptyCreateXMLfileName = "Введите имя файла XML для записи данных клиентов...";

        private const string emptyInputfileName = "Введите имя файла с данными клиентов...";

        public string OriginalFile { get; set; }

        public string XmlClientsNameFile { get; set; }

        private WaterMarkForTextBox waterMark;

        private TbMouseDoubleClick mouseDoubleClick;



        public WindowMakeClientsNameFile(WaterMarkForTextBox wm, TbMouseDoubleClick tbMouseDoubleClick)
        {
            waterMark = wm;

            mouseDoubleClick = tbMouseDoubleClick;

            InitializeComponent();
        }

        public void InitializeWaterMark()
        {
            waterMark.AddWaterMark(tbClientsNameFileIn, emptyCreateXMLfileName, Brushes.Gray);

            waterMark.AddWaterMark(tbClientsNameFileOut, emptyInputfileName, Brushes.Gray);
        }

        private void TbWm_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => waterMark.TbWm_GotKeyboardFocus(sender, e);

        private void TbWm_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => waterMark.TbWm_LostKeyboardFocus(sender, e);


        /// <summary>
        /// Создаёт диалоговое окно выбора/создания файла
        /// Путь и имя файла записывает в контент вызвавшего TextBox/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbWm_MouseDoubleClick(object sender, MouseButtonEventArgs e) => mouseDoubleClick(sender, e);

        private void BtnCreateXMLNamesClientFile_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, введены ли имена файлов.
            if (waterMark.WaterMarkTextBoxIsEmpty(tbClientsNameFileIn) == true)
            {
                MessageForEmptyTextBox messageBox = new MessageForEmptyTextBox(tbClientsNameFileIn);

                messageBox.Show("Укажите исходный текстовый файл с данными клиентов.");

                return;
            }
            else
            if (waterMark.WaterMarkTextBoxIsEmpty(tbClientsNameFileOut) == true)
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
                Filter = "XML|*.xml|All files *.*|*.*",
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

        private void WindowMakeClientsNameFile_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            waterMark.RemoveWaterMark(tbClientsNameFileOut);

            waterMark.RemoveWaterMark(tbClientsNameFileIn);
        }
    }
}
