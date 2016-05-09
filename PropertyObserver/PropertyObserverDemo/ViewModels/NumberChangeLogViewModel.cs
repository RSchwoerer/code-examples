using System;
using System.Collections.ObjectModel;

namespace PropertyObserverDemo.ViewModels
{
    public class NumberChangeLogViewModel
    {
        PropertyObserver<NumberViewModel> _observer;

        public NumberChangeLogViewModel()
        {
            this.Number = new NumberViewModel();
            this.ChangeLog = new ObservableCollection<string>();

            _observer =
                new PropertyObserver<NumberViewModel>(this.Number)
                   .RegisterHandler(n => n.Value, n => Log("Value: " + n.Value))
                   .RegisterHandler(n => n.IsNegative, this.AppendIsNegative)
                   .RegisterHandler(n => n.IsEven, this.AppendIsEven);
        }

        void AppendIsNegative(NumberViewModel number)
        {
            if (number.IsNegative)
                this.Log("\tNumber is now negative");
            else
                this.Log("\tNumber is now positive");
        }

        void AppendIsEven(NumberViewModel number)
        {
            if (number.IsEven)
                this.Log("\tNumber is now even");
            else
                this.Log("\tNumber is now odd");
        }

        void Log(string message)
        {
            this.ChangeLog.Add(message);
            this.OnLogged();
        }

        public ObservableCollection<string> ChangeLog { get; private set; }

        public NumberViewModel Number { get; private set; }

        #region Logged Event

        public event EventHandler Logged;

        void OnLogged()
        {
            EventHandler handler = this.Logged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion // Logged Event
    }
}