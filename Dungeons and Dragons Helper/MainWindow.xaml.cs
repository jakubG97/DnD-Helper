using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dungeons_and_Dragons_Helper.Utilities;
using log4net;

namespace Dungeons_and_Dragons_Helper
{
    public partial class MainWindow : Window
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SqLiteUtil SQL = null;
        private MainUtility Util = null;

        public MainWindow()
        {
            Log.Info("======================= Otwieranie nowej instancji =======================");
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
            using (SQLiteCommand cmd = new SQLiteCommand())
            {
                cmd.Connection = Util.SQL.dbConnection;
                SQLiteHelper sh = new SQLiteHelper(cmd);
                String query =
                    $"SELECT * FROM klasa JOIN klasa_rasa r on klasa.id = r.klasa_id where rasa_id = {rassName.SelectedValue ?? 0};";
                var ret = sh.Select(query, new Dictionary<string, object>());
                className.ItemsSource = ret?.DefaultView;
            }
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
            var base_size = (Int64) (((DataRowView) rassName.SelectedItem)["rozmiar_bazowy"] ?? 2);
            if (base_size == 1)
            {
                ret.DefaultView.Delete(2);
            }
            else if (base_size == 3) ret.DefaultView.Delete(0);

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
            LoadRzutyObronne();
            LoadAtak();
            LoadBron();
        }

