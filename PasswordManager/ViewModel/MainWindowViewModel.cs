using PasswordManager.Model;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using PasswordManager.View;

namespace PasswordManager.ViewModel
{
    public class MainWindowViewModel : Bindable
    {
        public string outputText { get; set; } = "Hello, World!";

        public ObservableCollection<PasswordListViewItem> Passwords { 
            get { return _passwords; } 
            set { _passwords = value; propertyIsChanged(); } 
        }
        private ObservableCollection<PasswordListViewItem> _passwords = new ObservableCollection<PasswordListViewItem>();

        SQLiteDatabaseContext sdb;
        public static MainWindowViewModel instance;

        public string MasterPassword { get; set; }

        public MainWindowViewModel() {
            // Singleton
            if (instance is null)
                instance = this;
            else { 
                throw new Exception("MainWindowViewModel instance already exists");
            }

            // Initialize the database context
            sdb = new SQLiteDatabaseContext();
        }

        public void OnStartupLoadDatabase()
        {
            // Load all the passwords from the database
            Passwords.Clear();
            foreach (DBPasswordEntry p in sdb.GetAllPasswordEntries())
            {
                Passwords.Add(new PasswordListViewItem()
                {
                    ServiceName = Encoding.UTF8.GetString(p.name),
                    Username = Encoding.UTF8.GetString(p.username),
                    PasswordData = p
                });
            }
        }

        public bool IsNewDatabase() {
            return sdb.IsNewDatabase();
        }

        public void LoadDatabase()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Password Database (*.db)|*.db";
            openFileDialog.Title = "Open Password Database";
            openFileDialog.ShowDialog();

            Debug.WriteLine(openFileDialog.FileName);
            
            // Set the database path in the SQLiteDatabaseContext
            SQLiteDatabaseContext.instance.DatabaseFilePath = openFileDialog.FileName;

            // Prompt user for password using a dialog
            PasswordPromptView passwordPrompt = new PasswordPromptView();
            passwordPrompt.ShowDialog();

            // Load all the passwords from the database
            Passwords.Clear();
            foreach (DBPasswordEntry p in sdb.GetAllPasswordEntries())
            {
                Passwords.Add(new PasswordListViewItem() { 
                    ServiceName = Encoding.UTF8.GetString(p.name),
                    Username = Encoding.UTF8.GetString(p.username),
                    PasswordData = p
                });
            }
        }

        // Same procedure as LoadDatabase, except it sets the database path to a new file instead of a preexisting database file
        public void NewDatabase() {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Password Database (*.db)|*.db";
            saveFileDialog.Title = "Open Password Database";
            saveFileDialog.ShowDialog();

            Debug.WriteLine(saveFileDialog.FileName);

            // Set the database path in the SQLiteDatabaseContext
            SQLiteDatabaseContext.instance.DatabaseFilePath = saveFileDialog.FileName;

            Passwords.Clear();

            // Prompt user for password using a dialog
            PasswordPromptView passwordPrompt = new PasswordPromptView();
            passwordPrompt.ShowDialog();
        }
    }
}
