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
         Window win = new TabControl_2_ScrollViewer_Simple();
         win.Show();
      }

      private void cmd3_Click(object sender, RoutedEventArgs e)
      {
         Window win = new TabControl_3_ScrollViewer_Advanced();
         win.Show();
      }

      private void cmd4_Click(object sender, RoutedEventArgs e)
      {
         Window win = new TabControl_4_ScrollViewer_Menu();
         win.Show();
      }

      private void cmd5_Click(object sender, RoutedEventArgs e)
      {
         Window win = new TabControl_5_ScrollableTabPanel();
         win.Show();
      }
   }
}