        private void LoadBron()
        {
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = Util.SQL.dbConnection;
                    SQLiteHelper sh = new SQLiteHelper(cmd);
                    String query =
                        $"SELECT bron.*,r.nazwa as 'rodzaj_nazwa' FROM bron JOIN rodzaj r on bron.rodzaj_id = r.id WHERE klasa_id = {className.SelectedValue ?? 0};";
                    var ret = sh.Select(query, new Dictionary<string, object>());
                    ret?.DefaultView.AddNew();
                    Bron1.ItemsSource = ret?.DefaultView;
                }
            }

            catch (Exception)
            {
            }
        }

        private void LoadAtak()
        {
            try
            {
                var lv = (Int64) (lvl.SelectedIndex + 1);
                var atak = Util.GetAllFromTable("bazowa_premia_do_ataku",
                    new Dictionary<string, object>()
                    {
                        {
                            "grupa_id",
                            (Int64) (((DataRowView) className.SelectedItem)["bazowa_premia_do_ataku_grupa_id"])
                        },
                        {
                            "poziom", (Int64) (lvl.SelectedIndex + 1)
                        }
                    });
                if (atak == null)
                {
                    BazowaPremiaAtakuWrecz.Text = "0";
                    BazowaPremiaAtakuDystans.Text = "0";
                    return;
                }

                foreach (DataRow atakRow in atak.Rows)
                {
                    BazowaPremiaAtakuWrecz.Text = ((Int64) atakRow["premia1"]).ToString();
                    BazowaPremiaAtakuDystans.Text = ((Int64) atakRow["premia1"]).ToString();
                }
            }

            catch (Exception)
            {
            }
        }

        private void LoadRzutyObronne()
        {
            try
            {
                if (className.SelectedValue == null)
                {
                    BazowyRzutObronnyWytrwalosc.Text = "0";
                    BazowyRzutObronnyRefleks.Text = "0";
                    BazowyRzutObronnyWola.Text = "0";
                }
                else
                {
                    var rzuty_obronne = Util.GetAllFromTable("rzuty_obronne",
                        new Dictionary<string, object>()
                        {
                            {"klasa_id", className.SelectedValue},
                            {"poziom", lvl.SelectedIndex + 1}
                        });
                    foreach (DataRow dataRow in rzuty_obronne.Rows)
                    {
                        if ((string) dataRow["nazwa"] == "Wytrwałość")
                        {
                            BazowyRzutObronnyWytrwalosc.Text = ((Int64) dataRow["bazowy_rzut_obronny"]).ToString();
                        }
                        else if ((string) dataRow["nazwa"] == "Refleks")
                        {
                            BazowyRzutObronnyRefleks.Text = ((Int64) dataRow["bazowy_rzut_obronny"]).ToString();
                        }
                        else if ((string) dataRow["nazwa"] == "Wola")
                        {
                            BazowyRzutObronnyWola.Text = ((Int64) dataRow["bazowy_rzut_obronny"]).ToString();
                        }
                    }
                }
            }
            catch (Exception)

            {
            }
        }

        private void rassName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadClasses();
            age_TextChanged(null, null);
            height_TextChanged(null, null);
            weight_TextChanged(null, null);
            LoadSizes();
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
            ForceAttributeSize((TextBox) sender);
            RecalculateAttributes();
        }

        private void ForceAttributeSize(TextBox input)
        {
            var value = input.Text.Trim();
            if (value.Equals(""))
            {
                value = "1";
            }

            var intVal = Int64.Parse(value);
            if (intVal > 25)
            {
                input.Text = "25";
                input.CaretIndex = input.Text.Length;
            }

            if (intVal < 1)
            {
                input.Text = "1";
                input.CaretIndex = input.Text.Length;
            }
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
            try
            {
                ModyfZrecz2.Text = GetAttributeModificatorTextFieldById(2).Content.ToString();
                ModyfikatorZAtrybutuWytrwalosc.Text = GetAttributeModificatorTextFieldById(3).Content.ToString();
                ModyfikatorZAtrybutuRefleks.Text = GetAttributeModificatorTextFieldById(2).Content.ToString();
                ModyfikatorZAtrybutuWola.Text = GetAttributeModificatorTextFieldById(5).Content.ToString();
                ModyfikatorSilyAtakWrecz.Text = GetAttributeModificatorTextFieldById(1).Content.ToString();
                ModyfikatorZreczAtakDystans.Text = GetAttributeModificatorTextFieldById(2).Content.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
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
            KpSum.Text = (10 +
                          GetIntegerSumOfTextFields(new List<TextBox>()
                          {
                              PremiaZPancerza,
                              PremiaZTarczy,
                              ModyfRozm1,
                              ModyfRozmaite2
                          }))
                .ToString();
        }

        private void size_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecalculateSize();
        }

        private void RecalculateSize()
        {
            try
            {
                var selectedSize = Rozmiar.getRozmiarById(size.SelectedIndex + 1);
                ModyfRozm1.Text = ((int) selectedSize).ToString();
                ModyfikatorRozmiaruAtakDystans.Text = ((int) selectedSize).ToString();
                ModyfikatorRozmiaruAtakWrecz.Text = ((int) selectedSize).ToString();
            }
            catch (Exception)
            {
                ModyfRozm1.Text = "0";
                ModyfikatorRozmiaruAtakDystans.Text = "0";
                ModyfikatorRozmiaruAtakWrecz.Text = "0";
            }
        }

        private void RecalculateInicjatywa(object sender, TextChangedEventArgs e)
        {
            InicjatywaSum.Text =
                GetIntegerSumOfTextFields(new List<TextBox>()
                    {
                        ModyfZrecz2,
                        ModyfRozmaite1
                    })
                    .ToString();
        }

        private void WyrwaloscRecalculateSum(object sender, TextChangedEventArgs e)
        {
            WytrwaloscSum.Text =
                GetIntegerSumOfTextFields(new List<TextBox>()
                    {
                        BazowyRzutObronnyWytrwalosc,
                        ModyfikatorZAtrybutuWytrwalosc,
                        ModyfikatorZMagiiWytrwalosc,
                        RozmaiteModyfikatoryWytrwalosc
                    })
                    .ToString();
        }

        private void RefleksRecalculate(object sender, TextChangedEventArgs e)
        {
            RefleksSum.Text =
                GetIntegerSumOfTextFields(new List<TextBox>()
                    {
                        BazowyRzutObronnyRefleks,
                        ModyfikatorZAtrybutuRefleks,
                        ModyfikatorZMagiiRefleks,
                        RozmaiteModyfikatoryRefleks
                    })
                    .ToString();
        }

        private void WolaRecalculate(object sender, TextChangedEventArgs e)
        {
            WolaSum.Text =
                GetIntegerSumOfTextFields(new List<TextBox>()
                    {
                        BazowyRzutObronnyWola,
                        ModyfikatorZAtrybutuWola,
                        ModyfikatorZMagiiWola,
                        RozmaiteModyfikatoryWola
                    })
                    .ToString();
        }

        private void RecalculateAtakWrecz(object sender, TextChangedEventArgs e)
        {
            AtakWreczSum.Text =
                GetIntegerSumOfTextFields(new List<TextBox>()
                    {
                        BazowaPremiaAtakuWrecz,
                        ModyfikatorSilyAtakWrecz,
                        ModyfikatorRozmiaruAtakWrecz,
                        RozmaiteModyfikatoryAtakWrecz
                    })
                    .ToString();
        }

        private void RecalculateAtakDystans(object sender, TextChangedEventArgs e)
        {
            AtakDystansSum.Text =
                GetIntegerSumOfTextFields(new List<TextBox>()
                    {
                        BazowaPremiaAtakuDystans,
                        ModyfikatorZreczAtakDystans,
                        ModyfikatorRozmiaruAtakDystans,
                        RozmaiteModyfikatoryAtakDystans
                    })
                    .ToString();
        }

        private int GetIntegerSumOfTextFields(List<TextBox> list)
        {
            var sum = 0;
            foreach (var textBox in list)
            {
                try
                {
                    if (Int32.TryParse(textBox.Text, out int output)) sum += output;
                }
                catch (Exception)
                {
                }
            }

            return sum;
        }

        private void Lvl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAtak();
        }

        private void Bron_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox) sender;
            var currentItem = ((ComboBox) sender).SelectedItem;
            if (currentItem != null)
            {
                try
                {
                    GetControlByName<TextBox>(comboBox.Name + "PremiaDoAtaku").Text =
                        ((Int64) (((DataRowView) currentItem)["premia_do_ataku"] ?? 0)).ToString();
                    GetControlByName<TextBox>(comboBox.Name + "Obrazenia").Text =
                        ((Int64) (((DataRowView) currentItem)["obrazenia"] ?? 0)).ToString();
                    GetControlByName<TextBox>(comboBox.Name + "Krytyk").Text =
                        ((Int64) (((DataRowView) currentItem)["krytyk"] ?? 0)).ToString();
                    GetControlByName<TextBox>(comboBox.Name + "Zasieg").Text =
                        ((Int64) (((DataRowView) currentItem)["zasieg"] ?? 0)).ToString();
                    GetControlByName<TextBox>(comboBox.Name + "Rodzaj").Text =
                        (String) (((DataRowView) currentItem)["rodzaj_nazwa"] ?? "");
                }
                catch (Exception)
                {
                    ClearBron(comboBox.Name);
                }
            }
            else
            {
                ClearBron(comboBox.Name);
            }
        }

        private void ClearBron(string index)
        {
            GetControlByName<TextBox>(index + "PremiaDoAtaku").Text = "0";
            GetControlByName<TextBox>(index + "Obrazenia").Text = "0";
            GetControlByName<TextBox>(index + "Krytyk").Text = "0";
            GetControlByName<TextBox>(index + "Zasieg").Text = "0";
            GetControlByName<TextBox>(index + "Rodzaj").Text = "";
            GetControlByName<TextBox>(index + "Specjalne").Text = "";
        }

        private T GetControlByName<T>(string name)
        {
            T obj = (T) FindName(name);
            return obj;
        }
    }
}