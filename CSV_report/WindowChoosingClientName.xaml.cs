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
using System.ComponentModel;

namespace SCVtiSQL.CSV_report
{
    /// <summary>
    /// Логика взаимодействия для WindowChoiceNameClient.xaml
    /// </summary>
    public partial class WindowChoosingClientName : Window
    {
        private readonly ListClients listClients;

        public Client SelectedClient { get; set; }

  
        public WindowChoosingClientName(ListClients lc)
        {
            InitializeComponent();

            listClients = lc;

            InitListBox();

            DescriptionOfPurpose.Width = listBoxClients.Width;
        }

        private void InitListBox()
        {
            foreach (var client in listClients)
            {
                listBoxClients.Items.Add(client);
            }

            listBoxClients.Items.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
        }

        private void AcceptId_Click(object sender, RoutedEventArgs e)
        {
            SelectedClient = ((Client)listBoxClients.SelectedItem);

            this.DialogResult = true;
        }
    }
}
