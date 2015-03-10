using Imagine.Common;
using Imagine.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
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
    public sealed partial class Create : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// From 1 to 3, indicates which step to create a quote/wordflow
        /// </summary>
        private int _step = 0;

        /// <summary>
        /// Will be whether a quote or a wordflow text
        /// </summary>
        private ObjectCreation objectCreation = new ObjectCreation();

        /// <summary>
        /// Create sequenced animation for ListView with a delay
        /// </summary>
        private double _delay = 0;

        // <summary>
        /// Make the link between the quote and the sharing ui
        /// </summary>
        private Quote _quoteToShare;

        /// <summary>
        /// Save the quote to delete => Make the link when the user confirms the deletion
        /// </summary>
        private Quote _quoteToDelete;

        /// <summary>
        /// True when the user wants to update a quote
        /// </summary>
        private bool _updateMode = false;

        private string _personalLang = "all";
        private string _personalChronology = "old first";


        public Create()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            CommandBarCreate.Opened += CommandBarCreate_Opened;
            CommandBarCreate.Closed += CommandBarCreate_Closed;

            ListViewPersonal.ItemsSource = App.WebMethods.CollectionPersonal; // binding
            RestoreStateData();
            InitializeCommandBar();
        }


        /// <summary>
        /// Custom scenarios when the backkey is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (GridCreate.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                if (_step == 2)
                {
                    // on est à l'étape 2
                    SlideToRight(GridWrite, GridCreateChoice);
                    AppBarButtonAccept.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    // Dans le cas où on serait en mode édition
                    // on veut supprimer les valeus si l'utilisateur retourne en arrière
                    if (_updateMode)
                    {
                        _updateMode = false;
                        ResetCreatedValues();
                    }

                    _step--;
                    e.Handled = true;
                }
                else if (_step == 3)
                {
                    // on est à l'étape 3
                    SlideToRight(GridTags, GridWrite);
                    AppBarButtonAccept.Icon = new SymbolIcon(Symbol.Accept); // Put back the accept icon

                    _step--;
                    e.Handled = true;
                }
            }
            else if (GridPersonal.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                // Les citations personnelles sont affichées
                // On veut retourner à la page de création
                Personal_Command.Label = "book";
                SlideToBottom(GridPersonal, GridCreate);
                CommandBarCreateMode();

                if (_step == 3)
                {
                    _step = 1;
                    SlideToLeft(GridWrite, GridCreateChoice);
                }
                
                e.Handled = true;
            }
            else if (GridAuthentificate.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                SlideToBottom(GridAuthentificate, GridCreate);
                Connect_Command.Label = "connect";

                CommandBarCreateMode();
                e.Handled = true;
            }
        }
        
        void CommandBarCreate_Closed(object sender, object e)
        {
            if (_step == 3) { CommandBarCreate.Opacity = 1; return; }
            CommandBarCreate.Opacity = 0;
        }

        void CommandBarCreate_Opened(object sender, object e)
        {
            CommandBarCreate.Opacity = 1;
        }


        private void InitializeCommandBar()
        {
            if (App.WebMethods._isAuthenticated && (App.WebMethods._user != null))
            {
                // If we're already connected,show disconnect Command
                Connect_Command.Label = "disconnect";
            }
            else
            {
                Connect_Command.Label = "connect";
                App.WebMethods._user = null;
            }
        }

        /// <summary>
        /// Set local variables from IO
        /// </summary>
        private async void RestoreStateData()
        {
            PopulateTags();
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
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            await PassAuth();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Remove the handler before we leave!
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        /// <summary>
        /// Decides if the Microsoft authenfication screen is showed to the user
        /// </summary>
        private async Task PassAuth()
        {
            if (App.WebMethods._firstApplicationLaunch)
            {
                GridAuthentificate.Opacity = 1;
                GridAuthentificate.Visibility = Windows.UI.Xaml.Visibility.Visible;
                App.WebMethods._firstApplicationLaunch = false;
            }
            else
            {
                if (_step == 0) _step++;
                CheckVisibility();

                // Show CommandBar
                CommandBarCreate.Visibility = Windows.UI.Xaml.Visibility.Visible;
                AppBarButtonAccept.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void CheckVisibility()
        {
            if(GridCreate.Visibility == Windows.UI.Xaml.Visibility.Collapsed
                && GridPersonal.Visibility == Windows.UI.Xaml.Visibility.Collapsed
                && GridAuthentificate.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
            {
                GridCreate.Visibility = Windows.UI.Xaml.Visibility.Visible;

                if (GridCreateChoice.Visibility == Windows.UI.Xaml.Visibility.Collapsed
                    && GridWrite.Visibility == Windows.UI.Xaml.Visibility.Collapsed
                    && GridTags.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
                {
                    GridCreateChoice.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
            else if (GridCreateChoice.Visibility == Windows.UI.Xaml.Visibility.Collapsed
                   && GridWrite.Visibility == Windows.UI.Xaml.Visibility.Collapsed
                   && GridTags.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
            {
                GridCreateChoice.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        /// <summary>
        /// Fire the OAuth authentification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonAuthentificate_Tap(object sender, RoutedEventArgs e)
        {
            ShowProgressTop("Sign into your Microsoft account...");

            bool result = await App.WebMethods.Signin();

            if (_step < 2)
                _step = 1;

            if (result)
            {
                App.WebMethods.SetUserAttributes();

                // Insert user in the database
                bool exists = await App.WebMethods.UserAlreadyexists(App.WebMethods._user.Msid);
                if (exists)
                {
                    // Get his quotes/wordflows
                    ShowProgressTop("Getting your personal stuff back...");
                    App.WebMethods.GetPersonalOnline();
                }
                else
                {
                    // Insert a new user
                    ShowProgressTop("Creating a new user account...");
                    bool inserted = await App.WebMethods.InsertUser(App.WebMethods._user);

                    if (inserted)
                    {
                        MessageDialog dialog = new MessageDialog("You're account has been created! You can start using the full potential of Imagine");
                        dialog.ShowAsync();
                    }
                    else
                    {
                        MessageDialog dialog = new MessageDialog("Sorry, there was a problem and we could not create your user account. Re-try in a moment", "Oops");
                        dialog.ShowAsync();
                    }
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Sorry, there was a problem connecting your account. Re-try in a moment", "Oops");
                dialog.ShowAsync();
            }

            GridCreateChoice.Visibility = Windows.UI.Xaml.Visibility.Visible;
            SlideToBottom(GridAuthentificate, GridCreate);

            // Show CommandBar
            CommandBarCreate.Visibility = Windows.UI.Xaml.Visibility.Visible;
            AppBarButtonAccept.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (App.WebMethods._isAuthenticated)
            {
                Connect_Command.Label = "disconnect";
                App.WebMethods.SaveSettings();
            }

            HideProgressTop();
        }

        private void ShowProgressTop(string text = "Doing some stuff")
        {
            ProgressText.Text = text;
            ProgressText.Visibility = Windows.UI.Xaml.Visibility.Visible;

            ProgressTop.IsEnabled = true;
            ProgressTop.IsIndeterminate = true;
            ProgressTop.Visibility = Windows.UI.Xaml.Visibility.Visible;

            LayoutRoot.Opacity = 0.5;
            LayoutRoot.IsHitTestVisible = false;
        }

        private void HideProgressTop()
        {
            ProgressText.Text = "";
            ProgressText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            ProgressTop.IsEnabled = false;
            ProgressTop.IsIndeterminate = false;
            ProgressTop.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            LayoutRoot.Opacity = 1;
            LayoutRoot.IsHitTestVisible = true;
        }
        
        /// <summary>
        /// Ignore OAuth and go to the personal hub
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonStayAnonymous_Click(object sender, RoutedEventArgs e)
        {
            if (_step < 2)
            {
                _step = 1;
            }
            
            GridCreateChoice.Visibility = Windows.UI.Xaml.Visibility.Visible;
            SlideToBottom(GridAuthentificate, GridCreate);

            // Show CommandBar
            CommandBarCreate.Visibility = Windows.UI.Xaml.Visibility.Visible;
            AppBarButtonAccept.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (App.WebMethods._isAuthenticated)
            {
                App.WebMethods._isAuthenticated = false;
                App.WebMethods._user = null;
            }

            App.WebMethods.SaveSettings();
                        
        }


        // ---------------
        // CREATE SECTION
        // ---------------
        /// <summary>
        /// Appen when the user clicks on create a quote
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCreateQuote_Tap(object sender, RoutedEventArgs e)
        {
            GoToStep2("quote");
        }

        /// <summary>
        /// Appen when the user clicks on create a wordflow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCreateWorldflow_Tap(object sender, RoutedEventArgs e)
        {
            //GoToStep2("wordflow");
            MessageDialog message = new MessageDialog("This will be available soon", "Sorry");
            message.ShowAsync();
        }


        // --------------
        // GOTO NEXT PAGE
        // --------------
        /// <summary>
        /// Go to the 2nd step : Redaction
        /// </summary>
        /// <param name="type"></param>
        private void GoToStep2(string type)
        {
            _step = 2;
            SlideToLeft(GridCreateChoice, GridWrite);
            objectCreation.Type = type;

            CommandBarCreate.Visibility = Windows.UI.Xaml.Visibility.Visible;
            CommandBarCreate.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
            AppBarButtonAccept.Visibility = Windows.UI.Xaml.Visibility.Visible;
            
        }

        /// <summary>
        /// Go to the step where the user selects tags, then upload the quote
        /// </summary>
        private void GoToStep3(string content)
        {
            _step = 3;

            // If the content is empty, don't pass
            if (content == null || content.Length < 3)
            {
                _step--;
                MessageDialog message = new MessageDialog("You didn't enter anything. Write something before going to the next step", "Hey");
                message.ShowAsync();
                return;
            }

            // Set the content & the reference attributes
            objectCreation.Content = BoxCreate.Text;
            objectCreation.Reference = BoxReference.Text;

            SlideToLeft(GridWrite, GridTags);

            AppBarButtonAccept.Icon = new SymbolIcon(Symbol.Upload);
            CommandBarCreate.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
            CommandBarCreate.Opacity = 1; // Permet d'avoir le bouton "valider" même quand on écrit un nouveau tag
        }


        #region tags functions

        private void PopulateTags()
        {
            // Don't add anything if it's already filled
            if (StackRecentTags.Children.Count > 0) return;

            // Crée les tags
            if (App.WebMethods.FavoritesTags.Count > 0)
            {
                // Si la liste des tags sauvegardés n'est pas vide
                foreach (string s in App.WebMethods.FavoritesTags)
                {
                    CreateFavoriteTag(s);
                }
            }
            else
            {
                // Sinon, crée une nouvelle liste
                // ------------------------------
                string[] tags = { "Pensé", "Science", "Vie", "Volonté", "Histoire" };
                CreateTags(tags);
            }
        }

        /// <summary>
        /// Crée un tag (TextBlock) dans un Grid
        /// et l'insert dans le ScrollViewer de tags récemment utilisés automatiquement
        /// </summary>
        private void CreateTags(string[] taglist)
        {
            foreach (string tag in taglist)
            {
                if (tag.Length < 2) continue;

                TextBlock block = new TextBlock();
                block.Text = tag;
                block.FontSize = 24;
                block.Margin = new Thickness(12, 0, 12, 0);

                Grid grid = new Grid();
                grid.Margin = new Thickness(6);
                grid.Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
                grid.Tag = tag;
                grid.Opacity = 0.5;

                grid.Children.Add(block);
                StackRecentTags.Children.Add(grid);

                // Event Tap
                //grid.Tapped -= RecentTag_Tap;
                grid.Tapped += RecentTag_Tap;
            }
        }

        private void CreateFavoriteTag(string tag)
        {
            Grid g = CreateSingleTag(tag, false);
            g.Opacity = 0.5;
            //g.Tapped -= RecentTag_Tap;
            g.Tapped += RecentTag_Tap;

            StackRecentTags.Children.Add(g);
        }

        /// <summary>
        /// Create a tag (Texblock in Grid) & return a Grid object
        /// </summary>
        /// <param name="tag">Tag's name</param>
        /// <param name="accentColor">If true, the color background will be PhoneAccentColor</param>
        /// <returns>Return a Grid object with the tag</returns>
        private Grid CreateSingleTag(string tag, bool accentColor)
        {
            if (tag == null || tag.Length < 2) return null;


            TextBlock block = new TextBlock();
            block.Text = tag;
            block.FontSize = 22;
            block.Margin = new Thickness(12, 0, 12, 0);

            Grid grid = new Grid();
            grid.Margin = new Thickness(6);
            grid.Tag = tag;

            if (accentColor)
            {
                block.Foreground = new SolidColorBrush(Colors.White);
                grid.Background = (Brush)Application.Current.Resources["PhoneAccentBrush"];
            }
            else
            {
                grid.Background = new SolidColorBrush(Colors.Transparent);
            }

            grid.Children.Add(block);

            return grid;
        }


        /// <summary>
        /// Se déclenche quand l'utilisateur tapote sur un tag
        /// parmi les tags récents
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecentTag_Tap(object sender, TappedRoutedEventArgs e)
        {
            Grid gridTapped = (Grid)sender;
            if (gridTapped == null) return;

            if (gridTapped.Opacity == 0.5)
            {
                gridTapped.Opacity = 1;
                AddSelectedTag(gridTapped.Tag.ToString());
            }
            else
            {
                gridTapped.Opacity = 0.5;
                RemoveSelectedTag(gridTapped.Tag.ToString());
            }
        }


        /// <summary>
        /// Ajoute le tag à la liste de tags de la citation/wordflow
        /// </summary>
        /// <param name="tag"></param>
        private void AddSelectedTag(string tag)
        {
            // Vérifie que le tag n'est pas nul
            if (tag == null || tag.Length < 2) return;

            // Vérifie si le tag a déjà été sélectionné
            foreach (Grid child in LVTagSelected.Items)
            {
                if (child.Tag.ToString() == tag)
                    return;
            }

            // Ajoute le tag à la liste (WrapPanel)
            Grid newTag = CreateSingleTag(tag, true);
            newTag.Tapped -= TagAdded_Tap;
            newTag.Tapped += TagAdded_Tap;

            //wrap.Children.Add(newTag);
            LVTagSelected.Items.Add(newTag);
        }

        /// <summary>
        /// Se déclenche quand l'utilisateur tapote
        /// sur un tag déjà ajouté à la liste de la citation/l'inspiration
        /// Et retire ce tag de la liste
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagAdded_Tap(object sender, TappedRoutedEventArgs e)
        {
            Grid gridTapped = (Grid)sender;
            if (gridTapped == null) return;
            RemoveSelectedTag(gridTapped);
        }



        /// <summary>
        /// Retire le tag de la liste de tags
        /// de la citation/ l'inspiration.
        /// Se déclenche quand on tape sur un tag récent déjà sélectionné
        /// OU sur un tag de la liste des tags sélectionnés de la citation/l'inspiration
        /// </summary>
        /// <param name="tag"></param>
        private void RemoveSelectedTag(object tag)
        {
            // Vérifie que le tag n'est pas nul
            if (tag == null) return;

            // Test si le tag est de type Grid
            // alors on a désélectionné un tag
            // de la liste des tags de la citation.
            if (tag.GetType() == typeof(Grid))
            {
                //var wrap = Helpers.FindVisualChild<WrapGrid>(LVTagSelected);

                if (LVTagSelected.Items.Count > 0)
                {
                    Grid gridToRemove = (Grid)tag;
                    
                    // Remet l'opacité à 0.5 du tag de la liste récente
                    foreach (Grid g in StackRecentTags.Children)
                    {
                        if (g.Tag.ToString() == gridToRemove.Tag.ToString())
                        {
                            g.Opacity = 0.5;
                            break;
                        }
                    }

                    //wrap.Children.Remove(gridToRemove);
                    LVTagSelected.Items.Remove(gridToRemove);

                }
            }

            // Sinon le tag est de type String,
            // et on a désélectionné un tag
            // de la liste des tags récents.
            else if (tag.GetType() == typeof(string) || tag.GetType() == typeof(String))
            {
                foreach (Grid child in LVTagSelected.Items)
                {
                    if (child.Tag.ToString() == tag.ToString())
                    {
                        LVTagSelected.Items.Remove(child);
                        break;
                    }
                }
            }
        }

        private void BoxCreate_TextChanged(object sender, TextChangedEventArgs e)
        {
            BlockWordCount.Text = BoxCreate.Text.Count().ToString();
        }


        /// <summary>
        /// Se produit quand une touche est enfoncée
        /// On veut surveillé quand l'utilisateur presse la touche Entrée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxTag_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Si l'utilisateur appuie sur la touche entrée
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                CreateTagFromKeyboard();
            }
        }

        private void AppBarButtonAddTag_Click(object sender, RoutedEventArgs e)
        {
            CreateTagFromKeyboard();
        }

        private void CreateTagFromKeyboard()
        {
            // On vérifie que le nom du tag souhaité
            // a au moins 3 caractères et ne contient pas de caractères spéciaux
            if (BoxTag.Text.Length < 3 || TextContainsSpecialCharacter(BoxTag.Text))
            {
                string message = "Le tag que vous souhaitez ajouter doit contenir au moins 3 caractères, et ne doit pas comporter de caractère spécial";

                MessageDialog dialog = new MessageDialog(message);
                dialog.ShowAsync();
                return;
            }

            // Et on crée un nouveau tag
            string tagName = BoxTag.Text.Substring(0, 1).ToUpper() + BoxTag.Text.Substring(1).Replace(" ", "");
            AddSelectedTag(tagName);    // Ajoute le tag à la citation/l'inspiration
            UpdateRecentTags(tagName);  // Ne pas oublié d'ajouter ce tag aux tags récemment utilisés
            BoxTag.Text = "";           // Vide le TextBox
        }


        /// <summary>
        /// Met à jour la liste des tags récents
        /// </summary>
        private void UpdateRecentTags(string tag)
        {
            // Vérifie si le tag est déjà présent? et le supprime
            foreach (Grid child in StackRecentTags.Children)
            {
                if (child.Tag.ToString() == tag)
                {
                    StackRecentTags.Children.Remove(child);
                    break;
                }
            }

            // Crée un nouveau tag
            Grid tagToAdd = CreateSingleTag(tag, false);
            //tagToAdd.Tapped -= RecentTag_Tap;
            tagToAdd.Tapped += RecentTag_Tap;

            // Insert le tag à la première place
            StackRecentTags.Children.Insert(0, tagToAdd);

            // Vérifie qu'on n'a pas plus de 10 tags
            if (StackRecentTags.Children.Count > 10)
            {
                StackRecentTags.Children.RemoveAt(10);
            }
        }

        /// <summary>
        /// Vérifie si le text passé en argument
        /// contient des caractères spéciaux (&, @, ?, §, ...)
        /// </summary>
        /// <param name="text">Text à prendre en entrée</param>
        /// <returns>Retourne vrai si le texte contient des caractères spéciaux, faux sinon</returns>
        private bool TextContainsSpecialCharacter(string text)
        {
            Regex regex = new Regex("(&|²|\\.|,|;|:|!|\\?|\\*|\\{|\\}|\\[|\\]|\\(|\\)|~|@|'|\"|\\#|/|\\|°|=|\\+|/|<|>|\\^|§|\\$|£|%|¤|€|\\|)");
            MatchCollection matches = regex.Matches(text);

            if (matches.Count > 0)
            {
                return true;
            }
            else return false;
        }

        private void BoxTag_Tap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void BoxCreate_GotFocus(object sender, RoutedEventArgs e)
        {
            // On remet la couleur de texte par défaut
            BoxCreate.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void BoxCreate_LostFocus(object sender, RoutedEventArgs e)
        {
            // On définit la couleur du texte à blanc
            SolidColorBrush color = (SolidColorBrush)Resources["PhoneBackgroundBrush"];
            if (color.Color.Equals(Color.FromArgb(255, 255, 255, 255)))
            {

            }
            else  BoxCreate.Foreground = new SolidColorBrush(Colors.White);
        }

        private void GridReference_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Allow the user to write a reference
            if (BlockReference.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                BlockReference.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                BoxReference.Visibility = Windows.UI.Xaml.Visibility.Visible;
                BoxReference.Focus(Windows.UI.Xaml.FocusState.Keyboard);
            }
            else // hide the TextBox
            {
                BlockReference.Visibility = Windows.UI.Xaml.Visibility.Visible;
                BoxReference.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                if (BoxReference.Text.Length > 1)
                {
                    BlockReference.Text = BoxReference.Text;
                    objectCreation.Reference = BlockReference.Text;
                    //BlockReference.Foreground = new SolidColorBrush(Colors.White);
                }
            }
        }

        private void BoxReference_LostFocus(object sender, RoutedEventArgs e)
        {
            // Masque le TextBox de la référence s'il est visible
            GridReference_Tapped(null, null);
        }

        /// <summary>
        /// Se déclenche quand le TextBox pour créer un tag obtient le focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxTag_GotFocus(object sender, RoutedEventArgs e)
        {
            // Affiche le bouton de création de tag de la Commandbar
            // Et masque le bouton d'upload de la citation
            AppBarButtonAccept.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            AppBarButtonAddTag.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        /// <summary>
        /// Se déclenche quand le TextBox pour créer un tag perd le focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxTag_LostFocus(object sender, RoutedEventArgs e)
        {
            // Affiche le bouton de création de tag de la Commandbar
            // Et masque le bouton d'upload de la citation
            AppBarButtonAccept.Visibility = Windows.UI.Xaml.Visibility.Visible;
            AppBarButtonAddTag.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        #endregion tags functions




        private void Connect_Command_Click(object sender, RoutedEventArgs e)
        {
            if (!App.WebMethods._isAuthenticated)
            {
                // We're not connected, we show login screen
                if (GridAuthentificate.Opacity != 0) return;
                SlideToTop(GridCreate, GridAuthentificate);
                Connect_Command.Label = "connect";

                CommandBarAuthentificateMode();
            }
            else
            {
                // We're already connected => we want to disconnect
                Connect_Command.Label = "connect";
                App.WebMethods._isAuthenticated = false;
                App.WebMethods._user = null;
                App.WebMethods.SaveSettings();

                CommandBarCreateMode();

                // Show a message to the user
                MessageDialog dialog = new MessageDialog("You've been disconnected from the application", "Disconnected");
                dialog.ShowAsync();
            }
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
        /// Anime un élément vers une direction dans l'ombre (sans ce que cela ne se voit sur l'UI)
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="direction"></param>
        private void ShadowMoveFrom(DependencyObject o1, string direction)
        {
            Type type = typeof(Grid);
            string translateAxe = "";
            double fromO1, toO1;
            fromO1 = toO1 = 0;

            // Hide the object
            Type oT1 = o1.GetType();
            if (oT1 == type)
            {
                Grid gr1 = (Grid)o1;
                gr1.Opacity = 0;
                gr1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }


            if (direction == "top" || direction == "bottom")
            {
                translateAxe = "(UIElement.RenderTransform).(CompositeTransform.TranslateY)";

                if (direction == "bottom")
                {
                    fromO1 = 300;
                    toO1 = 0;
                }
                else if (direction == "top")
                {
                    fromO1 = -300;
                    toO1 = 0;
                }

            }
            else if (direction == "left" || direction == "right")
            {
                translateAxe = "(UIElement.RenderTransform).(CompositeTransform.TranslateX)";

                if (direction == "left")
                {
                    fromO1 = -300;
                    toO1 = 0;
                }
                else if (direction == "right")
                {
                    fromO1 = 300;
                    toO1 = 0;
                }
            }


            // Hide
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

            sb.Begin();
        }

        private void AnimateBackground(DependencyObject o1, Windows.UI.Color color2)
        {
            // Hide first object
            Storyboard sb = new Storyboard();

            ColorAnimation animation = new ColorAnimation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                //From = color1,
                To = color2,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(animation, o1);
            Storyboard.SetTargetProperty(animation, "Background");
            sb.Children.Add(animation);
            
            sb.Begin();
        }

        private void ButtonQuote_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Grid g = (Grid)sender;
            if (g != null) g.Background = (SolidColorBrush)Resources["PhoneAccentBrush"];
        }

        private void ButtonQuote_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Grid g = (Grid)sender;
            if (g != null) g.Background = (SolidColorBrush)Resources["PhoneChromeBrush"];
        }

        /// <summary>
        /// Steps validation (AppBar button)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AppBarButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            // Then go to the next step
            switch (_step)
            {
                case 2:
                    objectCreation.Content = BoxCreate.Text;
                    GoToStep3(objectCreation.Content);
                    break;
                case 3:
                    // Message confirmation
                    MessageDialog dialog = new MessageDialog("Do you really want to upload this quote online?", "Confirmation");
                    dialog.Commands.Add(new UICommand(
                        "Sure!",
                        new UICommandInvokedHandler(this.CommandInvokedConfirmUpload)));
                    dialog.Commands.Add(new UICommand(
                        "Nope!",
                        new UICommandInvokedHandler(this.CommandInvokedConfirmUpload)));

                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;
                    dialog.ShowAsync();

                    
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Decides if the user confirm the upload or not
        /// </summary>
        /// <param name="command"></param>
        private void CommandInvokedConfirmUpload(IUICommand command)
        {
            string result = command.Label;
            if (result == "Sure!")
            {
                Upload();
            }
        }

        /// <summary>
        /// Upload the quote/wordflow to the servers
        /// </summary>
        private async void Upload()
        {
            // Animation
            AnimationUpload("on");
            SlideToTop(GridTags, GridUpload);

            // Get & Set the tags
            await SetTags();
            await SetUser();
            

            // Decides to create a Quote or a Wordflow
            if (objectCreation.Type == "quote")
            {
                UploadQuote();
            }
            else if (objectCreation.Type == "wordflow")
            {
                
            }
        }

        /// <summary>
        /// Show an animation to the user which indicates the progress
        /// </summary>
        private void AnimationUpload(string direction)
        {
            if (direction == "on")
            {
                // Show progress
                PRUpload.IsActive = true;
            }
            else
            {
                // Show progress
                PRUpload.IsActive = false;
            }
            
        }

        private async Task SetTags()
        {
            objectCreation.Tag = "";

            foreach (Grid grid in LVTagSelected.Items)
            {
                TextBlock block = (TextBlock)grid.Children[0];
                objectCreation.Tag += block.Text + ",";
            } 
            objectCreation.Tag.Substring(0, objectCreation.Tag.Length - 3);
            objectCreation.Tag = objectCreation.Tag.Replace(" ", "");
        }

        /// <summary>
        /// Set the user author
        /// </summary>
        /// <returns></returns>
        private async Task SetUser()
        {
            if (App.WebMethods._user != null)
            {
                objectCreation.User = App.WebMethods._user.Name;
                objectCreation.userid = App.WebMethods._user.Msid;
            }
            else
            {
                objectCreation.User = "Anonymous";
                objectCreation.userid = "0";
            }
        }

        private async Task<Quote> CreateQuote()
        {
            Quote q = new Quote();
            q.Content = objectCreation.Content;
            q.userid = objectCreation.userid;
            q.User = objectCreation.User;
            q.Reference = objectCreation.Reference;
            q.Tag = objectCreation.Tag;
            q.Lang = App.WebMethods._applicationLanguage;

            if (_updateMode) q.Id = objectCreation.Id;
            return q;
        }


        private async Task UploadQuote()
        {
            Quote q = null;
            q = await CreateQuote();

            // Upload to the server
            var result = false;

            if (_updateMode)
            {
                
                result = await App.WebMethods.UpdateQuote(q);
            }
            else result = await App.WebMethods.UploadQuote(q);


            if (result)
            {
                PostProcess(q); // begins the post process

                // Everything is fine
                MessageDialog message = new MessageDialog("Your quote was successful uploaded!", "Success");
                message.ShowAsync(); // show success message

                // Hide Appbar button
                AppBarButtonAccept.Icon = new SymbolIcon(Symbol.Accept);
                AppBarButtonAccept.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                // Show book
                Personal_Command_Click(null, null);

                ResetCreatedValues(); // clear local values

                //_step = 1;
                GridUpload.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                GridCreateChoice.Visibility = Windows.UI.Xaml.Visibility.Visible;
                GridCreateChoice.Opacity = 1;
                
                AnimationUpload("off");
                //ShadowMoveFrom(GridCreateChoice, "left");
            }
            else
            {
                // There was a problem
                MessageDialog message = new MessageDialog("Sorry, there was a problem. Take a pause and try in a moment.", "Aww!");
                message.ShowAsync();

                AnimationUpload("off");
                SlideToBottom(GridUpload, GridTags);   // animate to the previous
            }
        }


        /// <summary>
        /// Show personal quotes & wordflow creations when clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Personal_Command_Click(object sender, RoutedEventArgs e)
        {
            if (GridPersonal.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
            {
                SlideToTop(GridCreate, GridPersonal);                                      // Animate the screen
                Personal_Command.Label = "create";                                          // Modify the command label
                
                CommandBarPersonalMode();

                // Hide the Textblock saying there's no quote
                if (App.WebMethods.CollectionPersonal.Count > 0)
                {
                    TBNothingPersonal.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                else
                {
                    RefreshPersonal_Command_Click(null, null);
                }
            }

            else
            {
                Personal_Command.Label = "book";
                SlideToBottom(GridPersonal, GridCreate);
                CommandBarCreateMode();
            }
        }

        private async void CheckPersonalOnline()
        {
            // Test if the user is auth
            if (!App.WebMethods._isAuthenticated)
            {
                // Show the Textblock saying that there's no quote if the user isn't auth > if there's no quote in the Collection
                if (App.WebMethods.CollectionPersonal.Count < 1)
                    TBNothingPersonal.Visibility = Windows.UI.Xaml.Visibility.Visible;
                return;
            }

            // Then go online to check the server with userid parameter
            // Check for new quotes
            GridLoadingPersonal.Visibility = Windows.UI.Xaml.Visibility.Visible; // Loading grid
            await App.WebMethods.GetPersonalOnline();                            // checking

            if (App.WebMethods.CollectionPersonal.Count > 0)                    // result
            {
                TBNothingPersonal.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                App.WebMethods.SavePersonal();
            } GridLoadingPersonal.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void RefreshPersonal_Command_Click(object sender, RoutedEventArgs e)
        {
            CheckPersonalOnline();
        }

        private async void ResetCreatedValues()
        {
            // Clear object values
            objectCreation.Id = "";
            objectCreation.Content = "";
            objectCreation.userid = "";
            objectCreation.User = "";
            objectCreation.Reference = "";
            objectCreation.Type = "";
            objectCreation.Tag = "";
            

            // Clear controls
            BoxCreate.Text = "";
            BlockReference.Text = "Reference";
            BoxReference.Text = "Reference";
            LVTagSelected.Items.Clear(); // vide les tags sélectionnés
        }


        private async void PostProcess(Quote q)
        {
            // UPDATE PERSONAL
            if (!_updateMode)
                // If we're not in update mode, just add the quote to the personal collection
                App.WebMethods.CollectionPersonal.Add(q);
            else
            {
                // If we're in update mode, delete the previous quote and add the updated quote
                foreach (Quote quote in App.WebMethods.CollectionPersonal)
                {
                    if (quote.Id == q.Id)
                    {
                        App.WebMethods.CollectionPersonal.Remove(quote);
                        break;
                    }
                }
                App.WebMethods.CollectionPersonal.Add(q);

                _updateMode = false;
            }

            // UPDATE TAGS
            App.WebMethods.FavoritesTags.Clear();
            foreach (Grid child in StackRecentTags.Children)
            {
                string tag = child.Tag.ToString();
                App.WebMethods.FavoritesTags.Add(tag);
            }

            
            // Check if the list isn't too long
            if (App.WebMethods.FavoritesTags.Count > 10){
                int end = App.WebMethods.FavoritesTags.Count - 1;
                int count = end - 10;
                App.WebMethods.FavoritesTags.RemoveRange(10, count);
            }

            // Save to IO favorites tags
            App.WebMethods.SaveMyFavoritesTags();

            // Save to IO Personal collection
            App.WebMethods.SavePersonal();

            // Notify the change
            App.WebMethods._databaseQuoteChanged = true;
        }


        // ----------------
        // LISTVIEW EVENTS
        // ----------------
        
        /// <summary>
        /// Animate the quote's entrance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemPersonal_Loaded(object sender, RoutedEventArgs e)
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
        /// Show a MenuFlyout when a quote is tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Quote_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        /// <summary>
        /// When the user click on the share ItemMenu Flyout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        
        /// <summary>
        /// Show the (text) share menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            string title = "";
            string description = "";
            string de = "de";

            if (App.WebMethods._applicationLanguage == "FR")
            {
                title = "Citation : ";
                description = "Citation d'Imagine sur Windows Phone";
            }
            else if (App.WebMethods._applicationLanguage == "EN")
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

        /// <summary>
        /// Ask the quote's deletion to the user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog("You really want to delete this quote?", "Confirm");
            dialog.Commands.Add(new UICommand("Delete", new UICommandInvokedHandler(this.CommandInvokedConfirmDelete)));
            dialog.Commands.Add(new UICommand("Nope", new UICommandInvokedHandler(this.CommandInvokedConfirmDelete)));

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            dialog.ShowAsync();

            MenuFlyoutItem item = sender as MenuFlyoutItem;

            if (item != null)
            {
                _quoteToDelete = item.DataContext as Quote;
                
            }
        }

        /// <summary>
        /// Fires when the user confirms the quote's deletion
        /// </summary>
        /// <param name="command"></param>
        private async void CommandInvokedConfirmDelete(IUICommand command)
        {
            // Récupère la commande appuyée et teste sa valeur
            string result = command.Label;
            if (result == "Delete")
            {
                if (_quoteToDelete != null)
                {
                    // Tente de supprimer la citations de la bdd et récupère une valeur en sortie
                    String resultDeleted =  await App.WebMethods.DeleteQuote(_quoteToDelete);

                    if (resultDeleted == "true")
                    {
                        // Si tout s'est bien passée
                        App.WebMethods._databaseQuoteChanged = true;

                        // Delete from personal list
                        if (App.WebMethods.CollectionPersonal.Contains(_quoteToDelete))
                            App.WebMethods.CollectionPersonal.Remove(_quoteToDelete);
                        if (App.WebMethods.CollectionPersonalSorted.Contains(_quoteToDelete))
                            App.WebMethods.CollectionPersonalSorted.Remove(_quoteToDelete);
                    }
                    else
                    {
                        // Il y a eu un soucis
                        if (resultDeleted == "not exists")
                        {
                            // La citation n'existe plus dans la bdd, on la supprime de la liste personnelle
                            if (App.WebMethods.CollectionPersonal.Contains(_quoteToDelete))
                                App.WebMethods.CollectionPersonal.Remove(_quoteToDelete);
                            if (App.WebMethods.CollectionPersonalSorted.Contains(_quoteToDelete))
                                App.WebMethods.CollectionPersonalSorted.Remove(_quoteToDelete);
                        }
                        else
                        {
                            // La requête de suppression dans la bdd n'a pas abouti, on affiche un message d'erreur
                            MessageDialog dialog = new MessageDialog("The quote coudn't be deleted", "Oops");
                            dialog.ShowAsync();
                        }
                    }
                    
                    _quoteToDelete = null;
                }
            }
        }

        /// <summary>
        /// Get the quote and go into edit mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;

            if (item != null)
            {
                Quote q = item.DataContext as Quote;    // get the quote
                _updateMode = true;                     // tells that we're going to update data

                // GET DATA TO EDIT
                objectCreation.Id = q.Id;
                objectCreation.Content = q.Content;
                objectCreation.Reference = q.Reference;
                objectCreation.Tag = q.Tag;
                objectCreation.User = q.User;
                objectCreation.userid = q.userid;
                objectCreation.Type = "quote";

                // Show edit grid => prepare animation
                GridWrite.Visibility = Windows.UI.Xaml.Visibility.Visible;
                GridCreateChoice.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                GridTags.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                SlideToBottom(GridPersonal, GridCreate);

                // Set the editable content
                BoxCreate.Text = objectCreation.Content;
                BoxReference.Text = objectCreation.Reference;
                BlockReference.Text = objectCreation.Reference;


                // SPLIT TAGS
                string[] stringSeparator = new string[] { "," };
                string[] result;
                result = objectCreation.Tag.Split(stringSeparator, StringSplitOptions.None);
                
                LVTagSelected.Items.Clear();    // empty the selected tags
                foreach (string tag in result)
                {
                    if (tag.Length > 2)
                        AddSelectedTag(tag);
                }

                // GO TO EDIT UI
                //_step = 2; // set the step
                CommandBarCreateMode();
                GoToStep2("quote");
            }
        }

        /// <summary>
        /// Show the Commandbar on the "Book" (Personnal mode) screen
        /// </summary>
        private void CommandBarPersonalMode()
        {
            AppBarButtonAccept.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Connect_Command.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            Personal_Command.Label = "create";
            RefreshPersonal_Command.Visibility = Windows.UI.Xaml.Visibility.Visible;
            CommandBarCreate.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        /// <summary>
        /// Show the Commandbar on the Create mode screen
        /// </summary>
        private void CommandBarCreateMode()
        {
            if (_step > 1)
                AppBarButtonAccept.Visibility = Windows.UI.Xaml.Visibility.Visible;

            Personal_Command.Label = "book";
            Personal_Command.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Connect_Command.Visibility = Windows.UI.Xaml.Visibility.Visible;
            RefreshPersonal_Command.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            CommandBarCreate.Visibility = Windows.UI.Xaml.Visibility.Visible;
            CommandBarCreate.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
            
        }

        private void CommandBarAuthentificateMode()
        {
            CommandBarCreate.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }


        private void Chronology_Click(object sender, RoutedEventArgs e)
        {
            if (_personalChronology == "newer first")
            {
                _personalChronology = "older first";
                MenuFlyoutItemOrderBy.Text = "older first";
            }
            else
            {
                _personalChronology = "newer first";
                MenuFlyoutItemOrderBy.Text = "newer first";
            }
            App.WebMethods.OrderPersonalQuotes();
        }

        private async void LangAll_Click(object sender, RoutedEventArgs e)
        {
            if (_personalLang == "All") return;

            _personalLang = "All";
            ListViewPersonal.ItemsSource = App.WebMethods.CollectionPersonal;
            await ShowPersonalQuotesLang(_personalLang);
        }


        private async void LangEN_Click(object sender, RoutedEventArgs e)
        {
            if (_personalLang == "EN") return;

            _personalLang = "EN";
            ListViewPersonal.ItemsSource = App.WebMethods.CollectionPersonalSorted;
            await ShowPersonalQuotesLang(_personalLang);
        }

        private async void LangFR_Click(object sender, RoutedEventArgs e)
        {
            if (_personalLang == "FR") return;

            _personalLang = "FR";
            ListViewPersonal.ItemsSource = App.WebMethods.CollectionPersonalSorted;
            await ShowPersonalQuotesLang(_personalLang);
        }

        /// <summary>
        /// Sort the personal quotes and show wait screen
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        private async Task ShowPersonalQuotesLang(string lang)
        {
            // Show wait screen
            ShowProgressTop("Sorting the list...");

            // Processing
            bool result = await App.WebMethods.SortByLangPersonalQuotes(_personalLang);

            // Hide wait screen
            HideProgressTop();
        }

        private void HButtonSort_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void Quote_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

    }
}
