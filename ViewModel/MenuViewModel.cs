using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Nanopath.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MenuViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MenuViewModel()
        {
            _goToSetupCommand = new RelayCommand(GoToSetupScreen);
        }

        /// <summary>
        /// Header Text Property
        /// The currently displayed text in the header region of the UI. Compiler shows it as unused because it is dynamically bound in XAML.
        /// </summary>
        public string HeaderText { get; } = "Main Menu";

        /// <summary>
        /// Go To Setup Command
        /// Command and method to navigate to the setup screen
        /// Sends a message to the Main ViewModel
        /// </summary>
        private RelayCommand _goToSetupCommand;
        public ICommand GoToSetupCommand => _goToSetupCommand;
        private void GoToSetupScreen() { Messenger.Default.Send(Screen.Setup); }
    }
}