using Imagine.Common;
using Imagine.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
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
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Imagine.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Adminbackground : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        bool _editMode = false;
        Dailybackground _dailyToUpdate = null;

        public Adminbackground()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            LoadDailybackgrounds();
        }


        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (StackAddNew.Opacity == 1)
            {
                // Then the Add Stack is visible, hide it
                SlideToBottom(StackAddNew, StackList);

                // CommandBar
                AppBarButtonUpload.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                AppBarButtonAdd.Visibility = Windows.UI.Xaml.Visibility.Visible;

                e.Handled = true;
            }
        }


        private async void LoadDailybackgrounds()
        {
            //ListViewDailybackground.DataContext = App.WebMethods.CollectionBackgrounds;

            bool result = await App.WebMethods.GetAllDailybackground();
            if (result)
            {
                if (App.WebMethods.CollectionBackgrounds.Count > 0)
                {
                    TBNoEntry.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    ListViewDailybackground.DataContext = App.WebMethods.CollectionBackgrounds;
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Sorry, I couldn't get the Dailybackground items", "Oops!");
                dialog.ShowAsync();
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
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

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

        /// <summary>
        /// Animate two elements hiding the first and showing the last
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        private void SlideToBottom(DependencyObject o1, DependencyObject o2)
        {
            Slide(o1, o2, "bottom");
        }

        private void SlideToTop(DependencyObject o1, DependencyObject o2)
        {
            Slide(o1, o2, "top");
        }

        private void SlideToLeft(DependencyObject o1, DependencyObject o2)
        {
            Slide(o1, o2, "left");
        }

        private void SlideToRight(DependencyObject o1, DependencyObject o2)
        {
            Slide(o1, o2, "right");
        }


        private void Dailybackground_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        /// <summary>
        /// Add a new Dailybackground entry
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppBarButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            // Show the Add Panel
            SlideToTop(StackList, StackAddNew);

            // CommandBar
            AppBarButtonUpload.Visibility = Windows.UI.Xaml.Visibility.Visible;
            AppBarButtonAdd.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void AppBarButtonUpload_Click(object sender, RoutedEventArgs e)
        {
            if (BoxDailybackground.Text == null || BoxDailybackground.Text == "") return;

            Dailybackground bg = new Dailybackground();
            bg.url = BoxDailybackground.Text;

            //if (!VerifyDateIsFuture(DatePickerBackground.Date))
            //{
            //    MessageDialog dialog = new MessageDialog("The date must be later than today", "Oops!");
            //    dialog.ShowAsync();
            //    return;
            //}

            bg.day = DatePickerBackground.Date.DateTime;

            bool result = false;

            if (!_editMode)
            {
                // Add a new background if we're not in edit mode
                result = await App.WebMethods.InsertDailybackground(bg);
            }
            else
            {
                // We're in edit mode and we want to update an entry
                bg = _dailyToUpdate;
                result = await App.WebMethods.UpdateDailybackground(bg);
                _editMode = false; // don't forget to set the var at false
            }

            if (result)
            {
                // Back to the backgrounds list
                SlideToBottom(StackAddNew, StackList);

                // Refresh data
                LoadDailybackgrounds();

                // Show result
                MessageDialog dialog = new MessageDialog("The background entry has been correctly inserted!", "Success");
                dialog.ShowAsync();
            }
            else
            {
                MessageDialog dialog = new MessageDialog("There was an error. Re-try in a moment please.", "Oops!");
                dialog.ShowAsync();
            }
        }

        private void BoxDailybackground_GotFocus(object sender, RoutedEventArgs e)
        {
            BoxDailybackground.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void BoxDailybackground_LostFocus(object sender, RoutedEventArgs e)
        {
            BoxDailybackground.Foreground = new SolidColorBrush(Colors.White);
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            if (item != null)
            {
                _dailyToUpdate = null;
                _dailyToUpdate = item.DataContext as Dailybackground;

                // Show EDIT Panel
                SlideToTop(StackList, StackAddNew);
                BoxDailybackground.Text = _dailyToUpdate.url;

                AppBarButtonUpload.Visibility = Windows.UI.Xaml.Visibility.Visible;
                AppBarButtonAdd.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                _editMode = true;
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            if (item != null)
            {
                Dailybackground bg = item.DataContext as Dailybackground;
                bool result = await App.WebMethods.DeleteDailybackground(bg);

                if (result)
                {
                    // Refresh data
                    App.WebMethods.CollectionBackgrounds.Remove(bg);

                    // Show dialog
                    MessageDialog dialog = new MessageDialog("The entry has been deleted", "Done");
                    dialog.ShowAsync();
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("There was a problem. Try again in a moment", "Oops!");
                    dialog.ShowAsync();
                }
            }
        }

        private bool VerifyDateIsFuture(DateTimeOffset date)
        {
            if (date >= DateTimeOffset.Now)
            {
                return true;
            }
            return false;
        }

        private void Item_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Thumbnail_Loading(object sender, RoutedEventArgs e)
        {

        }

        private void Thumbnail_Loaded(object sender, RoutedEventArgs e)
        {
            Image thumbnail = (Image)sender;

            if (thumbnail != null)
            {
                StackPanel stack = (StackPanel)thumbnail.Parent;

                if (stack != null)
                {
                    ProgressBar pb = (ProgressBar)stack.Children[0];
                    pb.IsEnabled = false;
                    pb.IsIndeterminate = false;
                    pb.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }
    }
}
