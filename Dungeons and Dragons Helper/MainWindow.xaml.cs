using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Dungeons_and_Dragons_Helper.Utilities;
using log4net;
using log4net.Repository.Hierarchy;

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
            LoadSpeed();
            LoadCharacters();
            LoadDeity();
            LoadRzutyObronne();
            LoadAtak();
            LoadBron();
            LoadPrzedmiotyOchronne();
        }

        private void LoadPrzedmiotyOchronne()
        {
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = Util.SQL.dbConnection;
                    SQLiteHelper sh = new SQLiteHelper(cmd);
                    String query =
                        $"SELECT przedmioty_ochronne.*, kp.nazwa as 'kategoria_nazwa', kp.dwureczna, modyfikator as modyfikator_szybkosci FROM przedmioty_ochronne JOIN kategorie_przedmiotow kp on przedmioty_ochronne.kategorie_przedmiotow_id = kp.id JOIN przedmioty_ochronne_modyfikator_szybkosci poms on przedmioty_ochronne.id = poms.przedmiot_ochronny_id WHERE kategorie_przedmiotow_id =6 AND klasa_id = {className.SelectedValue ?? 0} AND rasa_id = {rassName.SelectedValue ?? 0};";
                    var ret = sh.Select(query, new Dictionary<string, object>());
                    ret?.DefaultView.AddNew();
                    Pancerz.ItemsSource = ret?.DefaultView;
                }

                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = Util.SQL.dbConnection;
                    SQLiteHelper sh = new SQLiteHelper(cmd);
                    String query =
                        $"SELECT przedmioty_ochronne.*, kp.nazwa as 'kategoria_nazwa', kp.dwureczna, modyfikator as modyfikator_szybkosci FROM przedmioty_ochronne JOIN kategorie_przedmiotow kp on przedmioty_ochronne.kategorie_przedmiotow_id = kp.id JOIN przedmioty_ochronne_modyfikator_szybkosci poms on przedmioty_ochronne.id = poms.przedmiot_ochronny_id WHERE kategorie_przedmiotow_id =7 AND klasa_id = {className.SelectedValue ?? 0} AND rasa_id = {rassName.SelectedValue ?? 0};";
                    var ret = sh.Select(query, new Dictionary<string, object>());
                    ret?.DefaultView.AddNew();
                    Tarcza.ItemsSource = ret?.DefaultView;
                }
            }

            catch (Exception)
            {
            }
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
                        $"SELECT b.id, b.nazwa|| ' - ' || r2.nazwa as nazwa, b.klasa_id, b.obrazenia, b.krytyk, r.nazwa AS kategoria_nazwa, r.dwureczna FROM bron b JOIN kategorie_przedmiotow r on b.kategorie_przedmiotow_id = r.id JOIN rozmiar r2 on b.rozmiar_id = r2.id WHERE klasa_id = {className.SelectedValue ?? 0} AND b.rozmiar_id IN(SELECT rozmiar_id FROM rozmiar_broni_rasa WHERE rasa_id = {rassName.SelectedValue ?? 0}) AND b.kategorie_przedmiotow_id IN(1,2);";
                    var ret = sh.Select(query, new Dictionary<string, object>());
                    ret?.DefaultView.AddNew();
                    Bron1.ItemsSource = ret?.DefaultView;
                    Bron2.ItemsSource = ret?.DefaultView;

                    query =
                        $"SELECT b.id, b.nazwa|| ' - ' || r2.nazwa as nazwa, b.klasa_id, b.obrazenia, b.krytyk, r.nazwa AS kategoria_nazwa, r.dwureczna FROM bron b JOIN kategorie_przedmiotow r on b.kategorie_przedmiotow_id = r.id JOIN rozmiar r2 on b.rozmiar_id = r2.id WHERE klasa_id = {className.SelectedValue ?? 0} AND b.rozmiar_id IN(SELECT rozmiar_id FROM rozmiar_broni_rasa WHERE rasa_id = {rassName.SelectedValue ?? 0}) AND b.kategorie_przedmiotow_id = 4;";
                    ret = sh.Select(query, new Dictionary<string, object>());
                    ret?.DefaultView.AddNew();
                    Bron3.ItemsSource = ret?.DefaultView;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
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
                if (atak?.Rows == null || atak.Rows.Count == 0)
                {
                    throw new Exception("Database does not contain data for this configuration");
                }

                foreach (DataRow atakRow in atak.Rows)
                {
                    BazowaPremiaAtakuWrecz1.Text = ((Int64) atakRow["premia1"]).ToString();
                    BazowaPremiaAtakuWrecz2.Text = ((Int64) atakRow["premia2"]).ToString();
                    BazowaPremiaAtakuWrecz3.Text = ((Int64) atakRow["premia3"]).ToString();
                    BazowaPremiaAtakuWrecz4.Text = ((Int64) atakRow["premia4"]).ToString();
                    BazowaPremiaAtakuDystans1.Text = ((Int64) atakRow["premia1"]).ToString();
                    BazowaPremiaAtakuDystans2.Text = ((Int64) atakRow["premia2"]).ToString();
                    BazowaPremiaAtakuDystans3.Text = ((Int64) atakRow["premia3"]).ToString();
                    BazowaPremiaAtakuDystans4.Text = ((Int64) atakRow["premia4"]).ToString();
                }
            }

            catch (Exception ex)
            {
                BazowaPremiaAtakuWrecz1.Text = "0";
                BazowaPremiaAtakuWrecz2.Text = "0";
                BazowaPremiaAtakuWrecz3.Text = "0";
                BazowaPremiaAtakuWrecz4.Text = "0";
                BazowaPremiaAtakuDystans1.Text = "0";
                BazowaPremiaAtakuDystans2.Text = "0";
                BazowaPremiaAtakuDystans3.Text = "0";
                BazowaPremiaAtakuDystans4.Text = "0";
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
            LoadSpeed();
            LoadRozmaiteModyfikatoryRzutyObronne();
        }

        private void LoadRozmaiteModyfikatoryRzutyObronne()
        {
            if ((Int64) (((DataRowView) rassName.SelectedItem)["id"] ?? 0) == 2)
            {
                RozmaiteModyfikatoryWytrwalosc.Text = "1";
                RozmaiteModyfikatoryRefleks.Text = "1";
                RozmaiteModyfikatoryWola.Text = "1";
            }
            else
            {
                RozmaiteModyfikatoryWytrwalosc.Text = "0";
                RozmaiteModyfikatoryRefleks.Text = "0";
                RozmaiteModyfikatoryWola.Text = "0";
            }
        }

        private void LoadSpeed()
        {
            Double speed = 0.0;
            try
            {
                speed += (Int64) (((DataRowView) rassName.SelectedItem)["szybkosc"] ?? 0);
                if (className.SelectedItem != null)
                {
                    speed += (Int64) (((DataRowView) className.SelectedItem)["modyfikator_szybkosci"] ?? 0);
                }

                if (PancerzSzybkość.Text != "")
                {
                    if (Double.TryParse(PancerzSzybkość.Text.Replace(".", ","), out double s))
                    {
                        speed += s;
                    }
                }

                if (TarczaSzybkość.Text != "")
                {
                    if (Double.TryParse(TarczaSzybkość.Text.Replace(".", ","), out double s))
                    {
                        speed += s;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            Szybkosc.Text = speed.ToString(CultureInfo.InvariantCulture);
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
                    var new_mod = ((Int64) attribute["modyfikator"]);
                    if (modificatorTextField.Name == "SkillModificatorValue")
                    {
                        var max = GetMaxSkillModifier();

                        modificatorTextField.Content = new_mod > max ? max.ToString() : new_mod.ToString();
                    }
                    else
                    {
                        modificatorTextField.Content = new_mod.ToString();
                    }
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
                RefreshObrazeniaWrecz(Bron1);
                RefreshObrazeniaWrecz(Bron2);
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
                var selectedSize = GetSizeModifier();
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
            if (BazowaPremiaAtakuWrecz1 != null)
                AtakWreczSum1.Text = BazowaPremiaAtakuWrecz1.Text != "0"
                    ? GetIntegerSumOfTextFields(new List<TextBox>()
                        {
                            BazowaPremiaAtakuWrecz1,
                            ModyfikatorSilyAtakWrecz,
                            ModyfikatorRozmiaruAtakWrecz,
                            RozmaiteModyfikatoryAtakWrecz
                        })
                        .ToString()
                    : "0";
            if (BazowaPremiaAtakuWrecz2 != null)
                AtakWreczSum2.Text = BazowaPremiaAtakuWrecz2.Text != "0"
                    ? GetIntegerSumOfTextFields(new List<TextBox>()
                        {
                            BazowaPremiaAtakuWrecz2,
                            ModyfikatorSilyAtakWrecz,
                            ModyfikatorRozmiaruAtakWrecz,
                            RozmaiteModyfikatoryAtakWrecz
                        })
                        .ToString()
                    : "0";
            if (BazowaPremiaAtakuWrecz3 != null)
                AtakWreczSum3.Text = BazowaPremiaAtakuWrecz3.Text != "0"
                    ? AtakWreczSum3.Text =
                        GetIntegerSumOfTextFields(new List<TextBox>()
                            {
                                BazowaPremiaAtakuWrecz3,
                                ModyfikatorSilyAtakWrecz,
                                ModyfikatorRozmiaruAtakWrecz,
                                RozmaiteModyfikatoryAtakWrecz
                            })
                            .ToString()
                    : "0";
            if (BazowaPremiaAtakuWrecz4 != null)
                AtakWreczSum4.Text = BazowaPremiaAtakuWrecz4.Text != "0"
                    ? AtakWreczSum4.Text =
                        GetIntegerSumOfTextFields(new List<TextBox>()
                            {
                                BazowaPremiaAtakuWrecz4,
                                ModyfikatorSilyAtakWrecz,
                                ModyfikatorRozmiaruAtakWrecz,
                                RozmaiteModyfikatoryAtakWrecz
                            })
                            .ToString()
                    : "0";
        }

        private void RecalculateAtakDystans(object sender, TextChangedEventArgs e)
        {
            if (BazowaPremiaAtakuDystans1 != null)
                AtakDystansSum1.Text = BazowaPremiaAtakuDystans1.Text != "0"
                    ? GetIntegerSumOfTextFields(new List<TextBox>()
                        {
                            BazowaPremiaAtakuDystans1,
                            ModyfikatorZreczAtakDystans,
                            ModyfikatorRozmiaruAtakDystans,
                            RozmaiteModyfikatoryAtakDystans
                        })
                        .ToString()
                    : "0";
            if (BazowaPremiaAtakuDystans2 != null)
                AtakDystansSum2.Text = BazowaPremiaAtakuDystans2.Text != "0"
                    ? GetIntegerSumOfTextFields(new List<TextBox>()
                        {
                            BazowaPremiaAtakuDystans2,
                            ModyfikatorZreczAtakDystans,
                            ModyfikatorRozmiaruAtakDystans,
                            RozmaiteModyfikatoryAtakDystans
                        })
                        .ToString()
                    : "0";

            if (BazowaPremiaAtakuDystans3 != null)
                AtakDystansSum3.Text = BazowaPremiaAtakuDystans3.Text != "0"
                    ? GetIntegerSumOfTextFields(new List<TextBox>()
                        {
                            BazowaPremiaAtakuDystans3,
                            ModyfikatorZreczAtakDystans,
                            ModyfikatorRozmiaruAtakDystans,
                            RozmaiteModyfikatoryAtakDystans
                        })
                        .ToString()
                    : "0";

            if (BazowaPremiaAtakuDystans4 != null)
                AtakDystansSum4.Text = BazowaPremiaAtakuDystans4.Text != "0"
                    ? GetIntegerSumOfTextFields(new List<TextBox>()
                        {
                            BazowaPremiaAtakuDystans4,
                            ModyfikatorZreczAtakDystans,
                            ModyfikatorRozmiaruAtakDystans,
                            RozmaiteModyfikatoryAtakDystans
                        })
                        .ToString()
                    : "0";
        }

        private int GetIntegerSumOfTextFields(List<TextBox> list)
        {
            var sum = 0;
            if (list != null)
                foreach (var textBox in list)
                {
                    try
                    {
                        if (textBox == null) continue;
                        if (Int32.TryParse(textBox.Text, out int output))
                            sum += output;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }

            return sum;
        }

        private void Lvl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAtak();
        }

        private void BronWrecz_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox) sender;
            var currentItem = ((ComboBox) sender).SelectedItem;
            if (comboBox.SelectedIndex != -1)
            {
                if (Tarcza.SelectedItem != null)
                {
                    ClearPrzedmiotOchronny("Tarcza");
                }

                ClearBron("Bron3"); //Usuwamy dystansowe bo są dwureczne
                try
                {
                    GetControlByName<TextBox>(comboBox.Name + "Obrazenia").Text =
                        ((String) (((DataRowView) currentItem)["obrazenia"] ?? 0));
                    GetControlByName<TextBox>(comboBox.Name + "Krytyk").Text =
                        ((String) (((DataRowView) currentItem)["krytyk"] ?? 0));
                    GetControlByName<TextBox>(comboBox.Name + "Kategoria").Text =
                        (String) (((DataRowView) currentItem)["kategoria_nazwa"] ?? "");
                    GetControlByName<TextBox>(comboBox.Name + "Kategoria").ToolTip = "Kategoria: " +
                                                                                     (String) (
                                                                                         ((DataRowView) currentItem)[
                                                                                             "kategoria_nazwa"] ?? "");

                    if ((Int64) (((DataRowView) currentItem)["dwureczna"]) == 1)
                    {
                        //Dwuręczna
                        LockBron(comboBox.Name.EndsWith("1") ? "Bron2" : "Bron1", true);
                    }
                    else
                    {
                        LockBron(comboBox.Name.EndsWith("1") ? "Bron2" : "Bron1", false);
                    }

                    RefreshObrazeniaWrecz(comboBox);
                    AtakPremia_TextChanged(null, null);
                }
                catch (Exception exception)
                {
                    ClearBron(comboBox.Name);
                }
            }
            else
            {
                ClearBron(comboBox.Name);
            }
        }

        private void RefreshObrazeniaWrecz(ComboBox cb)
        {
            var currentItem = cb.SelectedItem;
            if (cb.SelectedIndex != -1)
            {
                var ctrl = GetControlByName<TextBox>(cb.Name + "Obrazenia");
                if ((Int64) (((DataRowView) currentItem)["dwureczna"]) == 1)
                {
                    //Dwuręczna
                    if (decimal.TryParse(StrengthModificatorValue.Content.ToString(), out decimal result))
                    {
                        var str = ((int) Math.Ceiling((double) result / 2) + result).ToString(CultureInfo
                            .InvariantCulture);
                        ctrl.Text = ctrl.Text.Split(' ')[0] + " " +
                                    (str.StartsWith("-") ? "" : "+") + str;
                    }
                }
                else
                {
                    ctrl.Text = ctrl.Text.Split(' ')[0] + " " +
                                (StrengthModificatorValue.Content.ToString().StartsWith("-") ? "" : "+") +
                                StrengthModificatorValue.Content;
                }
            }
        }
        

        private void ClearBron(string index)
        {
            GetControlByName<ComboBox>(index).SelectedIndex = -1;
            GetControlByName<TextBox>(index + "Kategoria").ToolTip = "Kategoria";
            GetControlByName<TextBox>(index + "PremiaDoAtaku").Text = "0";
            GetControlByName<TextBox>(index + "Obrazenia").Text = "0";
            GetControlByName<TextBox>(index + "Krytyk").Text = "0";
            GetControlByName<TextBox>(index + "Kategoria").Text = "";
            GetControlByName<TextBox>(index + "Specjalne").Text = "";
        }

        private void LockBron(string index, bool new_state)
        {
            if (new_state) ClearBron(index);
            GetControlByName<TextBox>(index + "Specjalne").IsEnabled = !new_state;
            GetControlByName<ComboBox>(index).IsEnabled = !new_state;
        }

        private T GetControlByName<T>(string name)
        {
            T obj = (T) FindName(name);
            return obj;
        }

        private void Bron3_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox) sender;
            var currentItem = ((ComboBox) sender).SelectedItem;
            if (comboBox.SelectedIndex != -1)
            {
                if (Tarcza.SelectedItem != null)
                {
                    ClearPrzedmiotOchronny("Tarcza");
                }

                //Wszystkie dystansowe są dwuręczne
                ClearBron("Bron1");
                ClearBron("Bron2");
                ClearPrzedmiotOchronny("Tarcza");
                try
                {
                    GetControlByName<TextBox>(comboBox.Name + "Obrazenia").Text =
                        ((String) (((DataRowView) currentItem)["obrazenia"] ?? 0));
                    GetControlByName<TextBox>(comboBox.Name + "Krytyk").Text =
                        ((String) (((DataRowView) currentItem)["krytyk"] ?? 0));
                    GetControlByName<TextBox>(comboBox.Name + "Kategoria").Text =
                        (String) (((DataRowView) currentItem)["kategoria_nazwa"] ?? "");
                    GetControlByName<TextBox>(comboBox.Name + "Kategoria").ToolTip = "Kategoria: " +
                                                                                     (String) (
                                                                                         ((DataRowView) currentItem)[
                                                                                             "kategoria_nazwa"] ?? "");

                    AtakPremia_TextChanged(null, null);
                }
                catch (Exception exception)
                {
                    ClearBron(comboBox.Name);
                }
            }
            else
            {
                ClearBron(comboBox.Name);
            }
        }

        private void Pancerz_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentItem = ((ComboBox) sender).SelectedItem;
            if (currentItem != null)
            {
                try
                {
                    PancerzPremia.Text =
                        ((Int64) (((DataRowView) currentItem)["premia"] ?? 0)).ToString();
                    PancerzKaraDoTestu.Text =
                        ((Int64) (((DataRowView) currentItem)["kara_do_testu"] ?? 0)).ToString();
                    PancerzMaxZrecz.Text =
                        (String) (((DataRowView) currentItem)["max_zrecznosc"] ?? 0);
                    PancerzNiepowodzenieCzaru.Text =
                        ((Int64) (((DataRowView) currentItem)["niepowodzenie_czaru"] ?? "0")).ToString() + "%";
                    PancerzSzybkość.Text =
                        ((Double) (((DataRowView) currentItem)["modyfikator_szybkosci"] ?? "0")).ToString(CultureInfo
                            .InvariantCulture);
                    PancerzKategoria.Text =
                        (String) (((DataRowView) currentItem)["kategoria_nazwa"] ?? "");


                    SprawdzTarcze();
                }
                catch (Exception exception)
                {
                    ClearPrzedmiotOchronny("Pancerz");
                }
            }
            else
            {
                ClearPrzedmiotOchronny("Pancerz");
            }

            RecalculateAttributes();
        }

        private void ClearPrzedmiotOchronny(string index)
        {
            GetControlByName<ComboBox>(index).SelectedIndex = -1;
            GetControlByName<TextBox>(index + "Premia").Text = "0";
            GetControlByName<TextBox>(index + "KaraDoTestu").Text = "0";
            GetControlByName<TextBox>(index + "MaxZrecz").Text = "0";
            GetControlByName<TextBox>(index + "NiepowodzenieCzaru").Text = "0";
            GetControlByName<TextBox>(index + "Szybkość").Text = "0";
            GetControlByName<TextBox>(index + "Kategoria").Text = "";
        }

        private void SprawdzTarcze()
        {
            if (Tarcza.SelectedIndex == -1) //Tarcza nie aktywna
            {
                return;
            }

            try
            {
                if (Bron3.SelectedIndex != -1) ClearBron("Bron3");
                if (Bron1.SelectedIndex != -1)
                {
                    if ((Int64) ((DataRowView) Bron1.SelectedItem)["dwureczna"] == 1) //Pierwsza bron dwureczna
                    {
                        ClearBron("Bron1");
                    }
                }

                if (Bron2.SelectedIndex != -1)
                {
                    if ((Int64) ((DataRowView) Bron2.SelectedItem)["dwureczna"] == 1) //Druga bron dwureczna
                    {
                        ClearBron("Bron2");
                    }
                }

                if (Bron1.SelectedIndex != -1 && Bron2.SelectedIndex != -1)
                {
                    ClearBron("Bron2");
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void PancerzSzybkość_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSpeed();
        }

        private void PancerzNiepowodzenieCzaru_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadNiepowodzenieCzaru();
        }

        private void LoadNiepowodzenieCzaru()
        {
            double procent = 0.0;
            try
            {
                if (PancerzNiepowodzenieCzaru.Text != "")
                {
                    if (Double.TryParse(PancerzNiepowodzenieCzaru.Text.Replace(".", ",").Replace("%", ""),
                        out double s))
                    {
                        procent += s;
                    }
                }

                if (TarczaNiepowodzenieCzaru.Text != "")
                {
                    if (Double.TryParse(TarczaNiepowodzenieCzaru.Text.Replace(".", ",").Replace("%", ""),
                        out double s))
                    {
                        procent += s;
                    }
                }
            }
            catch (Exception)
            {
            }

            NiepowodzenieCzaruWtajemniczen.Text = procent + "%";
        }

        private void PancerzKaraDoTestu_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadKaraDoTestu();
        }

        private void LoadKaraDoTestu()
        {
            double wartosc = 0.0;
            try
            {
                if (PancerzKaraDoTestu.Text != "")
                {
                    if (Double.TryParse(PancerzKaraDoTestu.Text.Replace(".", ","), out double s))
                    {
                        wartosc += s;
                    }
                }
            }
            catch (Exception)
            {
            }

            KaraDoTestuZPancerza.Text = wartosc.ToString(CultureInfo.InvariantCulture);
        }

        private Int64 GetSizeModifier()
        {
            var ret = Util.GetAllFromTable("modyfikator_rozmiar_rasa",
                new Dictionary<string, object>()
                {
                    {"rasa_id", rassName.SelectedValue},
                    {"rozmiar_id", (int) Rozmiar.getRozmiarById(size.SelectedIndex + 1)}
                });
            return (Int64) ret.Rows[0]["modyfikator"];
        }

        private long GetMaxSkillModifier()
        {
            long pancerz = Int64.MaxValue;
            long tarcza = Int64.MaxValue;
            if (PancerzMaxZrecz.Text != "" && PancerzMaxZrecz.Text != "-")
                pancerz = Int64.Parse(PancerzMaxZrecz.Text.Replace("-", ""));

            if (pancerz != 0 && tarcza != 0)
            {
                return pancerz < tarcza ? pancerz : tarcza;
            }

            return Int64.MaxValue;
        }

        private void TarczaKaraDoTestu_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadKaraDoTestu();
        }

        private void Tarcza_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentItem = ((ComboBox) sender).SelectedItem;
            if (currentItem != null)
            {
                try
                {
                    TarczaPremia.Text =
                        ((Int64) (((DataRowView) currentItem)["premia"] ?? 0)).ToString();
                    TarczaKaraDoTestu.Text =
                        ((Int64) (((DataRowView) currentItem)["kara_do_testu"] ?? 0)).ToString();
                    TarczaMaxZrecz.Text =
                        (String) (((DataRowView) currentItem)["max_zrecznosc"] ?? 0);
                    TarczaNiepowodzenieCzaru.Text =
                        ((Int64) (((DataRowView) currentItem)["niepowodzenie_czaru"] ?? "0")).ToString() + "%";
                    TarczaSzybkość.Text =
                        ((Double) (((DataRowView) currentItem)["modyfikator_szybkosci"] ?? "0")).ToString(CultureInfo
                            .InvariantCulture);
                    TarczaKategoria.Text =
                        (String) (((DataRowView) currentItem)["kategoria_nazwa"] ?? "");


                    SprawdzTarcze();
                }
                catch (Exception exception)
                {
                    ClearPrzedmiotOchronny("Tarcza");
                }
            }
            else
            {
                ClearPrzedmiotOchronny("Tarcza");
            }

            RecalculateAttributes();
        }

        private void TarczaNiepowodzenieCzaru_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadNiepowodzenieCzaru();
        }

        private void TarczaSzybkość_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSpeed();
        }

        private void TarczaPremia_TextChanged(object sender, TextChangedEventArgs e)
        {
            PremiaZTarczy.Text = ((TextBox) sender).Text;
        }

        private void PancerzPremia_TextChanged(object sender, TextChangedEventArgs e)
        {
            PremiaZPancerza.Text = ((TextBox) sender).Text;
        }

        private void AtakPremia_TextChanged(object sender, TextChangedEventArgs e)
        {
            BazowyAtakWrecz.Text = GetSummaryForBazowyAtakWrecz();
            BazowyAtakDyst.Text = GetSummaryForBazowyAtakDyst();
            if (Bron1 != null && Bron1.SelectedIndex != -1)
            {
                Bron1PremiaDoAtaku.Text = GetSummaryForAtakWrecz();
            }

            if (Bron2 != null && Bron2?.SelectedIndex != -1)
            {
                Bron2PremiaDoAtaku.Text = GetSummaryForAtakWrecz();
            }
            if (Bron3 != null && Bron3?.SelectedIndex != -1)
            {
                Bron3PremiaDoAtaku.Text = GetSummaryForAtakDyst();
            }
        }

        private string GetSummaryForBazowyAtakWrecz()
        {
            var wrecz = new List<string>();
            if (BazowaPremiaAtakuWrecz1 != null) wrecz.Add(BazowaPremiaAtakuWrecz1.Text);
            if (BazowaPremiaAtakuWrecz2 != null) wrecz.Add(BazowaPremiaAtakuWrecz2.Text);
            if (BazowaPremiaAtakuWrecz3 != null) wrecz.Add(BazowaPremiaAtakuWrecz3.Text);
            if (BazowaPremiaAtakuWrecz4 != null) wrecz.Add(BazowaPremiaAtakuWrecz4.Text);
            return String.Join(" / ", wrecz);
        }

        private string GetSummaryForAtakWrecz()
        {
            var wrecz = new List<string>();
            if (AtakWreczSum1 != null && AtakWreczSum1.Text != "0") wrecz.Add(AtakWreczSum1.Text);
            if (AtakWreczSum2 != null && AtakWreczSum2.Text != "0") wrecz.Add(AtakWreczSum2.Text);
            if (AtakWreczSum3 != null && AtakWreczSum3.Text != "0") wrecz.Add(AtakWreczSum3.Text);
            if (AtakWreczSum4 != null && AtakWreczSum4.Text != "0") wrecz.Add(AtakWreczSum4.Text);
            return String.Join(" / ", wrecz);
        }

        private string GetSummaryForBazowyAtakDyst()
        {
            var dyst = new List<string>();
            if (BazowaPremiaAtakuDystans1 != null) dyst.Add(BazowaPremiaAtakuDystans1.Text);
            if (BazowaPremiaAtakuDystans2 != null) dyst.Add(BazowaPremiaAtakuDystans2.Text);
            if (BazowaPremiaAtakuDystans3 != null) dyst.Add(BazowaPremiaAtakuDystans3.Text);
            if (BazowaPremiaAtakuDystans4 != null) dyst.Add(BazowaPremiaAtakuDystans4.Text);
            return String.Join(" / ", dyst);
        }

        private string GetSummaryForAtakDyst()
        {
            var dyst = new List<string>();
            if (AtakDystansSum1 != null && AtakDystansSum1.Text != "0") dyst.Add(AtakDystansSum1.Text);
            if (AtakDystansSum2 != null && AtakDystansSum2.Text != "0") dyst.Add(AtakDystansSum2.Text);
            if (AtakDystansSum3 != null && AtakDystansSum3.Text != "0") dyst.Add(AtakDystansSum3.Text);
            if (AtakDystansSum4 != null && AtakDystansSum4.Text != "0") dyst.Add(AtakDystansSum4.Text);
            return String.Join(" / ", dyst);
        }
    }
}