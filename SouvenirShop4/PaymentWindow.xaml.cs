using SouvenirShop4.Connect;
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

namespace SouvenirShop4
{
    /// <summary>
    /// Логика взаимодействия для PaymentWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window
    {
        private List<MainWindow.CartItem> cartItems;
        private Users currentUser;

        public PaymentWindow(List<MainWindow.CartItem> items, Users user)
        {
            InitializeComponent();
            cartItems = items;
            currentUser = user;

            dgOrderItems.ItemsSource = cartItems;

            decimal total = cartItems.Sum(item => item.TotalPrice);
            txtTotal.Text = $"Общая сумма к оплате: {total:C}";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем клиента если его нет
                var customer = Connection.entities.Customers
                    .FirstOrDefault(c => c.Email == currentUser.Email);

                if (customer == null)
                {
                    customer = new Customers
                    {
                        FirstName = currentUser.FirstName,
                        LastName = currentUser.LastName,
                        Email = currentUser.Email,
                        RegistrationDate = DateTime.Now
                    };
                    Connection.entities.Customers.Add(customer);
                    Connection.entities.SaveChanges();
                }

                // Создаем заказ
                Orders newOrder = new Orders
                {
                    CustomerId = customer.CustomerId,
                    OrderDate = DateTime.Now,
                    TotalAmount = cartItems.Sum(item => item.TotalPrice),
                    Status = "В ожидании",
                    PaymentMethod = rbCash.IsChecked == true ? "Наличные" : "Карта",
                    Notes = $"Заказ пользователя: {currentUser.Username}"
                };

                Connection.entities.Orders.Add(newOrder);
                Connection.entities.SaveChanges();

                // Добавляем элементы заказа
                foreach (var cartItem in cartItems)
                {
                    OrderItems orderItem = new OrderItems
                    {
                        OrderId = newOrder.OrderId,
                        SouvenirId = cartItem.Souvenir.SouvenirId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice
                    };
                    Connection.entities.OrderItems.Add(orderItem);
                }

                Connection.entities.SaveChanges();
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}");
            }
        }
    }
}
