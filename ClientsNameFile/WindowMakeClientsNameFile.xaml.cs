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

namespace WpfAppConvertation.ClientsFileName
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

        private void TbWaterMark_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => mainWimdow.TbWaterMark_GotKeyboardFocus(sender, e);

        private void TbWaterMark_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => mainWimdow.TbWaterMark_LostKeyboardFocus(sender, e);


        /// <summary>
        /// Создаёт диалоговое окно выбора/создания файла
        /// Путь и имя файла записывает в контент вызвавшего TextBox/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbWaterMark_MouseDoubleClick(object sender, MouseButtonEventArgs e) => mainWimdow.TbWaterMark_MouseDoubleClick(sender, e);

        private void BtnCreateXMLNamesClientFile_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, введены ли имена файлов.
            if (tbClientsNameFileIn.Text == mainWimdow.GetWaterMarkString(tbClientsNameFileIn)
                || tbClientsNameFileOut.Text == mainWimdow.GetWaterMarkString(tbClientsNameFileOut))
            {
                DialogResult = false;
            }
            else
            {
                OriginalFile = tbClientsNameFileIn.Text.Trim();

                DialogResult = true;
            }
        }

        /// <summary>
        /// Создаёт диалоговое окно выбора файла/файлов и возвращает массив выбранных файлов.
        /// </summary>
        /// <param name="multiselect">Признак множественного(true) или одиночного(false) выбора</param>
        /// <returns></returns>
        private string GetNameSaveFiles()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "All files *.*|*.*|CSV|*.csv|TXT|*.txt|XML|*.xml",
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

        private void TbClientsNameFileOut_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            tbClientsNameFileOut.Text = GetNameSaveFiles();
        }
    }
}
