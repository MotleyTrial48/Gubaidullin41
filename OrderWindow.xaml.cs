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
using System.Windows.Shapes;

namespace Laba10
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
        List<Product> selectedProducts = new List<Product>();
        private User currentUser; // кто оформляет заказ (null для гостя)

        public OrderWindow(List<OrderProduct> selectedOrderProducts, List<Product> selectedProducts, string FIO, User user)
        {
            InitializeComponent();
            currentUser = user;
            this.selectedOrderProducts = selectedOrderProducts;
            this.selectedProducts = selectedProducts;

            PickupCombo.ItemsSource = Губайдуллин41Entities.GetContext().PickupPoint.ToList(); // пункты выдачи из БД

            ClientTB.Text = user == null ? "" : FIO; // ФИО только у авторизованного
            TBOrderID.Text = selectedOrderProducts.First().OrderID.ToString(); // номер заказа

            ShoeListView.ItemsSource = selectedProducts;
            foreach (Product p in selectedProducts)
            {
                p.Quantity = 1;
                foreach (OrderProduct q in selectedOrderProducts)
                {
                    if (p.ProductArticleNumber == q.ProductArticleNumber)
                        p.Quantity = q.Quantity; // количество берём из позиции заказа
                }
            }

            OrderDP.Text = DateTime.Now.ToString(); // дата формирования
            SetDeliveryDate();
            UpdateTotals();
        }
        private void SetDeliveryDate()
        {
            bool allInStock = selectedProducts.All(p => p.ProductQuantityInStock > 3);
            DeliveryDP.SelectedDate = DateTime.Now.AddDays(allInStock ? 3 : 6);
        }

        // подсчёт суммы и скидки в реальном времени
        private void UpdateTotals()
        {
            decimal sum = 0;
            decimal discount = 0;
            foreach (Product p in selectedProducts)
            {
                sum += p.ProductCost * p.Quantity;
                discount += p.ProductCost * p.ProductDiscountAmount / 100m * p.Quantity;
            }
            SumTB.Text = (sum - discount).ToString("0.00") + " рублей"; // с учётом скидки
            DiscountTB.Text = discount.ToString("0.00") + " рублей";
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button).DataContext as Product; // товар этой строки
            prod.Quantity++;
            //взяли элемент списка равный prod
            var selectedOP = selectedOrderProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);
            //увеличить кол-во для этого
            selectedOP.Quantity++;
            SetDeliveryDate();
            UpdateTotals();
            ShoeListView.Items.Refresh();
        }

        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button).DataContext as Product;
            var selectedOP = selectedOrderProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);
            prod.Quantity--;
            selectedOP.Quantity--;
            if (prod.Quantity <= 0) // количество 0, удаляем товар из заказа
            {
                prod.Quantity = 0;
                selectedOrderProducts.Remove(selectedOP);
                selectedProducts.Remove(prod);
                ShoeListView.ItemsSource = null;
                ShoeListView.ItemsSource = selectedProducts;
            }
            if (selectedOrderProducts.Count == 0) // заказ пуст, закрываем окно
            {
                MessageBox.Show("Заказ пуст");
                Close();
                return;
            }
            SetDeliveryDate();
            UpdateTotals();
            ShoeListView.Items.Refresh();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PickupCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите пункт выдачи");
                return;
            }
            if (DeliveryDP.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату доставки");
                return;
            }

            var context = Губайдуллин41Entities.GetContext();
            var order = new Order(); // новый заказ
            order.OrderDate = DateTime.Now.Date;
            order.OrderDeliveryDate = DeliveryDP.SelectedDate.Value;
            order.OrderPickupPointID = (PickupCombo.SelectedItem as PickupPoint).PickupPointID;
            order.OrderClientID = currentUser == null ? (int?)null : currentUser.UserID; // клиент или гость
            order.OrderCode = new Random().Next(100, 1000); // код получения из 3 цифр
            order.OrderStatus = "Новый";

            foreach (OrderProduct op in selectedOrderProducts)
            {
                var newOP = new OrderProduct();
                newOP.ProductArticleNumber = op.ProductArticleNumber;
                newOP.Quantity = op.Quantity;
                order.OrderProduct.Add(newOP); // позиции заказа
            }

            context.Order.Add(order);
            try
            {
                context.SaveChanges(); // записываем в БД
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            MessageBox.Show("Заказ № " + order.OrderID + " сохранён. Код получения: " + order.OrderCode);
            foreach (Product p in selectedProducts)
                p.Quantity = 0;
            selectedOrderProducts.Clear(); // заказ очищен
            Close();
        }
    }
}

