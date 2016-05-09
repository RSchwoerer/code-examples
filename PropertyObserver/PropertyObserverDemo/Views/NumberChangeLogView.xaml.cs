using System;
using System.Windows.Controls;

namespace PropertyObserverDemo.Views
{
    public partial class NumberChangeLogView : UserControl
    {
        public NumberChangeLogView()
        {
            InitializeComponent();
        }

        void OnLogged(object sender, EventArgs e)
        {
            _scrollViewer.ScrollToBottom();
        }
    }
}