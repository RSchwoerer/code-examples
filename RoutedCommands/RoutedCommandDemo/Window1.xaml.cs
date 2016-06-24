using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RoutedCommandDemo
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        void Foo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // The Window gets to determine if the Foo 
            // command can execute at this time.
            e.CanExecute = true;
        }

        void Foo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // The Window executes the command logic when the user wants to Foo.
            MessageBox.Show("The Window is Fooing...");
        }
    }
}