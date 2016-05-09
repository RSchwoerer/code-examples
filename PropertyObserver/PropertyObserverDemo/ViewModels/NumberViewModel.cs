using System.ComponentModel;
using System.Windows.Input;

namespace PropertyObserverDemo.ViewModels
{
    public class NumberViewModel : INotifyPropertyChanged
    {
        #region Observable Properties

        public bool IsEven
        {
            get { return this.Value % 2 == 0; }
        }

        public bool IsNegative
        {
            get { return this.Value < 0; }
        }

        public int Value
        {
            get { return _value; }
            set
            {
                if (value == _value)
                    return;

                bool wasEven = this.IsEven;
                bool wasNegative = this.IsNegative;

                _value = value;

                this.OnPropertyChanged("Value");

                if (wasEven != this.IsEven)
                    this.OnPropertyChanged("IsEven");

                if (wasNegative != this.IsNegative)
                    this.OnPropertyChanged("IsNegative");
            }
        }

        #endregion // Observable Properties

        #region Commands

        public ICommand DecrementCommand
        {
            get { return _decrementCommand ?? (_decrementCommand = new RelayCommand(param => --this.Value)); }
        }

        public ICommand IncrementCommand
        {
            get { return _incrementCommand ?? (_incrementCommand = new RelayCommand(param => ++this.Value)); }
        }

        #endregion // Commands

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Fields

        RelayCommand _decrementCommand;
        RelayCommand _incrementCommand;
        int _value;

        #endregion // Fields
    }
}