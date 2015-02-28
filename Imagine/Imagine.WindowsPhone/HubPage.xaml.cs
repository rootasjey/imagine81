using Imagine.Common;
using Imagine.Data;
using Imagine.Pages;
using Imagine.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
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

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace Imagine
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        /// <summary>
        /// Create sequenced animation for ListView with a delay
        /// </summary>
        private double _delay = 0;

        // <summary>
        /// Make the link between the quote and the sharing ui
        /// </summary>
        private Quote _quoteToShare = null;

        public HubPage()
        {
            this.InitializeComponent();

            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            // For the AppBar transparency
            CommandBarHubPage.Opened += CommandBarHubPage_Opened;
            CommandBarHubPage.Closed += CommandBarHubPage_Closed;
        }

        void CommandBarHubPage_Closed(object sender, object e)
        {
            CommandBarHubPage.Opacity = 0;
        }

        void CommandBarHubPage_Opened(object sender, object e)
        {
            CommandBarHubPage.Opacity = 1;
        }

        void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (GridSearchHub.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                SlideToBottom(GridSearchHub, Hub);
                e.Handled = true;
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
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroups = await SampleDataSource.GetGroupsAsync();
            this.DefaultViewModel["Groups"] = sampleDataGroups;

            LoadMainView();
        }

        private async void LoadMainView()
        {
            // Load main data if it isn't loaded
            if (!App.WebMethods.IsDataLoaded)
            {
                StartProgressRing();

                // Show loading text
                SbLoadingText.Begin();
                SbLoadingText.RepeatBehavior = Windows.UI.Xaml.Media.Animation.RepeatBehavior.Forever;

                await App.WebMethods.LoadData();
                PopulateHubOne();
                PopulateHubTwo();
                PopulateHubThree();
                ChooseRandomWall();
                ShowAdminDailybackground();
            }

            ReinitializeAnimationDelay();
        }

        /// <summary>
        /// Show ProgressRing in HubSections
        /// </summary>
        private void StartProgressRing()
        {
            // Show progress ring
            var pr = Helpers.FindVisualChild<ProgressRing>(HubToday);
            if (pr == null) return;
            pr.IsActive = true;
            pr.Visibility = Windows.UI.Xaml.Visibility.Visible;

            var pr2 = Helpers.FindVisualChild<ProgressRing>(HubBefore);
            if (pr2 == null) return;
            pr2.IsActive = true;
            pr2.Visibility = Windows.UI.Xaml.Visibility.Visible;

            var pr3 = Helpers.FindVisualChild<ProgressRing>(HubFavorites);
            if (pr3 == null) return;
            pr3.IsActive = true;
            pr3.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void ShowAdminDailybackground()
        {
            if (App.WebMethods.CheckAdmin().Result)
            {
                Loadbg_Command.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else Loadbg_Command.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        /// <summary>
        /// Populate Today hub with the today's quote
        /// </summary>
        private void PopulateHubOne()
        {
            HubToday.DataContext = App.WebMethods.ListSingleItem;

            // Hide progress ring
            var pr = Helpers.FindVisualChild<ProgressRing>(HubToday);
            pr.IsActive = false;
            pr.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            // Hide Loading text
            SbLoadingText.Stop();
            TBLoading.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            // Update the main tile
            UpdateTile();
        }


        /// <summary>
        /// Reinit the delay animation value
        /// </summary>
        private void ReinitializeAnimationDelay()
        {
            _delay = 0;
        }

        /// <summary>
        /// Fired when the each Item(Template) is loaded into the ListView DataTemplate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemToday_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanel stack = (StackPanel)sender;
            Grid grid = (Grid)stack.Children.ElementAt(0);

            if (grid.Opacity != 0) return;

            // ANIME THE CONTENT (OF STACKPANEL)
            // 1 - Anime the quote
            Storyboard sbQuote = new Storyboard();
            DoubleAnimation moveToRight = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                From = -100,
                To = 0,
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation darkToLight = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                From = 0,
                To = 1,
                EasingFunction =
                new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(darkToLight, "Opacity");
            Storyboard.SetTargetProperty(moveToRight, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
            sbQuote.Children.Add(darkToLight);
            sbQuote.Children.Add(moveToRight);

            //Grid grid = (Grid)stack.Children.ElementAt(0);
            grid.RenderTransform = new CompositeTransform();
            Storyboard.SetTarget(sbQuote, grid);
            sbQuote.BeginTime = TimeSpan.FromMilliseconds(700);
            sbQuote.Begin();


            // 2 - Anime the author
            Storyboard sbAuthor = new Storyboard();
            DoubleAnimation dropDownA = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                From = -40,
                To = 0,
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation lightsUP = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                From = 0,
                To = 1,
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(lightsUP, "Opacity");
            Storyboard.SetTargetProperty(dropDownA, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            sbAuthor.Children.Add(lightsUP);
            sbAuthor.Children.Add(dropDownA);

            TextBlock TBAuthor = (TextBlock)stack.Children.ElementAt(1);
            TBAuthor.RenderTransform = new CompositeTransform();
            Storyboard.SetTarget(sbAuthor, TBAuthor);
            sbAuthor.BeginTime = TimeSpan.FromMilliseconds(1000);
            sbAuthor.Begin();


            // 3 - Anime the Reference
            Storyboard sbReference = new Storyboard();
            DoubleAnimation dropDownB = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                From = -40,
                To = 0,
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation lightsUP2 = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                From = 0,
                To = 1,
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(lightsUP2, "Opacity");
            Storyboard.SetTargetProperty(dropDownB, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            sbReference.Children.Add(lightsUP2);
            sbReference.Children.Add(dropDownB);

            TextBlock TBReference = (TextBlock)stack.Children.ElementAt(2);
            TBReference.RenderTransform = new CompositeTransform();
            Storyboard.SetTarget(sbReference, TBReference);
            sbReference.BeginTime = TimeSpan.FromMilliseconds(1300);
            sbReference.Begin();

            
            // 4 - Anime the Rectangle
            Storyboard sbRectangle = new Storyboard();
            DoubleAnimation dropDownC = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                From = -40,
                To = 0,
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation lightsUP3 = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                From = 0,
                To = 1,
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(lightsUP3, "Opacity");
            Storyboard.SetTargetProperty(dropDownC, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            sbRectangle.Children.Add(lightsUP3);
            sbRectangle.Children.Add(dropDownC);

            Windows.UI.Xaml.Shapes.Rectangle rect = (Windows.UI.Xaml.Shapes.Rectangle)grid.Children[0];
            rect.RenderTransform = new CompositeTransform();
            Storyboard.SetTarget(sbRectangle, rect);
            sbRectangle.BeginTime = TimeSpan.FromMilliseconds(1500);
            sbRectangle.Begin();
        }

        /// <summary>
        /// Hub Before => Fill the list view with a few quotes
        /// </summary>
        private void PopulateHubTwo()
        {
            HubBefore.DataContext = App.WebMethods._quotes;

            // Hide progress ring
            var pr = Helpers.FindVisualChild<ProgressRing>(HubBefore);
            pr.IsActive = false;
            pr.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        /// <summary>
        /// Fired when the each Item(Template) is loaded into the ListView DataTemplate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemBefore_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanel stack = (StackPanel)sender;
            if (stack.Opacity != 0) return;


            // ANIME EACH QUOTE
            Storyboard sb = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(1000),
                From = -90,
                To = 0,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(animation, "(UIElement.Projection).(PlaneProjection.RotationY)");
            sb.Children.Add(animation);

            animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(1000),
                From = 0,
                To = 1,
                EasingFunction =
                    new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(animation, "Opacity");
            sb.Children.Add(animation);

            stack.Projection = new PlaneProjection();
            Storyboard.SetTarget(sb, stack);
            sb.BeginTime = TimeSpan.FromMilliseconds(_delay);
            sb.Begin();

            _delay += 200;
        }

        /// <summary>
        /// Fill the list view with favorites quotes
        /// </summary>
        private void PopulateHubThree()
        {
            HubFavorites.DataContext = App.WebMethods.CollectionFavorites;

            // Hide progress ring
            var pr = Helpers.FindVisualChild<ProgressRing>(HubFavorites);
            pr.IsActive = false;
            pr.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            // Test if we must hide the default TB
            RefreshFavoritesVoid();

            // Refresh favorites => check if changed online
            App.WebMethods.RefreshFavorites();
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
            // TODO: Save the unique state of the page here.
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
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            if (App.WebMethods.databaseQuoteChanged)
            {
                Refresh_Command_Click(null, null);
                App.WebMethods.databaseQuoteChanged = false;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Remove the handler before we leave!
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        private void Share_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;

            if (item != null)
            {
                Quote q = item.DataContext as Quote;
                _quoteToShare = q;

                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.ShareTextHandler);
                Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
            }
        }

        private void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            string title = "";
            string description = "";
            string de = "de";

            if (App.WebMethods.applicationLanguage == "FR")
            {
                title = "Citation : ";
                description = "Citation d'Imagine sur Windows Phone";
            }
            else if (App.WebMethods.applicationLanguage == "EN")
            {
                title = "Quote";
                de = "by";
                description = "Imagine's quote on Windows Phone";
            }

            string result = _quoteToShare.Content + " " + de + " " + _quoteToShare.User;

            // The Title is mandatory
            request.Data.Properties.Title = title;
            request.Data.Properties.Description = description;
            request.Data.SetText(result);

            _quoteToShare = null; // Clear values
        }

        private void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;

            if (item != null)
            {
                Quote q = item.DataContext as Quote;
                App.WebMethods.CollectionFavorites.Add(q);

                RefreshFavoritesVoid();
                SaveFavorites();
            }
        }

        private void Quote_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        /// <summary>
        /// Refresh favorites quotes
        /// </summary>
        private void RefreshFavoritesVoid()
        {
            if (App.WebMethods.CollectionFavorites.Count > 0)
            {
                var stack = Helpers.FindVisualChild<StackPanel>(HubFavorites);
                stack.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                var stack = Helpers.FindVisualChild<StackPanel>(HubFavorites);
                stack.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        /// <summary>
        /// Save favorites quotes to IO
        /// </summary>
        private void SaveFavorites()
        {
            App.WebMethods.SaveFavorites();
        }

        private void RemoveFromFavorites_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;

            if (item != null)
            {
                Quote q = item.DataContext as Quote;
                App.WebMethods.CollectionFavorites.Remove(q);

                RefreshFavoritesVoid();
                SaveFavorites();
            }
        }

        private void AppBackground_ImageOpened(object sender, RoutedEventArgs e)
        {
            //MessageDialog dialog = new MessageDialog("completed");
            //dialog.ShowAsync();
        }

        private void AppBackground_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            //MessageDialog dialog = new MessageDialog("failed");
            //dialog.ShowAsync();
        }

        /// <summary>
        /// Navigate to the Create page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Create_Command_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Create));
        }

        /// <summary>
        /// Navigate to the Settings page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_Command_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Settings));
        }

        /// <summary>
        /// Reload the informations of the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_Command_Click(object sender, RoutedEventArgs e)
        {
            // Clear values
            App.WebMethods._quotes.Clear();
            App.WebMethods.CollectionFavorites.Clear();
            App.WebMethods.CollectionPersonal.Clear();
            App.WebMethods.ListSingleItem.Clear();
            App.WebMethods.RefreshPurpose();

            LoadMainView();
        }

        private void Loadbg_Command_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Imagine.Pages.Adminbackground));
        }

        private async void ChooseRandomWall()
        {
            // Vérifie que l'utilisateur a choisi cette option
            if (!App.WebMethods._dynamicBackground) return;


            Dailybackground bg = await App.WebMethods.GetTodayBackground();
            if (bg == null) return;

            Uri uri = new Uri(bg.url, UriKind.Absolute);

            BitmapImage bmi = new BitmapImage();
            bmi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bmi.UriSource = uri;
            AppBackground.ImageSource = bmi;

            //Random rand = new Random();
            //int n = rand.Next(1, 4);
            //string path = "/Assets/Backgrounds/bg" + n.ToString() + ".jpg";
            //AppBackground.ImageSource = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        }


        /// <summary>
        /// Fonction dupliquée de Create.xaml.cs
        /// Animation de transition entre deux éléments
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        /// <param name="direction"></param>
        private void Slide(DependencyObject o1, DependencyObject o2, string direction)
        {
            Type type = typeof(Grid);
            string translateAxe = "";
            double fromO1, fromO2, toO1, toO2;
            fromO1 = fromO2 = toO1 = toO2 = 0;


            if (direction == "top" || direction == "bottom")
            {
                translateAxe = "(UIElement.RenderTransform).(CompositeTransform.TranslateY)";

                if (direction == "bottom")
                {
                    fromO1 = 0;
                    toO1 = 300;
                    fromO2 = -300;
                    toO2 = 0;
                }
                else if (direction == "top")
                {
                    fromO1 = 0;
                    toO1 = -300;
                    fromO2 = 300;
                    toO2 = 0;
                }

            }
            else if (direction == "left" || direction == "right")
            {
                translateAxe = "(UIElement.RenderTransform).(CompositeTransform.TranslateX)";

                if (direction == "left")
                {
                    fromO1 = 0;
                    toO1 = -300;
                    fromO2 = 300;
                    toO2 = 0;
                }
                else if (direction == "right")
                {
                    fromO1 = 0;
                    toO1 = 300;
                    fromO2 = -300;
                    toO2 = 0;
                }
            }


            // Hide first object
            Storyboard sb = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                From = fromO1,
                To = toO1,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(animation, translateAxe);
            sb.Children.Add(animation);

            animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                From = 1,
                To = 0,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(animation, "Opacity");
            sb.Children.Add(animation);

            Storyboard.SetTarget(sb, o1);

            sb.Completed += delegate
            {
                // When the animation completes, collapse the object
                Type oT1 = o1.GetType();
                if (oT1 == type)
                {
                    Grid gr1 = (Grid)o1;
                    gr1.Opacity = 0;
                    gr1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            };
            sb.Begin();


            // Check Visibility for the object to show
            Type oT2 = o2.GetType();

            if (oT2 == type)
            {
                Grid gr2 = (Grid)o2;
                gr2.Opacity = 0;
                gr2.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            // --------------------------------------


            Storyboard sb2 = new Storyboard();
            DoubleAnimation animation2 = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                From = fromO2,
                To = toO2,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(animation2, translateAxe);
            sb2.Children.Add(animation2);

            animation2 = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                From = 0,
                To = 1,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(animation2, "Opacity");
            sb2.Children.Add(animation2);

            Storyboard.SetTarget(sb2, o2);
            sb2.Begin();
        }

        private void SlideToBottom(DependencyObject o1, DependencyObject o2)
        {
            Slide(o1, o2, "bottom");
        }

        private void SlideToTop(DependencyObject o1, DependencyObject o2)
        {
            Slide(o1, o2, "top");
        }


        private void Search_Command_Click(object sender, RoutedEventArgs e)
        {
            SlideToTop(Hub, GridSearchHub);
        }

        private void ItemSearch_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void UpdateTile()
        {
            string imagePath = "Assets\\Icons\\Logo.scale-240.png";
            string text = App.WebMethods.ListSingleItem.First().Content;
            Common.TileSetter.CreateTiles(imagePath, imagePath, text);
        }
    }
}