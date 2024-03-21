
namespace PasswordManager.ViewModel
{
    class AddCredentialsViewModel : Bindable
    {
        public string ServiceName {
            get { return _serviceName; }
            set { _serviceName = value; propertyIsChanged(); }
        }
        private string _serviceName = "";

        public string Username {
            get { return _username; }
            set { _username = value; propertyIsChanged(); }
        }
        private string _username;

        public string Password {
            get { return _password; }
            set { _password = value; propertyIsChanged(); }
        }
        private string _password = Crypto.GenerateRandomPassword(12, true);

        public AddCredentialsViewModel() {
            
        }


    }
}
