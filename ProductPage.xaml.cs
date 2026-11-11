using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Laba10
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    

    public partial class ProductPage : Page
    {

        List<OrderProduct> selectedOrderProducts = new List<OrderProduct>(); // позиции заказа
        List<Product> selectedProducts = new List<Product>(); // выбранные товары
        private int newOrderID; // номер нового заказа
        private User currentUser; // кто сейчас вошёл (null для гостя)


        public ProductPage(User user)
        {
            InitializeComponent();

            currentUser = user;

            if (user == null)
            {
                FIOTB.Text = "Гость";
                RoleTB.Text = "Гость";
            }
            else
            {
                FIOTB.Text = user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;
                switch (user.UserRole)
                {
                    case 1:
                        RoleTB.Text = "Клиент";
                        break;
                    case 2:
                        RoleTB.Text = "Менеджер";
                        break;
                    case 3:
                        RoleTB.Text = "Администратор";
                        break;

                }
            }

            var currentProducts = Губайдуллин41Entities.GetContext().Product.ToList();

            ProductListView.ItemsSource = currentProducts;

            ComboType.SelectedIndex = 0;

            UpdateProduct();
        }




        private void UpdateProduct()
        {
            var currentProducts = Губайдуллин41Entities.GetContext().Product.ToList();


            if (ComboType.SelectedIndex == 0)
            {
                currentProducts = currentProducts.Where(p => (p.ProductDiscountAmount >= 0 && p.ProductDiscountAmount <= 100)).ToList();
            }
            if (ComboType.SelectedIndex == 1)
            {
                currentProducts = currentProducts.Where(p => (p.ProductDiscountAmount >= 0 && p.ProductDiscountAmount < 10)).ToList();
            }
            if (ComboType.SelectedIndex == 2)
            {
                currentProducts = currentProducts.Where(p => (p.ProductDiscountAmount >= 10 && p.ProductDiscountAmount < 15)).ToList();
            }
            if (ComboType.SelectedIndex == 3)
            {
                currentProducts = currentProducts.Where(p => (p.ProductDiscountAmount >= 15 && p.ProductDiscountAmount <= 100)).ToList();
            }

            currentProducts = currentProducts.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            //реализуем поиск данных в листвью при вводе текста в окно поиска
            currentProducts = currentProducts.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            //реализуем сортировку по убыванию
            if (RbuttonDown.IsChecked.Value)
            {
                currentProducts = currentProducts.OrderByDescending(p => p.ProductCost).ToList();
            }
            //реализуем сортировку по возрастанию
            if (RButtonUp.IsChecked.Value)
            {
                currentProducts = currentProducts.OrderBy(p => p.ProductCost).ToList();
            }

            //отображаем итоги поиска/фильтрации/сортировки
            ProductListView.ItemsSource = currentProducts;

            //количество выведенных из общего количества
            TBCount.Text = "кол-во " + currentProducts.Count + " из " + Губайдуллин41Entities.GetContext().Product.Count();

        }


        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProduct();
        }

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProduct();
        }

        private void RbuttonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProduct();
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProduct();
        }

        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            selectedProducts = selectedProducts.Distinct().ToList(); // убираем повторы товаров
            OrderWindow orderWindow = new OrderWindow(selectedOrderProducts, selectedProducts, FIOTB.Text, currentUser);
            orderWindow.ShowDialog(); // модальное окно заказа

            //после закрытия окна: если заказ пуст (сохранён или всё удалено)
            if (selectedOrderProducts.Count == 0)
            {
                OrderBtn.Visibility = Visibility.Collapsed; // прячем кнопку
                selectedProducts.Clear();
                var allOrders = Губайдуллин41Entities.GetContext().Order.ToList();
                newOrderID = allOrders.Count == 0 ? 1 : allOrders.Max(p => p.OrderID) + 1; // номер следующего заказа
            }
        }

    }
}
