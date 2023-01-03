using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Nanopath.Service;

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
    public class CurrentSettingsViewModel : ViewModelBase
    {
        private readonly ITestService _testService;         // Instance of the TestService object

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public CurrentSettingsViewModel(ITestService testService)
        {
            _testService = testService;

            _goToSetupCommand = new RelayCommand(GoToSetupScreen);
            _goToLoadSampleCommand = new RelayCommand(GoToLoadSampleScreen);
            _goToMenuCommand = new RelayCommand(GoToMenuScreen);
        }
        #endregion

        #region HeaderText Property
        /// <summary>
        /// Header Text Property
        /// The currently displayed text in the header region of the UI. Compiler shows it as unused because it is dynamically bound in XAML.
        /// </summary>
        public string HeaderText { get; } = "Current Settings";
        #endregion

        #region CurrentSettings Property
        /// <summary>
        /// CurrentSettings Property
        /// The current settings as a readable string
        /// </summary>
        public string CurrentSettings => _testService.TestSettings.ToString();
        #endregion

        #region Commands
        /// <summary>
        /// Go To Setup Command
        /// Command and method to navigate to the Setup screen
        /// Sends a message to the Main ViewModel
        /// </summary>
        private RelayCommand _goToSetupCommand;
        public ICommand GoToSetupCommand => _goToSetupCommand;
        private void GoToSetupScreen() { Messenger.Default.Send(Screen.Setup); }

        /// <summary>
        /// Go To Load Sample Command
        /// Command and method to navigate to the Load Sample screen
        /// Sends a message to the Main ViewModel
        /// </summary>
        private RelayCommand _goToLoadSampleCommand;
        public ICommand GoToLoadSampleCommand => _goToLoadSampleCommand;
        private void GoToLoadSampleScreen() { Messenger.Default.Send(Screen.LoadSample); }

        /// <summary>
        /// Go To Menu Command
        /// Command and method to navigate to the Main Menu screen
        /// Sends a message to the Main ViewModel
        /// </summary>
        private RelayCommand _goToMenuCommand;
        public ICommand GoToMenuCommand => _goToMenuCommand;
        private void GoToMenuScreen() { Messenger.Default.Send(Screen.Menu); }
        #endregion Commands
    }
}