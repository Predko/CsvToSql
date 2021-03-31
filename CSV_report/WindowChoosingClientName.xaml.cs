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

namespace WpfAppConvertation.CSV_report
{
    /// <summary>
    /// Логика взаимодействия для WindowChoiceNameClient.xaml
    /// </summary>
    public partial class WindowChoosingClientName : Window
    {
        private readonly ListClientsName listClients;

        public int IdClient { get; set; }

  
        public WindowChoosingClientName(ListClientsName lcn)
        {
            InitializeComponent();

            listClients = lcn;

            foreach (var kvp in listClients)
            {
                listBoxClients.Items.Add(kvp);
            }
        }

        private void AcceptId_Click(object sender, RoutedEventArgs e)
        {
            IdClient = ((KeyValuePair<int, string>)listBoxClients.SelectedItem).Key;

            this.DialogResult = true;
        }
    }
}
