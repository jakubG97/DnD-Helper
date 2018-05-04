using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Dungeons_and_Dragons_Helper.Models;
using Dungeons_and_Dragons_Helper.Utilities;
using log4net;

namespace Dungeons_and_Dragons_Helper
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SqLiteUtil SQL = null;
        private MainUtility Util = null;

        public MainWindow()
        {
            Log.Info("=======================Otwieranie nowej instancji =======================");
            InitializeComponent();
            SQL = new SqLiteUtil();
            Util = new MainUtility(SQL);
            LoadDataToUI();
        }

        private void LoadDataToUI()
        {
            LoadRaces();
            LoadClasses();
            LoadCharacters();
            LoadDeity();
            LoadLvls();
            LoadSizes();
        }

        private void LoadClasses()
        {
            var ret = Util.GetAllFromTable("klasa",
                new Dictionary<string, object>() {{"rasa_id", rassName.SelectedValue}});
            className.ItemsSource = ret?.DefaultView;
        }

        private void LoadCharacters()
        {
            var ret = Util.GetAllFromTable("charakter",
                new Dictionary<string, object>() {{"klasa_id", className.SelectedValue}});
            characterName.ItemsSource = ret?.DefaultView;
        }

        private void LoadDeity()
        {
            var ret = Util.GetAllFromTable("bostwo",
                new Dictionary<string, object>() {{"klasa_id", className.SelectedValue}});
            deityName.ItemsSource = ret?.DefaultView;
        }

        private void LoadRaces()
        {
            var ret = Util.GetAllFromTable("rasa");
            rassName.ItemsSource = ret?.DefaultView;
        }

        private void LoadSizes()
        {
            var ret = Util.GetAllFromTable("rozmiar");
            size.ItemsSource = ret?.DefaultView;
        }

        private void LoadLvls()
        {
            DataTable lvls = new DataTable();
            lvls.Columns.Add("lvl");
            foreach (var i in Enumerable.Range(1, 19))
            {
                lvls.Rows.Add(i);
            }

            lvl.ItemsSource = lvls.DefaultView;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[^0-9]+$");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void className_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadCharacters();
            LoadDeity();
        }

        private void rassName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadClasses();
            age_TextChanged(null, null);
            height_TextChanged(null, null);
            weight_TextChanged(null, null);
        }

        private void age_TextChanged(object sender, TextChangedEventArgs e)
        {
            var value = age.Text.Trim();
            if (value.Equals(""))
            {
                value = "1";
            }

            var intVal = Int64.Parse(value);
            var maxVal = (Int64) (((DataRowView) rassName.SelectedItem)["wiek_do"] ?? 100);
            if (intVal > maxVal)
            {
                age.Text = maxVal.ToString();
                age.CaretIndex = age.Text.Length;
            }

            if (intVal < 1)
            {
                age.Text = "1";
                age.CaretIndex = age.Text.Length;
            }
        }

        private void height_TextChanged(object sender, TextChangedEventArgs e)
        {
            var value = height.Text.Trim();
            if (value.Equals(""))
            {
                value = "1";
            }

            var intVal = Int64.Parse(value);
            var maxVal = (Int64) (((DataRowView) rassName.SelectedItem)["wzrost_do"] ?? 100);
            if (intVal > (int) maxVal)
            {
                height.Text = ((int) maxVal).ToString();
                height.CaretIndex = height.Text.Length;
            }

            if (intVal < 1)
            {
                height.Text = "1";
                height.CaretIndex = height.Text.Length;
            }
        }

        private void weight_TextChanged(object sender, TextChangedEventArgs e)
        {
            var value = weight.Text.Trim();
            if (value.Equals(""))
            {
                value = "1";
            }

            var intVal = Int64.Parse(value);
            var maxVal = (Int64) (((DataRowView) rassName.SelectedItem)["waga_do"] ?? 100);
            if (intVal > (int) maxVal)
            {
                weight.Text = maxVal.ToString();
                weight.CaretIndex = weight.Text.Length;
            }

            if (intVal < 1)
            {
                weight.Text = "1";
                weight.CaretIndex = weight.Text.Length;
            }
        }

       
    }
}