using System.Windows.Controls;

namespace Nanopath.View
{
    /// <summary>
    /// Interaction logic for LoadSampleScreen.xaml
    /// </summary>
    public partial class LoadSampleScreen : UserControl
    {
       
        public LoadSampleScreen()
        {
            InitializeComponent();

        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.SampleIdBox.Focus();
        }
    }
}
