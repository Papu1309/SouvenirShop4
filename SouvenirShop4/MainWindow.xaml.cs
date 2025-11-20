using SouvenirShop4.Connect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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


namespace SouvenirShop4
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<CartItem> cartItems;
        private List<Souvenirs> allSouvenirs;
        private List<Categories> allCategories;

        public MainWindow()
        {
            InitializeComponent();

            if (NavigationManager.CurrentUser != null)
            {
                InitializeWithUser();
            }
            else
            {
                NavigationManager.ShowLoginWindow();
                this.Close();
            }
        }

        private void InitializeWithUser()
        {
            cartItems = new ObservableCollection<CartItem>();

            txtUserInfo.Text = $"Пользователь: {NavigationManager.CurrentUser.FirstName} {NavigationManager.CurrentUser.LastName}";

            // Показываем вкладку админа если пользователь админ
            if (NavigationManager.IsAdmin)
            {
                adminTab.Visibility = Visibility.Visible;
            }

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Загрузка категорий
                allCategories = Connection.entities.Categories.ToList();
                cmbCategories.ItemsSource = allCategories;

                // Загрузка сувениров и связывание с категориями
                allSouvenirs = Connection.entities.Souvenirs.ToList();

                // Вручную связываем сувениры с категориями
                foreach (var souvenir in allSouvenirs)
                {
                    if (souvenir.CategoryId.HasValue)
                    {
                        souvenir.Categories = allCategories.FirstOrDefault(c => c.CategoryId == souvenir.CategoryId.Value);
                    }
                }

                dgSouvenirs.ItemsSource = allSouvenirs;

                // Загрузка данных для админской таблицы
                if (NavigationManager.IsAdmin)
                {
                    dgAdminSouvenirs.ItemsSource = allSouvenirs;
                    UpdateAdminStats();
                }

                // Обновление корзины
                UpdateCartDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        #region Каталог и фильтрация
        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filteredSouvenirs = allSouvenirs.AsEnumerable();

            // Фильтр по категории
            if (cmbCategories.SelectedItem != null)
            {
                var selectedCategory = (Categories)cmbCategories.SelectedItem;
                filteredSouvenirs = filteredSouvenirs.Where(s => s.CategoryId == selectedCategory.CategoryId);
            }

            // Фильтр по поиску
            if (!string.IsNullOrWhiteSpace(txtSearch.Text) && txtSearch.Text != "Поиск по названию...")
            {
                string searchText = txtSearch.Text.ToLower();
                filteredSouvenirs = filteredSouvenirs.Where(s =>
                    s.Name.ToLower().Contains(searchText) ||
                    (s.Description != null && s.Description.ToLower().Contains(searchText)));
            }

            dgSouvenirs.ItemsSource = filteredSouvenirs.ToList();
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            cmbCategories.SelectedIndex = -1;
            txtSearch.Text = "Поиск по названию...";
            dgSouvenirs.ItemsSource = allSouvenirs;
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var souvenir = (Souvenirs)button.DataContext;

            var existingItem = cartItems.FirstOrDefault(item => item.Souvenir.SouvenirId == souvenir.SouvenirId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cartItems.Add(new CartItem
                {
                    Souvenir = souvenir,
                    Quantity = 1,
                    UnitPrice = souvenir.Price
                });
            }

            UpdateCartDisplay();
            MessageBox.Show($"Товар \"{souvenir.Name}\" добавлен в корзину!");
        }
        #endregion

        #region Корзина
        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if (dgCart.SelectedItem != null)
            {
                var cartItem = (CartItem)dgCart.SelectedItem;
                cartItems.Remove(cartItem);
                UpdateCartDisplay();
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (dgCart.SelectedItem != null)
            {
                var cartItem = (CartItem)dgCart.SelectedItem;

                if (cartItem.Quantity < cartItem.Souvenir.StockQuantity)
                {
                    cartItem.Quantity++;
                    UpdateCartDisplay();
                }
                else
                {
                    MessageBox.Show("Недостаточно товара на складе!");
                }
            }
            else
            {
                MessageBox.Show("Выберите товар из корзины!");
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (dgCart.SelectedItem != null)
            {
                var cartItem = (CartItem)dgCart.SelectedItem;

                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                    UpdateCartDisplay();
                }
                else
                {
                    // Если количество становится 0, удаляем товар из корзины
                    cartItems.Remove(cartItem);
                    UpdateCartDisplay();
                }
            }
            else
            {
                MessageBox.Show("Выберите товар из корзины!");
            }
        }

        private void UpdateCartDisplay()
        {
            dgCart.ItemsSource = null;
            dgCart.ItemsSource = cartItems;
            txtCartCount.Text = cartItems.Sum(item => item.Quantity).ToString();

            decimal total = cartItems.Sum(item => item.TotalPrice);
            txtTotalAmount.Text = $"Общая сумма: {total:C}";
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            mainTabControl.SelectedIndex = 1; // Переход на вкладку корзины
        }

        private void PlaceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста!");
                return;
            }

            PaymentWindow paymentWindow = new PaymentWindow(cartItems.ToList(), NavigationManager.CurrentUser);
            if (paymentWindow.ShowDialog() == true)
            {
                cartItems.Clear();
                UpdateCartDisplay();
                MessageBox.Show("Заказ успешно оформлен!");
            }
        }
        #endregion

        #region Управление товарами (Админ)
        private void AddSouvenir_Click(object sender, RoutedEventArgs e)
        {
            SouvenirEditWindow editWindow = new SouvenirEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadData(); // Перезагружаем данные
            }
        }

        private void EditSouvenir_Click(object sender, RoutedEventArgs e)
        {
            if (dgAdminSouvenirs.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар для редактирования!");
                return;
            }

            var selectedSouvenir = (Souvenirs)dgAdminSouvenirs.SelectedItem;
            SouvenirEditWindow editWindow = new SouvenirEditWindow(selectedSouvenir);
            if (editWindow.ShowDialog() == true)
            {
                LoadData(); // Перезагружаем данные
            }
        }

        private void DeleteSouvenir_Click(object sender, RoutedEventArgs e)
        {
            if (dgAdminSouvenirs.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар для удаления!");
                return;
            }

            var selectedSouvenir = (Souvenirs)dgAdminSouvenirs.SelectedItem;
            var result = MessageBox.Show($"Вы уверены, что хотите удалить товар \"{selectedSouvenir.Name}\"?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Находим товар в контексте
                    var souvenirToDelete = Connection.entities.Souvenirs
                        .FirstOrDefault(s => s.SouvenirId == selectedSouvenir.SouvenirId);

                    if (souvenirToDelete != null)
                    {
                        Connection.entities.Souvenirs.Remove(souvenirToDelete);
                        Connection.entities.SaveChanges();
                        LoadData(); // Перезагружаем данные
                        MessageBox.Show("Товар успешно удален!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                }
            }
        }

        private void RefreshSouvenirs_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void UpdateAdminStats()
        {
            var totalSouvenirs = Connection.entities.Souvenirs.Count();
            var totalCategories = Connection.entities.Categories.Count();
            var lowStock = Connection.entities.Souvenirs.Count(s => s.StockQuantity < 10);

            txtAdminStats.Text = $"Всего товаров: {totalSouvenirs} | Категорий: {totalCategories} | Мало на складе (<10): {lowStock}";
        }
        #endregion

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.CurrentUser = null;
            NavigationManager.ShowLoginWindow();
            this.Close();
        }

        // Класс для элемента корзины
        public class CartItem
        {
            public Souvenirs Souvenir { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice => Quantity * UnitPrice;
        }
    }
}

