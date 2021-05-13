using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CSVtoDataBase
{
    /// <summary>
    /// Логика взаимодействия для WindowChoiceNameCustomer.xaml
    /// </summary>
    public partial class WindowChoosingCustomerName : Window
    {
        public int IdCompany { get; set; }

        public string NewNameCompany { get; set; }

        public Customer SelectedCustomer;
        
        //Использовать DataGrid!
        public WindowChoosingCustomerName(IEnumerable<Customer> list, string nameCompany)
        {
            InitializeComponent();

            TbNameCompany.Text = nameCompany;

            IdCompany = -1;

            NewNameCompany = null;

            BtnChangeName.IsEnabled = false;

            LvChooseCustomer.ItemsSource = list;
            
            LvChooseCustomer.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        public bool? NeedChangeUNP() => ChbChangeUNP.IsChecked;

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem();
        }

        private void ListBoxCustomers_Select(object sender, MouseButtonEventArgs e)
        {
            SelectedItem();
        }

        private void SelectedItem()
        {
            if (LvChooseCustomer.SelectedItem == null)
            {
                DialogResult = false;
            }
            else
            {
                SelectedCustomer = ((Customer)LvChooseCustomer.SelectedItem);

                DialogResult = true;
            }
        }

        private void BtnChangeName_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem();

            SelectedCustomer.Name = TbNameCompany.Text;
        }

        private void LvChooseCustomer_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            bool isEnabled = (LvChooseCustomer.SelectedItem != null);
            
            BtnChangeName.IsEnabled = isEnabled;
            BtnOk.IsEnabled = isEnabled;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            LvChooseCustomer.ItemsSource = null;
        }
    }
}
