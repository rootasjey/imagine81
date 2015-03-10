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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
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

            RefreshUserNameBox();
            CheckDynamicBackground();
            CheckAppLanguage();
            CheckIconTheme();
        }


        /// <summary>
        /// Actualise le nom de l'utilisateur connecté
        /// </summary>
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
        /// Actualise le status du fond dynamique
        /// </summary>
        private void CheckDynamicBackground()
        {
            if (App.WebMethods._dynamicBackground)
                TGDynamicBackground.IsOn = true;
            else TGDynamicBackground.IsOn = false;
        }

        /// <summary>
        /// Acutalise la langue de l'application
        /// </summary>
        private void CheckAppLanguage()
        {
            if (App.WebMethods._applicationLanguage == "FR")
            {
                ButtonLanguage.Content = "Langue : Français";
                LanguageFrench.Foreground = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];
            }
            else if (App.WebMethods._applicationLanguage == "EN")
            {
                ButtonLanguage.Content = "Languague : English";
                LanguageEnglish.Foreground = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];
            }
            else
            {
                App.WebMethods._applicationLanguage = "FR";
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
            if (App.WebMethods._applicationLanguage != "FR")
            {
                App.WebMethods._applicationLanguage = "FR";
                LanguageEnglish.Foreground = (SolidColorBrush)App.Current.Resources["PhoneBackgroundBrush"];
                LanguageFrench.Foreground = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];

                //App.WebMethods.SaveSettings();              // persistance
                App.WebMethods._databaseQuoteChanged = true; // notify changes
                CheckAppLanguage();
            }
        }

        private void LanguageEnglish_Click(object sender, RoutedEventArgs e)
        {
            var color = App.Current.Resources["PhoneBackgroundBrush"];

            if (App.WebMethods._applicationLanguage != "EN")
            {
                App.WebMethods._applicationLanguage = "EN";
                LanguageFrench.Foreground = (SolidColorBrush)App.Current.Resources["PhoneBackgroundBrush"];
                LanguageEnglish.Foreground = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];

                //App.WebMethods.SaveSettings();              // persistance
                App.WebMethods._databaseQuoteChanged = true; // notify changes
                CheckAppLanguage();
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
            email.Body = "[Imagine] Report";
            await EmailManager.ShowComposeNewEmailAsync(email);
        }

        private void BoxName_GotFocus(object sender, RoutedEventArgs e)
        {
            SolidColorBrush brush = (SolidColorBrush)Resources["PhoneBackgroundBrush"];
            
            // Si le thème blanc est actif
            if (brush.Color.Equals(Colors.White))
            {
            }
            else
            {
                // Sinon, si le thème noir est actif
                BoxName.Foreground = (Brush)Resources["PhoneBackgroundBrush"];
            }
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

                    App.WebMethods._databaseQuoteChanged = true; // notify changes

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

        /// <summary>
        /// Déclenche un évènement -> une animation quand l'utilisateur tappe sur le stackpanel.
        /// Si le contrôle est réduit, l'animation l'agrandit et vice versa.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackUsernameHeader_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (StackUsername.Height < 50)
                ExpendOrMinimize(StackUsername, CloseUsername, 120);
            else ExpendOrMinimize(StackUsername, CloseUsername, 32);
        }

        /// <summary>
        /// Déclenche un évènement -> une animation quand l'utilisateur tappe sur le stackpanel.
        /// Si le contrôle est réduit, l'animation l'agrandit et vice versa.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackLanguageHeader_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (StackLanguage.Height < 50)
                ExpendOrMinimize(StackLanguage, CloseLanguage, 150);
            else ExpendOrMinimize(StackLanguage, CloseLanguage, 32);
        }

        /// <summary>
        /// Déclenche un évènement -> une animation quand l'utilisateur tappe sur le stackpanel.
        /// Si le contrôle est réduit, l'animation l'agrandit et vice versa.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackDynamictileHeader_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (StackDynamictile.Height < 50)
                ExpendOrMinimize(StackDynamictile, CloseDynamictile, 190);
            else ExpendOrMinimize(StackDynamictile, CloseDynamictile, 32);
        }

        /// <summary>
        /// Déclenche un évènement -> une animation quand l'utilisateur tappe sur le stackpanel.
        /// Si le contrôle est réduit, l'animation l'agrandit et vice versa.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackBackground_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (StackBackground.Height < 50)
                ExpendOrMinimize(StackBackground, CloseBackground, 190);
            else ExpendOrMinimize(StackBackground, CloseBackground, 32);
        }

        /// <summary>
        /// Etend ou réduit (selon la valeur passée en paramètre) le composant graphique.
        /// Anime également l'icône associée s'il est présent
        /// </summary>
        /// <param name="panel">Objet dont la taille doit être modifiée</param>
        /// <param name="closeIcon">Icône placée à côté du header, si elle existe</param>
        /// <param name="height">Nouvelle taille du panel</param>
        private void ExpendOrMinimize(DependencyObject panel, DependencyObject closeIcon, double height)
        {
            // ANIMATION DU PANEL
            if (panel == null) return;

            Storyboard sbStackExpend = new Storyboard();
            DoubleAnimationUsingKeyFrames animateStackExpend = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(500),
                EnableDependentAnimation = true,
                BeginTime = TimeSpan.FromMilliseconds(0)
            };
            LinearDoubleKeyFrame d1 = new LinearDoubleKeyFrame();
            d1.KeyTime = TimeSpan.FromMilliseconds(500);
            d1.Value = height;
            animateStackExpend.KeyFrames.Add(d1);

            Storyboard.SetTargetProperty(animateStackExpend, "(FrameworkElement.Height)");
            sbStackExpend.Children.Add(animateStackExpend);
            Storyboard.SetTarget(sbStackExpend, panel);
            sbStackExpend.Begin();


            // ANIMATION DE L'ICÔNE
            if (closeIcon == null) return;
            
            int from, to;
            if (height < 50) { from = 90; to = 45; }
            else { from = 45; to = 90; }

            Storyboard sbIconRotation = new Storyboard();
            DoubleAnimation animateIconRotation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                From = from,
                To = to,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(animateIconRotation, "(UIElement.RenderTransform).(CompositeTransform.Rotation)");
            sbIconRotation.Children.Add(animateIconRotation);
            Storyboard.SetTarget(sbIconRotation, closeIcon);
            sbIconRotation.Begin();
        }

        /// <summary>
        /// Vérifie si les icones doivent être remplacées en fonction du thème du téléphone
        /// </summary>
        private void CheckIconTheme()
        {
            SolidColorBrush brush = (SolidColorBrush)Resources["PhoneBackgroundBrush"];

            // Si le thème blanc est actif,
            // on change les icones
            if (brush.Color.Equals(Colors.White))
            {
                BitmapImage bmi = new BitmapImage();
                bmi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bmi.UriSource = new Uri("ms-appx:/Assets/Icons/iconCancel-black.png", UriKind.RelativeOrAbsolute);
                CloseUsername.Source = bmi;
                CloseLanguage.Source = bmi;
                CloseDynamictile.Source = bmi;
                CloseBackground.Source = bmi;
            }
        }
    }
}
