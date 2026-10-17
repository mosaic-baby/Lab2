using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;


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

namespace Lab2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Создаем списки для картинок и врагов
        public Dictionary<string, string> AvailableIcons { get; set; } = new Dictionary<string, string>();
        public System.Collections.ObjectModel.ObservableCollection<Enemy> EnemiesList { get; set; } = new System.Collections.ObjectModel.ObservableCollection<Enemy>();

        public MainWindow()
        {
            InitializeComponent();
            LoadAvailableIcons();
            ListBoxEnemies.ItemsSource = EnemiesList; // Связываем левый список с врагами
        }

        // 1. Автоматическая загрузка картинок 1, 2, 3, 4 при старте
        private void LoadAvailableIcons()
        {

            string imagesPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

            if (Directory.Exists(imagesPath))
            {
                var files = Directory.GetFiles(imagesPath, "*.jpg");
                foreach (var file in files)
                {

                    string filename = System.IO.Path.GetFileNameWithoutExtension(file);

                    AvailableIcons.Add(filename, file);
                }
            }
            ListBoxIcons.ItemsSource = AvailableIcons;
        }

        // 2. Клик по картинке в правом списке — вывод её на экран по центру
        private void ListBoxIcons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxIcons.SelectedItem is KeyValuePair<string, string> selectedIcon)
            {
                TxtIconName.Text = selectedIcon.Key;
                ImgEnemyIcon.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(selectedIcon.Value));
            }
        }

        // 3. Кнопка ADD — добавление нового врага в список слева
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtEnemyName.Text)) return;

            // Заставляем программу понимать и точку, и запятую при вводе
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ".";

            // Если пользователь ввел запятую, временно заменим её на точку для одинаковой обработки
            string lifeModText = TxtLifeModifier.Text.Replace(',', '.');
            string goldModText = TxtGoldModifier.Text.Replace(',', '.');

            Enemy newEnemy = new Enemy
            {
                Name = TxtEnemyName.Text,
                IconName = TxtIconName.Text,
                BaseLife = int.TryParse(TxtBaseLife.Text, out int bl) ? bl : 0,
                BaseGold = int.TryParse(TxtBaseGold.Text, out int bg) ? bg : 0,
                // Используем культуру с точкой для безопасного чтения
                LifeModifier = double.TryParse(lifeModText, NumberStyles.Any, culture, out double lm) ? lm : 0,
                GoldModifier = double.TryParse(goldModText, NumberStyles.Any, culture, out double gm) ? gm : 0,
                SpawnChance = int.TryParse(TxtSpawnChance.Text, out int sc) ? sc : 0
            };

            EnemiesList.Add(newEnemy);
        }

        // 4. Кнопка REMOVE — удаление выбранного врага
        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxEnemies.SelectedItem is Enemy selectedEnemy)
            {
                EnemiesList.Remove(selectedEnemy);
            }
        }

        // 5. Клик по врагу в левом списке — показать все его параметры по центру
        private void ListBoxEnemies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxEnemies.SelectedItem is Enemy selectedEnemy)
            {
                TxtEnemyName.Text = selectedEnemy.Name;
                TxtIconName.Text = selectedEnemy.IconName;
                TxtBaseLife.Text = selectedEnemy.BaseLife.ToString();
                TxtBaseGold.Text = selectedEnemy.BaseGold.ToString();
                TxtLifeModifier.Text = selectedEnemy.LifeModifier.ToString();
                TxtGoldModifier.Text = selectedEnemy.GoldModifier.ToString();
                TxtSpawnChance.Text = selectedEnemy.SpawnChance.ToString();

                if (AvailableIcons.ContainsKey(selectedEnemy.IconName))
                {
                    ImgEnemyIcon.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(AvailableIcons[selectedEnemy.IconName]));
                }
            }
        }

        // 6. Кнопка SAVE — сохранение всего списка в текстовый файл
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog { Filter = "JSON файлы (*.json)|*.json" };
            if (dialog.ShowDialog() == true)
            {
                List<string> lines = new List<string>();
                foreach (var enemy in EnemiesList)
                {

                    lines.Add($"{enemy.Name};{enemy.IconName};{enemy.BaseLife};{enemy.BaseGold};{enemy.LifeModifier.ToString(CultureInfo.InvariantCulture)};{enemy.GoldModifier.ToString(CultureInfo.InvariantCulture)};{enemy.SpawnChance}");
                }
                System.IO.File.WriteAllLines(dialog.FileName, lines);
            }
        }

        // 7. Кнопка LOAD — загрузка списка обратно в программу
        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "JSON файлы (*.json)|*.json" };
            if (dialog.ShowDialog() == true)
            {
                EnemiesList.Clear();
                var lines = System.IO.File.ReadAllLines(dialog.FileName);
                foreach (var line in lines)
                {
                    var parts = line.Split(';');
                    if (parts.Length < 7) continue;


                    Enemy enemy = new Enemy
                    {
                        Name = parts[0],
                        IconName = parts[1],
                        BaseLife = int.Parse(parts[2]),
                        BaseGold = int.Parse(parts[3]),
                        LifeModifier = double.Parse(parts[4], CultureInfo.InvariantCulture),
                        GoldModifier = double.Parse(parts[5], CultureInfo.InvariantCulture),
                        SpawnChance = int.Parse(parts[6])
                    };
                    EnemiesList.Add(enemy);
                }
            }
        }

        // Класс описания противника
        public class Enemy
        {
            public string Name { get; set; }
            public string IconName { get; set; }
            public int BaseLife { get; set; }
            public int BaseGold { get; set; }
            public double LifeModifier { get; set; }
            public double GoldModifier { get; set; }
            public int SpawnChance { get; set; }

            public override string ToString() => Name;
        }
    }
}