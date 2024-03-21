using PasswordManager.ViewModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

using PasswordManager.Model;
using System.Text;
using System.Windows.Controls;

namespace PasswordManager.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        
        MainWindowViewModel viewModel;
        
        public MainWindow() {
            InitializeComponent();

            // Initialize the ViewModel
            viewModel = new MainWindowViewModel();
            this.DataContext = viewModel;

            // Update the ListView
            passwordListView.Items.Clear();
            passwordListView.ItemsSource = viewModel.Passwords;

            // Prompt user for password
            PromptUserForPassword();

            viewModel.OnStartupLoadDatabase();
        }

        private void PromptUserForPassword()
        {
            // Prompt user for password using a dialog
            PasswordPromptView passwordPrompt = new PasswordPromptView();
            passwordPrompt.ShowDialog();

        }

        private void Export_User_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button!.DataContext as PasswordListViewItem;
            
            System.Windows.Clipboard.SetText(item!.Username);
        }

        private void Export_Pass_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button!.DataContext as PasswordListViewItem;

            string password = Crypto.DecryptPassword(item!.PasswordData, viewModel.MasterPassword);

            System.Windows.Clipboard.SetText(password);
        }

        private void NewCredentials_Click(object sender, RoutedEventArgs e)
        {
            // Open a new window to add new credentials
            AddCredentialsView addCredentials = new AddCredentialsView();
            addCredentials.ShowDialog();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            // Open file browser to select and load a database file
            viewModel.LoadDatabase();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Data is saved to the database automatically.\nCreate a new database using the new button.\nLoad other databases by using the load button.\nAdd new credentials using the add credentials button.");
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            // Open file browser to create and save a new database file
            viewModel.NewDatabase();
        }
    }
}