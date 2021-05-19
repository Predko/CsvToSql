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
        /// <summary>
        /// Выбранный клиент в списке.
        /// </summary>
        public Customer SelectedCustomer;
        
        public WindowChoosingCustomerName(IEnumerable<Customer> list, string nameCompany)
        {
            InitializeComponent();

            TbNameCompany.Text = nameCompany;

            BtnChangeName.IsEnabled = false;

            LvChooseCustomer.ItemsSource = list;
            
            LvChooseCustomer.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        /// <summary>
        /// Указывает, нужно ли менять УНП для выбранного клиента.
        /// </summary>
        /// <returns></returns>
        public bool? NeedChangeUNP() => ChbChangeUNP.IsChecked;

        /// <summary>
        /// Обработчик подтверждения выбора клиента в списке.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem();
        }

        /// <summary>
        /// Обработчик выбора клиента в списке с помощью мыши.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxCustomers_Select(object sender, MouseButtonEventArgs e)
        {
            SelectedItem();
        }

        /// <summary>
        /// Обрабатывает выбор клиента и закрытие окна.
        /// </summary>
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

        /// <summary>
        /// Обработчик выбора клиента с изменением его названия.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnChangeName_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem();

            SelectedCustomer.Name = TbNameCompany.Text;
        }

        /// <summary>
        /// Обрабатывает изменение выбора в списке.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvChooseCustomer_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            bool isEnabled = (LvChooseCustomer.SelectedItem != null);
            
            BtnChangeName.IsEnabled = isEnabled;
            BtnOk.IsEnabled = isEnabled;
        }

        /// <summary>
        /// Обрабатывает событие закрытие окна. Удаляет привязку к списку клиентов.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, System.EventArgs e)
        {
            LvChooseCustomer.ItemsSource = null;
        }
    }
}
