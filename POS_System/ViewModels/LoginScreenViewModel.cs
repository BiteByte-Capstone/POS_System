using POS_System.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace POS_System.ViewModels
{
    public class LoginScreenViewModel : ViewModelBase
    {
        //Fields
        //binding the View and ViewModel
        //Howard: skip password for now. will be completed later
        private int _id;
        private string _password;
        private string _errorMessage;
        private bool _canLogin = true;
        private DatabaseHelper _dbHelper;

        //Properties
        //add propertyChange to notify the value has changed 
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }


        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }


        }
        public bool CanLogin
        {
            get
            {
                return _canLogin;
            }
            set
            {
                _canLogin = value;
                OnPropertyChanged(nameof(CanLogin));
            }


        }

        //Commands
        public ICommand LoginCommand { get; }

        //Constructor
        public LoginScreenViewModel()
        {
            _dbHelper = new DatabaseHelper("yourServer", "yourDatabase", "yourUserID", "yourPassword");
            LoginCommand = new RalayCommand(ExecuteLoginCommad, CanExecuteLoginCommand);
        }

        
        //Logic to check if login can be performed like fields are not empty 
        private bool CanExecuteLoginCommand(object obj)
        {
            return Id != 0 && !string.IsNullOrWhiteSpace(Password);


        }

        //Here is the login validation 
        private void ExecuteLoginCommad(object obj)
        {
            // Replace with your actual DB connection parameters
            string server = "localhost";
            string database = "pos_db";
            string uid = "1521";
            string dbPassword = "password";

            using (var dbHelper = new DatabaseHelper(server, database, uid, dbPassword))
            {
                if (dbHelper.AuthenticateUser(Id.ToString(), Password))
                {
                    _canLogin = true;
                    ErrorMessage = "Successfully Logged In!";
                    //TODO: Navigate to next page
                }
                else
                {
                    _canLogin = false;
                    ErrorMessage = "Invalid credentials.";
                }
            }
        }
    }
}
