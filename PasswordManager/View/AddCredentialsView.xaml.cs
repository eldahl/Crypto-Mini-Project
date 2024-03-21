using PasswordManager.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace PasswordManager.View
{
    /// <summary>
    /// Interaction logic for AddCredentialsView.xaml
    /// </summary>
    public partial class AddCredentialsView : Window
    {
        public AddCredentialsView()
        {
            InitializeComponent();
            AddCredentialsViewModel viewModel = new AddCredentialsViewModel();
            this.DataContext = viewModel;
        }

        private void GenerateRandomPassword_Click(object sender, RoutedEventArgs e)
        {
            ((AddCredentialsViewModel)this.DataContext).Password = Crypto.GenerateRandomPassword(14, true);
        }

        private void CopyPassword_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(((AddCredentialsViewModel)this.DataContext).Password);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if all fields are filled
            if (((AddCredentialsViewModel)this.DataContext).ServiceName == "" || ((AddCredentialsViewModel)this.DataContext).Username == "" || ((AddCredentialsViewModel)this.DataContext).Password == "")
            {
                MessageBox.Show("Please fill in all fields", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            Crypto.AddNewCredentials(((AddCredentialsViewModel)this.DataContext).ServiceName, ((AddCredentialsViewModel)this.DataContext).Username, ((AddCredentialsViewModel)this.DataContext).Password, MainWindowViewModel.instance.MasterPassword);
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
