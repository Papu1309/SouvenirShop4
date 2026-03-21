
using SouvenirShop4;
using SouvenirShop4.Connect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SouvenirShop
{
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

        private void PaymentMethod_Changed(object sender, RoutedEventArgs e)
        {
            if (rbCard == null || rbCash == null) return;

            if (rbCard.IsChecked == true)
            {
                cardPanel.Visibility = Visibility.Visible;
            }
            else
            {
                cardPanel.Visibility = Visibility.Collapsed;
                ClearCardErrors();
            }
        }

        #region Валидация номера карты
        private void CardNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtCardNumber == null) return;
            string raw = txtCardNumber.Text.Replace(" ", "");
            if (raw.Length > 16)
            {
                raw = raw.Substring(0, 16);
                txtCardNumber.Text = FormatCardNumber(raw);
                txtCardNumber.CaretIndex = txtCardNumber.Text.Length;
            }

            if (raw.Length > 0)
            {
                string formatted = FormatCardNumber(raw);
                if (txtCardNumber.Text != formatted)
                {
                    txtCardNumber.Text = formatted;
                    txtCardNumber.CaretIndex = txtCardNumber.Text.Length;
                }
            }

            ValidateCardNumber();
        }

        private string FormatCardNumber(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            var groups = new List<string>();
            for (int i = 0; i < raw.Length; i += 4)
            {
                groups.Add(raw.Substring(i, Math.Min(4, raw.Length - i)));
            }
            return string.Join(" ", groups);
        }

        private bool ValidateCardNumber()
        {
            if (txtCardNumber == null) return false;
            string raw = txtCardNumber.Text.Replace(" ", "");
            bool isValid = raw.Length == 16 && raw.All(char.IsDigit);

            if (!isValid)
            {
                lblCardNumberError.Text = "Номер карты должен содержать 16 цифр";
                lblCardNumberError.Visibility = Visibility.Visible;
            }
            else
            {
                lblCardNumberError.Visibility = Visibility.Collapsed;
            }
            return isValid;
        }
        #endregion

        #region Валидация имени владельца
        private void CardHolderValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[a-zA-Zа-яА-Я\s\.\-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void CardHolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateCardHolder();
        }

        private bool ValidateCardHolder()
        {
            if (txtCardHolder == null) return false;
            string name = txtCardHolder.Text.Trim();
            bool isValid = !string.IsNullOrWhiteSpace(name) && name.Length >= 3;

            if (!isValid)
            {
                lblCardHolderError.Text = "Введите имя владельца (минимум 3 символа)";
                lblCardHolderError.Visibility = Visibility.Visible;
            }
            else
            {
                lblCardHolderError.Visibility = Visibility.Collapsed;
            }
            return isValid;
        }
        #endregion

        #region Валидация срока действия
        private void Expiry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtExpiry == null) return;
            string text = txtExpiry.Text.Replace("/", "");
            if (text.Length > 4)
                text = text.Substring(0, 4);

            if (text.Length >= 2)
            {
                string month = text.Substring(0, 2);
                string year = text.Length > 2 ? text.Substring(2) : "";
                txtExpiry.Text = $"{month}/{year}";
                txtExpiry.CaretIndex = txtExpiry.Text.Length;
            }
            else
            {
                txtExpiry.Text = text;
            }

            ValidateExpiry();
        }

        private bool ValidateExpiry()
        {
            if (txtExpiry == null) return false;
            string[] parts = txtExpiry.Text.Split('/');
            if (parts.Length != 2 || parts[0].Length != 2 || parts[1].Length != 2)
            {
                lblExpiryError.Text = "Формат: MM/YY";
                lblExpiryError.Visibility = Visibility.Visible;
                return false;
            }

            if (!int.TryParse(parts[0], out int month) || month < 1 || month > 12)
            {
                lblExpiryError.Text = "Месяц должен быть от 01 до 12";
                lblExpiryError.Visibility = Visibility.Visible;
                return false;
            }

            if (!int.TryParse(parts[1], out int year))
            {
                lblExpiryError.Text = "Год должен быть числом";
                lblExpiryError.Visibility = Visibility.Visible;
                return false;
            }

            int currentYear = DateTime.Now.Year % 100;
            int currentMonth = DateTime.Now.Month;

            if (year < currentYear || (year == currentYear && month < currentMonth))
            {
                lblExpiryError.Text = "Срок действия карты истек";
                lblExpiryError.Visibility = Visibility.Visible;
                return false;
            }

            lblExpiryError.Visibility = Visibility.Collapsed;
            return true;
        }
        #endregion

        #region Валидация CVV
        private void CVV_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateCVV();
        }

        private bool ValidateCVV()
        {
            if (txtCVV == null) return false;
            string cvv = txtCVV.Text;
            bool isValid = cvv.Length == 3 && cvv.All(char.IsDigit);

            if (!isValid)
            {
                lblCVVError.Text = "CVV должен содержать 3 цифры";
                lblCVVError.Visibility = Visibility.Visible;
            }
            else
            {
                lblCVVError.Visibility = Visibility.Collapsed;
            }
            return isValid;
        }
        #endregion

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void ClearCardErrors()
        {
            if (lblCardNumberError != null) lblCardNumberError.Visibility = Visibility.Collapsed;
            if (lblCardHolderError != null) lblCardHolderError.Visibility = Visibility.Collapsed;
            if (lblExpiryError != null) lblExpiryError.Visibility = Visibility.Collapsed;
            if (lblCVVError != null) lblCVVError.Visibility = Visibility.Collapsed;
        }

        private bool ValidateCardDetails()
        {
            return ValidateCardNumber() && ValidateCardHolder() && ValidateExpiry() && ValidateCVV();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Защита от null
            if (rbCard == null)
            {
                ProcessOrder(false);
                return;
            }

            // Если оплата картой, проверяем данные
            if (rbCard.IsChecked == true)
            {
                if (!ValidateCardDetails())
                {
                    MessageBox.Show("Пожалуйста, заполните все данные карты корректно.", "Ошибка валидации",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            ProcessOrder(rbCard.IsChecked == true);
        }

        private void ProcessOrder(bool isCardPayment)
        {
            try
            {
                // Создаем клиента
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
                    PaymentMethod = isCardPayment ? "Карта" : "Наличные",
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
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}