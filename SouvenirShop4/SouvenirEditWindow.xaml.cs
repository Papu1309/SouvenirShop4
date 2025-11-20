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
    /// Логика взаимодействия для SouvenirEditWindow.xaml
    /// </summary>
    public partial class SouvenirEditWindow : Window
    {
        private Souvenirs currentSouvenir;
        private bool isEditMode = false;

        public SouvenirEditWindow()
        {
            InitializeComponent();
            LoadCategories();
            this.Title = "Добавление товара";
        }

        public SouvenirEditWindow(Souvenirs souvenir) : this()
        {
            currentSouvenir = souvenir;
            isEditMode = true;
            this.Title = "Редактирование товара";

            // Заполняем поля данными
            txtName.Text = souvenir.Name;
            txtDescription.Text = souvenir.Description;
            txtPrice.Text = souvenir.Price.ToString("0.00");
            txtStockQuantity.Text = souvenir.StockQuantity.ToString();

            if (souvenir.CategoryId.HasValue)
            {
                var categories = Connection.entities.Categories.ToList();
                var selectedCategory = categories.FirstOrDefault(c => c.CategoryId == souvenir.CategoryId.Value);
                if (selectedCategory != null)
                {
                    cmbCategories.SelectedItem = selectedCategory;
                }
            }
        }

        private void LoadCategories()
        {
            try
            {
                var categories = Connection.entities.Categories.ToList();
                cmbCategories.ItemsSource = categories;

                if (categories.Count > 0 && !isEditMode)
                {
                    cmbCategories.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите название товара!");
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
                {
                    MessageBox.Show("Введите корректную цену!");
                    return;
                }

                if (!int.TryParse(txtStockQuantity.Text, out int quantity) || quantity < 0)
                {
                    MessageBox.Show("Введите корректное количество!");
                    return;
                }

                if (cmbCategories.SelectedItem == null)
                {
                    MessageBox.Show("Выберите категорию!");
                    return;
                }

                var selectedCategory = (Categories)cmbCategories.SelectedItem;

                // Сохранение
                if (isEditMode)
                {
                    // Редактирование существующего товара
                    currentSouvenir.Name = txtName.Text.Trim();
                    currentSouvenir.Description = txtDescription.Text.Trim();
                    currentSouvenir.Price = price;
                    currentSouvenir.StockQuantity = quantity;
                    currentSouvenir.CategoryId = selectedCategory.CategoryId;
                }
                else
                {
                    // Добавление нового товара
                    Souvenirs newSouvenir = new Souvenirs
                    {
                        Name = txtName.Text.Trim(),
                        Description = txtDescription.Text.Trim(),
                        Price = price,
                        StockQuantity = quantity,
                        CategoryId = selectedCategory.CategoryId
                    };
                    Connection.entities.Souvenirs.Add(newSouvenir);
                }

                Connection.entities.SaveChanges();
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }
    }
}
