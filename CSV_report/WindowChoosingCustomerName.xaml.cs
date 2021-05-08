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

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        public WindowChoosingCustomerName(ObservableCollection<Customer> list, string nameCompany)
        {
            InitializeComponent();

            TbNameCompany.Text = nameCompany;

            IdCompany = -1;

            NewNameCompany = null;

            BtnChangeName.IsEnabled = false;

            LvChooseCustomer.ItemsSource = list;

            CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(LvChooseCustomer.ItemsSource);

            collectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
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

        private void LvChooseCustomerColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            
            string sortBy = column.Tag.ToString();
            
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                
                LvChooseCustomer.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);

            LvChooseCustomer.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }
    }

    public class SortAdorner : Adorner
    {
        private static Geometry ascGeometry = Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

        private static Geometry descGeometry = Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

        public ListSortDirection Direction { get; private set; }

        public SortAdorner(UIElement element, ListSortDirection dir) : base(element)
        {
            this.Direction = dir;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement.RenderSize.Width < 20)
                return;

            TranslateTransform transform = new TranslateTransform
                (
                    AdornedElement.RenderSize.Width - 15,
                    (AdornedElement.RenderSize.Height - 5) / 2
                );
            drawingContext.PushTransform(transform);

            Geometry geometry = ascGeometry;
            if (this.Direction == ListSortDirection.Descending)
                geometry = descGeometry;
            drawingContext.DrawGeometry(Brushes.Black, null, geometry);

            drawingContext.Pop();
        }
    }
}
