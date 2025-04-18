using SQLite;
using System.Collections.ObjectModel;
using System.IO;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        SQLiteConnection db;
        ObservableCollection<Income> incomeList = new();
        ObservableCollection<Outcome> outcomeList = new();
        ObservableCollection<PassIncome> passIncomeList = new();

        string dbPath => Path.Combine(FileSystem.AppDataDirectory, "finance.db");
        User currentUser;

        string currentType = "Income";
        object editItem = null;

        public MainPage()
        {
            InitializeComponent();
            db = new SQLiteConnection(dbPath);
            db.CreateTable<User>();
            db.CreateTable<Income>();
            db.CreateTable<Outcome>();
            db.CreateTable<PassIncome>();
            UpdateUI();
        }

        void UpdateUI()
        {
            AuthLayout.IsVisible = currentUser == null;
            MainLayout.IsVisible = currentUser != null;
            AddFormLayout.IsVisible = false;
            SwitchTable("Main");
        }

        void OnRegisterClicked(object sender, EventArgs e)
        {
            string login = LoginEntry.Text?.Trim();
            string password = PasswordEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                DisplayAlert("Ошибка", "Введите логин и пароль", "OK");
                return;
            }

            if (db.Table<User>().FirstOrDefault(u => u.Login == login) != null)
            {
                DisplayAlert("Ошибка", "Пользователь уже существует", "OK");
                return;
            }

            db.Insert(new User { Login = login, Password = password });
            DisplayAlert("Успех", "Регистрация завершена", "OK");
        }

        void OnLoginClicked(object sender, EventArgs e)
        {
            string login = LoginEntry.Text?.Trim();
            string password = PasswordEntry.Text?.Trim();

            currentUser = db.Table<User>().FirstOrDefault(u => u.Login == login && u.Password == password);

            if (currentUser != null)
            {
                DisplayAlert("Добро пожаловать", login, "OK");
                UpdateUI();
            }
            else
            {
                DisplayAlert("Ошибка", "Неверные данные", "OK");
            }
        }

        void SwitchTable(string type)
        {
            currentType = type;

            IncomeView.IsVisible = type == "Income";
            OutcomeView.IsVisible = type == "Outcome";
            PassIncomeView.IsVisible = type == "PassIncome";
            MainInfoLayout.IsVisible = type == "Main";

            AddFormLayout.IsVisible = false;

            IncomeView.ItemsSource = incomeList;
            OutcomeView.ItemsSource = outcomeList;
            PassIncomeView.ItemsSource = passIncomeList;

            if (currentUser == null) return;

            incomeList.Clear();
            outcomeList.Clear();
            passIncomeList.Clear();

            foreach (var item in db.Table<Income>().Where(i => i.UID == currentUser.ID))
                incomeList.Add(item);

            foreach (var item in db.Table<Outcome>().Where(i => i.UID == currentUser.ID))
                outcomeList.Add(item);

            foreach (var item in db.Table<PassIncome>().Where(i => i.UID == currentUser.ID))
                passIncomeList.Add(item);

            IncomeButton.BackgroundColor = type == "Income" ? Colors.DarkGray : Colors.Gray;
            OutcomeButton.BackgroundColor = type == "Outcome" ? Colors.DarkGray : Colors.Gray;
            PassIncomeButton.BackgroundColor = type == "PassIncome" ? Colors.DarkGray : Colors.Gray;
            MainButton.BackgroundColor = type == "Main" ? Colors.DarkGray : Colors.Gray;

            if (type == "Main")
                UpdateMainInfo();
        }

        void OnIncomeClicked(object sender, EventArgs e) => SwitchTable("Income");
        void OnOutcomeClicked(object sender, EventArgs e) => SwitchTable("Outcome");
        void OnPassIncomeClicked(object sender, EventArgs e) => SwitchTable("PassIncome");
        void OnMainClicked(object sender, EventArgs e) => SwitchTable("Main");

        void OnAddClicked(object sender, EventArgs e)
        {
            EntryAmount.Text = "";
            EntryDate.Date = DateTime.Today;
            EntryBuyPrice.Text = "";
            EntryPercent.Text = "";
            editItem = null;

            EntryAmount.IsVisible = currentType != "PassIncome";
            EntryDate.IsVisible = true;

            EntryBuyPrice.IsVisible = currentType == "PassIncome";
            EntryPercent.IsVisible = currentType == "PassIncome";

            LabelBuyPrice.IsVisible = currentType == "PassIncome";
            LabelPercent.IsVisible = currentType == "PassIncome";

            AddFormLayout.IsVisible = true;
        }

        void OnEditClicked(object sender, EventArgs e)
        {
            editItem = (sender as Button)?.CommandParameter;
            if (editItem is Income income)
            {
                EntryAmount.Text = income.Amount.ToString();
                EntryDate.Date = income.Date;
            }
            else if (editItem is Outcome outcome)
            {
                EntryAmount.Text = outcome.Amount.ToString();
                EntryDate.Date = outcome.Date;
            }
            else if (editItem is PassIncome pass)
            {
                EntryBuyPrice.Text = pass.BuyPrice.ToString();
                EntryPercent.Text = pass.Percent.ToString();
                EntryDate.Date = pass.Date;
            }

            EntryAmount.IsVisible = currentType != "PassIncome";
            EntryBuyPrice.IsVisible = currentType == "PassIncome";
            EntryPercent.IsVisible = currentType == "PassIncome";

            LabelBuyPrice.IsVisible = currentType == "PassIncome";
            LabelPercent.IsVisible = currentType == "PassIncome";

            AddFormLayout.IsVisible = true;
        }

        void OnSaveClicked(object sender, EventArgs e)
        {
            var date = EntryDate.Date;
            var uid = currentUser.ID;

            if (currentType == "Income")
            {
                if (decimal.TryParse(EntryAmount.Text, out decimal amount))
                {
                    if (editItem is Income existing)
                    {
                        existing.Amount = amount;
                        existing.Date = date;
                        db.Update(existing);
                    }
                    else
                    {
                        db.Insert(new Income { UID = uid, Amount = amount, Date = date });
                    }
                }
            }
            else if (currentType == "Outcome")
            {
                if (decimal.TryParse(EntryAmount.Text, out decimal amount))
                {
                    if (editItem is Outcome existing)
                    {
                        existing.Amount = amount;
                        existing.Date = date;
                        db.Update(existing);
                    }
                    else
                    {
                        db.Insert(new Outcome { UID = uid, Amount = amount, Date = date });
                    }
                }
            }
            else if (currentType == "PassIncome")
            {
                if (decimal.TryParse(EntryBuyPrice.Text, out decimal buy) &&
                    decimal.TryParse(EntryPercent.Text, out decimal percent))
                {
                    if (editItem is PassIncome existing)
                    {
                        existing.BuyPrice = buy;
                        existing.Percent = percent;
                        existing.Date = date;
                        db.Update(existing);
                    }
                    else
                    {
                        db.Insert(new PassIncome { UID = uid, BuyPrice = buy, Percent = percent, Date = date });
                    }
                }
            }

            AddFormLayout.IsVisible = false;
            SwitchTable(currentType);
        }

        void OnDeleteClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is Income income)
                db.Delete(income);
            if ((sender as Button)?.CommandParameter is Outcome outcome)
                db.Delete(outcome);
            if ((sender as Button)?.CommandParameter is PassIncome pass)
                db.Delete(pass);

            SwitchTable(currentType);
        }

        void UpdateMainInfo()
        {
            var uid = currentUser.ID;

            decimal incomeSum = db.Table<Income>().Where(i => i.UID == uid).Sum(i => i.Amount);
            decimal outcomeSum = db.Table<Outcome>().Where(o => o.UID == uid).Sum(o => o.Amount);
            decimal balance = incomeSum - outcomeSum;

            BalanceLabel.Text = $"Баланс: {balance} ₽";
            BalanceLabel.TextColor = balance < 0 ? Colors.Red : Colors.Black;

            var now = DateTime.Now;
            var nextDividend = db.Table<PassIncome>()
                                 .Where(p => p.UID == uid && p.Date > now)
                                 .OrderBy(p => p.Date)
                                 .FirstOrDefault();

            DividendLabel.Text = "Ближайшие дивиденды: " + (nextDividend != null ? nextDividend.Date.ToString("dd.MM.yyyy") : "—");
        }
    }

    public class User
    {
        [PrimaryKey, AutoIncrement] public int ID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class Income
    {
        [PrimaryKey, AutoIncrement] public int ID { get; set; }
        public int UID { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }

    public class Outcome
    {
        [PrimaryKey, AutoIncrement] public int ID { get; set; }
        public int UID { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }

    public class PassIncome
    {
        [PrimaryKey, AutoIncrement] public int ID { get; set; }
        public int UID { get; set; }
        public DateTime Date { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal Percent { get; set; }
    }

}
