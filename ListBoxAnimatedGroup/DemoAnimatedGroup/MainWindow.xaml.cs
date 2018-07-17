using System.Windows;
using System.Windows.Data;
using DemoAnimatedGroup.ViewModel;

namespace DemoAnimatedGroup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var viewmodel = new MainViewModel();

            this.DataContext = viewmodel;

            var cv = CollectionViewSource.GetDefaultView(viewmodel.Cities);
            cv.GroupDescriptions.Add(new PropertyGroupDescription("Country"));
        }
    }
}
