/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Nanopath"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using CommonServiceLocator;
using Nanopath.Service;

namespace Nanopath.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //if (ViewModelBase.IsInDesignModeStatic)
            //{
            //    // Create design time view services and models
            //    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            //}
            //else
            //{
            //    // Create run time view services and models
            //    SimpleIoc.Default.Register<IDataService, DataService>();
            //}

            #region Register Services
            SimpleIoc.Default.Register<ITestService, TestService>(); 
            SimpleIoc.Default.Register<IDiagnosticsService, DiagnosticsService>(); 
            #endregion

            #region Register ViewModels
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SplashViewModel>();
            SimpleIoc.Default.Register<MenuViewModel>();
            SimpleIoc.Default.Register<SetupViewModel>();
            SimpleIoc.Default.Register<CurrentSettingsViewModel>();
            SimpleIoc.Default.Register<LoadSampleViewModel>();
            SimpleIoc.Default.Register<TestViewModel>();
            SimpleIoc.Default.Register<ResultsViewModel>(); 
            #endregion
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }
        
        public static void Cleanup()
        {
            // Clear the ViewModels
            SimpleIoc.Default.Unregister<SplashViewModel>();
            SimpleIoc.Default.Unregister<MenuViewModel>();
            SimpleIoc.Default.Unregister<SetupViewModel>();
            SimpleIoc.Default.Unregister<CurrentSettingsViewModel>();
            SimpleIoc.Default.Unregister<LoadSampleViewModel>();
            SimpleIoc.Default.Unregister<TestViewModel>();
            SimpleIoc.Default.Unregister<ResultsViewModel>();
        }

        public SplashViewModel SplashViewModel => ServiceLocator.Current.GetInstance<SplashViewModel>();
        public MenuViewModel MenuViewModel => ServiceLocator.Current.GetInstance<MenuViewModel>();
        public SetupViewModel SetupViewModel => ServiceLocator.Current.GetInstance<SetupViewModel>();
        public CurrentSettingsViewModel CurrentSettingsViewModel => ServiceLocator.Current.GetInstance<CurrentSettingsViewModel>();
        public LoadSampleViewModel LoadSampleViewModel => ServiceLocator.Current.GetInstance<LoadSampleViewModel>();
        public TestViewModel TestViewModel => ServiceLocator.Current.GetInstance<TestViewModel>();
        public ResultsViewModel ResultsViewModel => ServiceLocator.Current.GetInstance<ResultsViewModel>();
    }
}