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
using System.Globalization;
using System.Collections.ObjectModel;

namespace CSVtoSQL.CSV_report
{
    /// <summary>
    /// Логика взаимодействия для WindowChoiceNameClient.xaml
    /// </summary>
    public partial class WindowChoosingClientName : Window
    {
        private readonly ObservableCollection<Client> list = new ObservableCollection<Client>();

        public Client SelectedClient { get; set; }

  
        public WindowChoosingClientName(ListClients lc)
        {
            InitializeComponent();

            InitListBox(lc);
        }

        private void InitListBox(ListClients lc)
        {
            int maxCountStirng = 0;

            Client maxStringClient = null;

            foreach (Client c in lc)
            {
                list.Add(c);
                
                if (maxCountStirng < c.Name.Length)
                {
                    maxCountStirng = c.Name.Length;

                    maxStringClient = c;
                }
            }

            string text = maxStringClient.ToString();

            FormattedText ft = new FormattedText(text, CultureInfo.CurrentUICulture, listBoxClients.FlowDirection,
                                                 new Typeface(listBoxClients.FontFamily, listBoxClients.FontStyle,
                                                              listBoxClients.FontWeight, listBoxClients.FontStretch),
                                                 listBoxClients.FontSize, listBoxClients.Foreground, VisualTreeHelper.GetDpi(listBoxClients).PixelsPerDip);

            listBoxClients.Width = listBoxClients.Margin.Left + listBoxClients.Margin.Right
                                 + listBoxClients.BorderThickness.Left + listBoxClients.BorderThickness.Right
                                 + Math.Floor(ft.Width + 1) + Math.Floor(SystemParameters.VerticalScrollBarWidth + 5);



            DescriptionOfPurpose.Width = listBoxClients.Width;

            listBoxClients.ItemsSource = list;
            
            listBoxClients.Items.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
        }

        private void AcceptId_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem();
        }

        private void ListBoxClients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectedItem();
        }

        private void SelectedItem()
        {
            SelectedClient = ((Client)listBoxClients.SelectedItem);

            DialogResult = true;
        }
    }
}
