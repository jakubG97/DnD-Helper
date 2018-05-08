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
            RecalculateSize();
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

        private void AttributesTextField_TextChanged(object sender, TextChangedEventArgs e)
        {
            RecalculateAttributes();
        }

        private void RecalculateAttributes()
        {
            var attributes = Util.GetAllAttributes();
            foreach (var i in Enumerable.Range(1, 6))
            {
                GetAttributeModificatorTextFieldById(i).Content = "0";
            }

            foreach (DataRow attribute in attributes.Rows)
            {
                var attributeTextField = GetAttributeTextFieldById((Int64) attribute["atrybut_id"]);
                var modificatorTextField = GetAttributeModificatorTextFieldById((Int64) attribute["atrybut_id"]);
                var enteredText = attributeTextField.Text == "" ? "0" : attributeTextField.Text;
                if (Int64.Parse(enteredText) == (Int64) attribute["wartosc"])
                {
                    modificatorTextField.Content = ((Int64) attribute["modyfikator"]).ToString();
                }
            }

            SpreadRecalculatedAttributes();
        }

        private void SpreadRecalculatedAttributes()
        {
            ModyfZrecz2.Text = GetAttributeModificatorTextFieldById(2).Content.ToString();
        }

        private Label GetAttributeModificatorTextFieldById(Int64 id)
        {
            switch (id)
            {
                case 1: return StrengthModificatorValue;
                case 2: return SkillModificatorValue;
                case 3: return BuildModificatorValue;
                case 4: return IntellectModificatorValue;
                case 5: return CautionModificatorValue;
                case 6: return CharismaModificatorValue;
            }

            return null;
        }

        private TextBox GetAttributeTextFieldById(Int64 id)
        {
            switch (id)
            {
                case 1: return StrengthValue;
                case 2: return SkillValue;
                case 3: return BuildValue;
                case 4: return IntellectValue;
                case 5: return CautionValue;
                case 6: return CharismaValue;
            }

            return null;
        }

        private void KP_Recalculate(object sender, TextChangedEventArgs e)
        {
            var PremiaZPancerzaInteger = Int32.Parse(PremiaZPancerza.Text == "" ? "0" : PremiaZPancerza.Text);
            var PremiaZTarczyInteger = Int32.Parse(PremiaZTarczy.Text == "" ? "0" : PremiaZTarczy.Text);
            var ModyfRozm1Integer = Int32.Parse(ModyfRozm1.Text == "" ? "0" : ModyfRozm1.Text);
            var ModyfRozmaite2Integer = Int32.Parse(ModyfRozmaite2.Text == "" ? "0" : ModyfRozmaite2.Text);
            KpSum.Text = (10 + PremiaZPancerzaInteger + PremiaZTarczyInteger + ModyfRozm1Integer + ModyfRozmaite2Integer).ToString();
        }

        private void size_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecalculateSize();
        }

        private void RecalculateSize()
        {
            try
            {
                var baseSize = (Int64) (((DataRowView) rassName.SelectedItem)["rozmiar_bazowy"] ?? 0);
                ModyfRozm1.Text = baseSize.ToString();
            }
            catch (Exception e)
            {
                ModyfRozm1.Text = "0";
            }
        }

        private void RecalculateInicjatywa(object sender, TextChangedEventArgs e)
        {
            try
            {
                var ModyfZrecz2Integer = Int32.Parse(ModyfZrecz2.Text == "" ? "0" : ModyfZrecz2.Text);
                var ModyfRozmaite1Integer = Int32.Parse(ModyfRozmaite1.Text == "" ? "0" : ModyfRozmaite1.Text);
                InicjatywaSum.Text = (ModyfRozmaite1Integer + ModyfZrecz2Integer).ToString();
            }
            catch (Exception ignored)
            {

            }
        }
    }
}