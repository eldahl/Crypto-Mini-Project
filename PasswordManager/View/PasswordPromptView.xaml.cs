using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using PasswordManager.Model;
using PasswordManager.ViewModel;

namespace PasswordManager.View
{
    /// <summary>
    /// Interaction logic for PasswordPrompt.xaml
    /// </summary>
    public partial class PasswordPromptView : Window
    {
        public PasswordPromptView()
        {
            InitializeComponent();
        }

        private void onEnterPasswordOK(object sender, RoutedEventArgs e)
        {
            // If the database is new, encrypt the verification tag and return
            if (MainWindowViewModel.instance.IsNewDatabase()) {
                Crypto.EncryptVerificationTag(PasswordBox.Password);
                MainWindowViewModel.instance.MasterPassword = PasswordBox.Password;
                this.Close();
                return;
            }

            PasswordDecryptResult result = Crypto.DecryptVerificationTag(PasswordBox.Password);

            // If the password is incorrect, display an error message
            if (result != PasswordDecryptResult.Success)
            {
                MessageBox.Show("Incorrect password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MainWindowViewModel.instance.MasterPassword = PasswordBox.Password;
            this.Close();
        }

        private void onEnterPasswordCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
