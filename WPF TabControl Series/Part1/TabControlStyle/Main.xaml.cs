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
         Window win = new TabControl_1_ColorsOnly();
         win.Show();
      }

      private void cmd2_Click(object sender, RoutedEventArgs e)
      {
         Window win = new TabControl_2_Sections();
         win.Show();
      }

      private void cmd3_Click(object sender, RoutedEventArgs e)
      {
         Window win = new TabControl_3_SimpleStyle();
         win.Show();
      }

      private void cmd4_Click(object sender, RoutedEventArgs e)
      {
         Window win = new TabControl_4_SimpleStyle_TabItems();
         win.Show();
      }

      private void cmd5_Click(object sender, RoutedEventArgs e)
      {
         Window win = new TabControl_5_Triggers();
         win.Show();
      }

   
   }
}
