using PasswordManager.ViewModel;
using System.Diagnostics;
using System.Windows;

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

            // Add some dummy data
            viewModel.passwords.Add(new PasswordListViewItem { ServiceName = viewModel.outputText, Username = "email@email.com" });

            viewModel.passwords.Add(new PasswordListViewItem { ServiceName = "Google", Username = "email@email.com" });
            viewModel.passwords.Add(new PasswordListViewItem { ServiceName = "Facebook", Username = "email@email.com" });
            viewModel.passwords.Add(new PasswordListViewItem { ServiceName = "Twitter", Username = "email@email.com" });

            // Update the ListView
            passwordListView.Items.Clear();
            passwordListView.ItemsSource = viewModel.passwords;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Edit");
        }

        private void NewPassword_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("NewPassword");
        }
    }
}