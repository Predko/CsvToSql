using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CSVtoDataBase.CSV_report
{
    /// <summary>
    /// Логика взаимодействия для WindowChoiceNameCustomer.xaml
    /// </summary>
    public partial class WindowChoosingCustomerName : Window
    {
        private readonly ObservableCollection<Customer> list = new ObservableCollection<Customer>();

        public Customer SelectedCustomer { get; set; }


        public WindowChoosingCustomerName(ListCustomers lc)
        {
            InitializeComponent();

            InitListBox(lc);
        }

        private void InitListBox(ListCustomers lc)
        {
            int maxCountStirng = 0;

            Customer maxStringCustomer = null;

            foreach (Customer c in lc)
            {
                list.Add(c);

                if (maxCountStirng < c.Name.Length)
                {
                    maxCountStirng = c.Name.Length;

                    maxStringCustomer = c;
                }
            }

            string text = maxStringCustomer.ToString();

            FormattedText ft = new FormattedText(text, CultureInfo.CurrentUICulture, listBoxCustomers.FlowDirection,
                                                 new Typeface(listBoxCustomers.FontFamily, listBoxCustomers.FontStyle,
                                                              listBoxCustomers.FontWeight, listBoxCustomers.FontStretch),
                                                 listBoxCustomers.FontSize, listBoxCustomers.Foreground, VisualTreeHelper.GetDpi(listBoxCustomers).PixelsPerDip);

            //listBoxCustomers.Width = listBoxCustomers.Margin.Left + listBoxCustomers.Margin.Right
            //                     + listBoxCustomers.BorderThickness.Left + listBoxCustomers.BorderThickness.Right
            //                     + Math.Floor(ft.Width + 1) + Math.Floor(SystemParameters.VerticalScrollBarWidth + 5);



            //DescriptionOfPurpose.Width = listBoxCustomers.Width;

            listBoxCustomers.ItemsSource = list;

            listBoxCustomers.Items.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
        }

        private void AcceptId_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem();
        }

        private void ListBoxCustomers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectedItem();
        }

        private void SelectedItem()
        {
            SelectedCustomer = ((Customer)listBoxCustomers.SelectedItem);

            DialogResult = true;
        }
    }
}
