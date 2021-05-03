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
        public int IdCompany { get; set; }

        public string NewNameCompany { get; set; }

        public Customer SelectedCustomer;


        public WindowChoosingCustomerName(ObservableCollection<Customer> list, string nameCompany)
        {
            InitializeComponent();

            TbNameCompany.Text = nameCompany;

            IdCompany = -1;

            NewNameCompany = null;

            BtnChangeName.IsEnabled = false;

            ListBoxCustomers.ItemsSource = list;

            ListBoxCustomers.Items.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
        }

        public bool? NeedChangeUNP() => ChbChangeUNP.IsChecked;

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
            if (ListBoxCustomers.SelectedItem == null)
            {
                DialogResult = false;
            }
            else
            {
                SelectedCustomer = ((Customer)ListBoxCustomers.SelectedItem);

                DialogResult = true;
            }
        }

        private void BtnChangeName_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem();

            //NewNameCompany = TbNameCompany.Text;
            SelectedCustomer.Name = TbNameCompany.Text;
        }

        private void ListBoxCustomers_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ListBoxCustomers.SelectedItem != null)
            {
                BtnChangeName.IsEnabled = true;
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            //if (ListBoxCustomers.SelectedItem != null)
            //{
            //    //IdCompany = ((Customer)ListBoxCustomers.SelectedItem).Id;
            //}

            ListBoxCustomers.ItemsSource = null;
        }
    }
}
