using System.Windows;

namespace TabControlStyle
{
   public partial class Main : Window
   {
      public Main()
      {
         InitializeComponent();
      }

      private void cmd1_Click(object sender, RoutedEventArgs e)
      {
         Window win = new TabControl_1_Start();
         win.Show();
      }

      private void cmd2_Click(object sender, RoutedEventArgs e)
      {
         Window win = new TabControl_2_Animations();
         win.Show();
      }
   }
}
