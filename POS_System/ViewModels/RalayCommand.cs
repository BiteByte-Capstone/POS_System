using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace POS_System.ViewModels
{
    //To binding the action button on .xaml
    public class RalayCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public Action<object> _execute { get; set; }

        public Predicate<object> _canExecute { get; set; }

        public RalayCommand(Action<object> Execute, Predicate<object> CanExecuteMethod) 
        {
            _canExecute = CanExecuteMethod;
            _execute = Execute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
