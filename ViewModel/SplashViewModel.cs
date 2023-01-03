using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

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
    public class SplashViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public SplashViewModel()
        {
            //if (IsInDesignMode)
            //{
            //    // Code runs in Blend --> create design time data.
            //}
            //else
            //{
            //    // Code runs "for real"
            //}
            _userControlLoadedCommand = new RelayCommand(UserControlLoaded);
            _userControlUnloadedCommand = new RelayCommand(UserControlUnloaded);
        }

        public string SoftwareVersion { get; } = "01.00.01";        // The hard-coded software version number

        #region UserControlLoaded Command
        /// <summary>
        /// UserControlLoaded Command
        /// Command and method to track when the user control is loaded
        /// </summary>
        private readonly RelayCommand _userControlLoadedCommand;
        public ICommand UserControlLoadedCommand => _userControlLoadedCommand;
        private void UserControlLoaded()
        {
            Mouse.OverrideCursor = Cursors.Wait;
        }
        #endregion

        #region UserControlLoaded Command
        /// <summary>
        /// UserControlLoaded Command
        /// Command and method to track when the user control is loaded
        /// </summary>
        private readonly RelayCommand _userControlUnloadedCommand;
        public ICommand UserControlUnloadedCommand => _userControlUnloadedCommand;
        private void UserControlUnloaded()
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }
        #endregion
    }
}