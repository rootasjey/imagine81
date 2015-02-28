using Imagine.Common;
using Imagine.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Imagine.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public Settings()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            // Refresh the username TextBox
            RefreshUserNameBox();

            // Put the right visual state for the toggle switch
            CheckDynamicBackground();
        }


        private void RefreshUserNameBox()
        {
            if (BoxName.Text == null || BoxName.Text == "")
            {
                if (App.WebMethods._user.Name != null)
                {
                    BoxName.Text = App.WebMethods._user.Name;
                }
                else BoxName.Text = "Anonymous";
                
            }
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            CheckLanguage();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        private void CheckLanguage()
        {
            if (App.WebMethods.applicationLanguage == "FR" || App.WebMethods.applicationLanguage == null)
            {
                App.WebMethods.applicationLanguage = "FR";
                LanguageFrench.Foreground = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];
            }
            else if (App.WebMethods.applicationLanguage == "EN")
            {
                LanguageEnglish.Foreground = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];
            }

            // Affiche la langue dans le TextBlock
            TBLang.Text = App.WebMethods.applicationLanguage;
        }

        private void TGDynamicTile_Toggled(object sender, RoutedEventArgs e)
        {
            if (TGDynamicTile.IsOn)
            {
                RegisterBackgroundTask();
            }
            else UnregisterBackgroundTask();
        }

        private async void RegisterBackgroundTask()
        {
            // On Windows, RequestAccessAsync presents the user with a confirmation
            // dialog that requests that an app be allowed ont the lock screen.
            // On Windows Phone, RequestAccessAsync does not show any user confirmation UI
            // but *must* be called before registering any tasks.
            var access = await BackgroundExecutionManager.RequestAccessAsync();
            
            // A 'good' status return on Phone is BackgroundAccess.AllowedMayUseActiveRealTimeConnectivity
            if (access == BackgroundAccessStatus.Denied)
            {
                // Either the user has explicitly denied background execution for this app
                // or the maximum number of background apps across the system has been reached
                // Display some informative message to the user...
                MessageDialog dialog = new MessageDialog("The BackgroundTask couldn't be enregistred.", "Oops!");
                dialog.ShowAsync();
            }
            else
            {
                BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
                taskBuilder.Name = "BackgroundTaskQuote";

                // Many different trigger type could be used here
                TimeTrigger trigger = new TimeTrigger(16, false);
                taskBuilder.SetTrigger(trigger);
                taskBuilder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));

                // Entry point is the full name of our IBackgroundTask implementation
                // Good practice to user reflection as here to ensure correct name
                taskBuilder.TaskEntryPoint = "BackgroundTaskQuote.TheTask";
                //taskBuilder.TaskEntryPoint = typeof(BackgroundTaskQuote.TheTask);
                taskBuilder.Register();
            }
        }

        private async void UnregisterBackgroundTask()
        {
            // AllTasks is a disctionary <Guid, IBackgroundTaskRegistration> so you can back
            // to your registration by id or by position, or select First if you only have one registration.
            var taskRegistration = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault();

            // We could then unregister the tasj, optionally cancelling any running instance
            if (taskRegistration!=null)
            {
                taskRegistration.Unregister(true);
            }
        }


        private void LanguageFrench_Click(object sender, RoutedEventArgs e)
        {
            if (App.WebMethods.applicationLanguage != "FR")
            {
                App.WebMethods.applicationLanguage = "FR";
                LanguageEnglish.Foreground = (SolidColorBrush)App.Current.Resources["PhoneBackgroundBrush"];
                LanguageFrench.Foreground = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];

                //App.WebMethods.SaveSettings();              // persistance
                App.WebMethods.databaseQuoteChanged = true; // notify changes
            }
        }

        private void LanguageEnglish_Click(object sender, RoutedEventArgs e)
        {
            var color = App.Current.Resources["PhoneBackgroundBrush"];

            if (App.WebMethods.applicationLanguage != "EN")
            {
                App.WebMethods.applicationLanguage = "EN";
                LanguageFrench.Foreground = (SolidColorBrush)App.Current.Resources["PhoneBackgroundBrush"];
                LanguageEnglish.Foreground = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];

                //App.WebMethods.SaveSettings();              // persistance
                App.WebMethods.databaseQuoteChanged = true; // notify changes
            }
        }

        /// <summary>
        /// Compose a new email to the app developer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonContactMe_Click(object sender, RoutedEventArgs e)
        {
            var email = new EmailMessage();
            email.To.Add(new EmailRecipient("metroappdev@outlook.com"));
            await EmailManager.ShowComposeNewEmailAsync(email);
        }

        private void BoxName_GotFocus(object sender, RoutedEventArgs e)
        {
            BoxName.Foreground = (Brush)Resources["PhoneBackgroundBrush"];
        }

        private void BoxName_LostFocus(object sender, RoutedEventArgs e)
        {
            BoxName.Foreground = (Brush)Resources["PhoneForegroundBrush"];
        }

        /// <summary>
        /// Update in the database if the username has been changed
        /// </summary>
        private async Task CheckUserNameChanged()
        {
            if (BoxName.Text != App.WebMethods._user.Name)
            {
                string oldname = App.WebMethods._user.Name;

                User user = App.WebMethods._user;
                user.Name = BoxName.Text;

                bool available = await App.WebMethods.CheckUsernameAvailability(user.Name);

                if (!available)
                {
                    // If available == false, => the name is already taken, exit the function
                    MessageDialog dialog = new MessageDialog("This name is already taken, please choose another one");
                    dialog.ShowAsync();
                    return;
                }
                
                // The name is available, try to update the database
                bool result = await App.WebMethods.UpdateUser(user);

                if (result)
                {
                    MessageDialog dialog = new MessageDialog("Your username has been update!", "Success");
                    dialog.ShowAsync();

                    // Update quotes with the new name
                    App.WebMethods._user.Name = BoxName.Text;
                    App.WebMethods.UpdateCreationWithNewUsername(oldname);

                    App.WebMethods.SaveUser();

                    App.WebMethods.databaseQuoteChanged = true; // notify changes

                    // Exit the settings
                    //if (Frame.CanGoBack)
                    //    Frame.GoBack();
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("Couldn't update your username, try in some minute and check your internet connexion", "Oops!");
                    dialog.ShowAsync();
                }
            }
        }

        /// <summary>
        /// Save changes, and Return to the previous page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            await CheckUserNameChanged();

            App.WebMethods.SaveSettings();  // persistance

            // Exit settings
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        /// <summary>
        /// Return to the previous page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DenyButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        private void TGDynamicBackground_Toggled(object sender, RoutedEventArgs e)
        {
            if (TGDynamicBackground.IsOn)
                App.WebMethods._dynamicBackground = true;
            else
                App.WebMethods._dynamicBackground = false;
        }

        private void CheckDynamicBackground()
        {
            if (App.WebMethods._dynamicBackground)
                TGDynamicBackground.IsOn = true;
            else TGDynamicBackground.IsOn = false;
        }
    }
}
