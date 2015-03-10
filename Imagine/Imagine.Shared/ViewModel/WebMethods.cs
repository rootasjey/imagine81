using Imagine.Common;
using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Popups;

namespace Imagine.ViewModel
{
    public class WebMethods : INotifyPropertyChanged
    {
        // PERSISTENT VARIABLES
        #region variables

        #region collections
        /// <summary>
        /// Collection of favorites' quotes
        /// </summary>
        public ObservableCollection<Quote> CollectionFavorites { get; private set; }

        /// <summary>
        /// Collection de citations personnelles (écites par l'utilisateur)
        /// </summary>
        public ObservableCollection<Quote> CollectionPersonal { get; private set; }

        /// <summary>
        /// Personal Collection of quotes sorted (date, lang)
        /// </summary>
        public ObservableCollection<Quote> CollectionPersonalSorted { get; private set; }

        /// <summary>
        /// Liste de tag préférés
        /// </summary>
        public List<string> FavoritesTags { get; private set; }


        /// <summary>
        /// Quotes' list
        /// </summary>
        public ObservableCollection<Quote> _quotes;

        public ObservableCollection<Quote> ListSingleItem { get; private set; }

        public ObservableCollection<Dailybackground> CollectionBackgrounds { get; private set; }
        #endregion collectons

        #region vars

        /// <summary>
        /// Application language
        /// </summary>
        public string _applicationLanguage = "FR";

        /// <summary>
        /// True if it's the first time the app is launched
        /// </summary>
        public bool _firstApplicationLaunch = true;

        /// <summary>
        /// Tells if the user is authenticated
        /// </summary>
        public bool _isAuthenticated = false;

        /// <summary>
        /// An array representing a user object (contains the user's firstname, lastname, id, language)
        /// </summary>
        public dynamic _userMicrosoft;

        /// <summary>
        /// User object which contains MS id and name
        /// </summary>
        public User _user;

        private MobileServiceUser userMobile;

        /// <summary>
        /// Fond de l'application (si défini)
        /// </summary>
        private string applicationBackground = "";

        public bool _dynamicBackground = true;

        /// <summary>
        /// Si la base de données des citations a été modifiée (ajout de citation)
        /// </summary>
        public bool _databaseQuoteChanged = false;

        /// <summary>
        /// Tells if the app is running by the developper
        /// </summary>
        public bool _ADMIN = true;

        /// <summary>
        /// Mutex permettant de verouiller les accès à l'IO
        /// </summary>
        Mutex _mutex = new Mutex();
        #endregion vars

        #endregion variables

        /// <summary>
        /// Constructeur
        /// </summary>
        public WebMethods()
        {
            this.FavoritesTags = new List<string>();
            this.CollectionFavorites = new ObservableCollection<Quote>();
            this.CollectionPersonal = new ObservableCollection<Quote>();
            this.CollectionPersonalSorted = new ObservableCollection<Quote>();
            this.CollectionBackgrounds = new ObservableCollection<Dailybackground>();
            
            this.ListSingleItem = new ObservableCollection<Quote>();

            _user = new User();
        }


        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Set the variable to false for reload data
        /// </summary>
        public void RefreshPurpose()
        {
            IsDataLoaded = false;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public async Task LoadData()
        {
            await RestoreData(); // Récupère les paramètres utilisateur

            // Récupère la liste des citations
            _quotes = await App.MobileService.GetTable<Quote>()
                                .Where(quote =>quote.Lang == _applicationLanguage)
                                .OrderByDescending(quote => quote.__createdAt)
                                .ToCollectionAsync();

            ListSingleItem.Add(_quotes.FirstOrDefault());

            this.IsDataLoaded = true;
        }


        public async Task RefreshData()
        {
            _quotes.Clear();
            ListSingleItem.Clear();

            _quotes = await App.MobileService.GetTable<Quote>().OrderByDescending(quote => quote.__createdAt).ToCollectionAsync();
            ListSingleItem.Add(_quotes.FirstOrDefault());
        }


        public async Task RefreshFavorites()
        {
            if (CollectionFavorites.Count < 1) return;

            foreach (Quote quote in CollectionFavorites)
            {
                IEnumerable<Quote> query = _quotes.Where(q => q.Id == quote.Id);
                if (query.Count() > 0)
                {
                    Quote qToModify = quote;
                    qToModify = query.First();

                    CollectionFavorites.Remove(quote);
                    CollectionFavorites.Add(qToModify);
                }
            }
        }



        // --------------
        // QUOTE'S METHOD
        // --------------
        #region quote's methods

        /// <summary>
        /// Upload the quote to the quoteFR table on Azure server
        /// </summary>
        /// <param name="quote">quoteFR, an object representing a quote in french language</param>
        /// <returns>True if the data has been successful uploaded</returns>
        public async Task<bool> UploadQuote(Quote quote)
        {
            if (quote == null) return false;

            try
            {
                // Upload on Azure database then return true if it was successfull
                await App.MobileService.GetTable<Quote>().InsertAsync(quote);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateQuote(Quote quote)
        {
            if (quote == null) return false;

            try
            {
                await App.MobileService.GetTable<Quote>().UpdateAsync(quote);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<String> DeleteQuote(Quote quote)
        {
            if (quote == null) return "false";

            try
            {
                await App.MobileService.GetTable<Quote>().DeleteAsync(quote);
                return "true";
            }
            catch (MobileServiceInvalidOperationException e)
            {
                if (e.Message.Contains("does not exist"))
                    return "not exists";
                return "false";
            }
        }

        #endregion quote's methods

        // --------------
        // PERSONAL LOGIC
        // --------------

        // --------------
        // USER LOGIC
        // --------------
        #region userlogic
        /// <summary>
        /// Methods that signin the user into his MS account
        /// Authentication without the Azure protocole, the mobile service won't recognize this auth
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Signin()
        {
            try
            {
                var authClient = new LiveAuthClient();
                LiveLoginResult result = await authClient.LoginAsync(new string[] { "wl.signin", "wl.skydrive" });

                if (result.Status == LiveConnectSessionStatus.Connected)
                {
                    _isAuthenticated = true;
                    var connectClient = new LiveConnectClient(result.Session);
                    var meResult = await connectClient.GetAsync("me");
                    //dynamic meData = meResult.Result;
                    _userMicrosoft = meResult.Result;
                    return true;
                }

                return false;
            }
            catch (LiveAuthException ex)
            {
                // Display an error message.
                MessageDialog message = new MessageDialog(ex.Message, "Ooops!");
                return false;
            }
            catch (LiveConnectException ex)
            {
                // Display an error message.
                MessageDialog message = new MessageDialog(ex.Message, "Ooops!");
                return false;
            }
        }

        /// <summary>
        /// Authentication with the Azure protocole
        /// </summary>
        /// <returns></returns>
        public async Task Authenticate()
        {
            while (userMobile == null)
            {
                string message;
                try
                {
                    userMobile = await App.MobileService
                        .LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount);
                    message =
                        string.Format("You are now logged in - {0}", userMobile.UserId);
                }
                catch (InvalidOperationException)
                {
                    message = "You must log in. Login Required";
                }

                MessageDialog dialog = new MessageDialog(message);
                dialog.ShowAsync();
            }
        }

        public async Task SetUserAttributes()
        {
            if (_userMicrosoft != null)
            {
                _user = new User(null, _userMicrosoft.first_name, _userMicrosoft.id);
                //_user.Name = userMicrosoft.first_name;
                //_user.Msid = userMicrosoft.id;
            }
        }

        /// <summary>
        /// Add a new user to the database
        /// </summary>
        /// <returns></returns>
        public async Task<bool> InsertUser(User user)
        {
            if (user == null) return false;

            try
            {
                await App.MobileService.GetTable<User>().InsertAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Update a existing user in the database
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UpdateUser(User user)
        {
            try
            {
                await App.MobileService.GetTable<User>().UpdateAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Delete a user from the database
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DeleteUser(User user)
        {
            try
            {
                await App.MobileService.GetTable<User>().DeleteAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check a username availability
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Return true if the name specified is available</returns>
        public async Task<bool> CheckUsernameAvailability(string name)
        {
            try
            {
                IList<User> users = await App.MobileService.GetTable<User>().Where(user => user.Name == name).ToListAsync();

                // Si aucun résultat n'est trouvé, le pseudo est disponible
                if (users.Count < 1) return true;

                // Alors si l'ID MS est le même le pseudo est dispo,
                // Sinon, le pseudo n'est pas disponible et on retourne faux;
                if (users.ElementAt(0).Msid == _user.Msid)
                {
                    _user.Id = users.ElementAt(0).Id;
                    return true;
                }
                else return false;
                
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if the user already exists with the MS ID
        /// </summary>
        /// <param name="msid"></param>
        /// <returns></returns>
        public async Task<bool> UserAlreadyexists(string msid)
        {
            try
            {
                IList<User> users = await App.MobileService.GetTable<User>().Where(user => user.Msid == msid).ToListAsync();

                // If the list contains at least 1 result, return false
                if (users.Count > 0) return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Update all quotes/wordflows with new username
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UpdateCreationWithNewUsername(string oldname)
        {
            if (oldname == null || oldname == "") return false;

            IList<Quote> listquotes = await App.MobileService.GetTable<Quote>().Where(quote => quote.User == oldname).ToListAsync();

            if (listquotes.Count > 0)
            {
                foreach (Quote q in listquotes)
                {
                    q.User = _user.Name;
                    App.MobileService.GetTable<Quote>().UpdateAsync(q);
                }
            }

            return true;
        }

        /// <summary>
        /// Get the user's personal quotes with his unique user id
        /// </summary>
        /// <returns></returns>
        public async Task GetPersonalOnline()
        {
            // Get the the user's quotes using user ID
            IEnumerable<Quote> query = _quotes.Where(q => q.userid == _user.Msid)
                                                .OrderByDescending(quote => quote.__createdAt);

            // Clear the personal collection and add what we've found
            CollectionPersonal.Clear();

            foreach (Quote q in query)
            {
                CollectionPersonal.Add(q);
            }
        }

        public async Task OrderPersonalQuotes()
        {
            IEnumerable<Quote> temporaryE = null;
            temporaryE = CollectionPersonalSorted.Reverse();
        }

        public async Task<bool> SortByLangPersonalQuotes(string lang)
        {
            try
            {
                CollectionPersonalSorted.Clear();
                IEnumerable<Quote> temporaryList = null;

                if (lang != "all")
                {
                    temporaryList = CollectionPersonal.Where(quote => quote.Lang == lang);
                }
                else temporaryList = CollectionPersonalSorted;
                

                foreach (Quote quote in temporaryList)
                {
                    CollectionPersonalSorted.Add(quote);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CheckAdmin()
        {
            if (_user.Msid == "fe52b46d3240d773")
                return true;

            return false;
        }
        #endregion userlogic


        #region BackgroundOnline

        /// <summary>
        /// Get all Dailybackground items from the database and return true if the method was successful
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GetAllDailybackground()
        {
            try
            {
                CollectionBackgrounds.Clear();
                CollectionBackgrounds = await App.MobileService.GetTable<Dailybackground>().ToCollectionAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> InsertDailybackground(Dailybackground bg)
        {
            try
            {
                await App.MobileService.GetTable<Dailybackground>().InsertAsync(bg);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateDailybackground(Dailybackground bg)
        {
            try
            {
                await App.MobileService.GetTable<Dailybackground>().UpdateAsync(bg);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteDailybackground(Dailybackground bg)
        {
            try
            {
                await App.MobileService.GetTable<Dailybackground>().DeleteAsync(bg);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get the last background for the application
        /// </summary>
        /// <returns></returns>
        public async Task<Dailybackground> GetTodayBackground()
        {
            try
            {
                // (parameter) DateTime day
                //IEnumerable<Dailybackground> bgs = await App.MobileService.GetTable<Dailybackground>()
                //                                            .Where(bg => bg.day == day)
                //                                            .ToEnumerableAsync();

                IEnumerable<Dailybackground> bgs = await App.MobileService.GetTable<Dailybackground>()
                                                            .ToEnumerableAsync();
                if (bgs.Count<Dailybackground>() > 0)
                {
                    return bgs.FirstOrDefault();
                }
                return null;
                
            }
            catch
            {
                return null;
            }
        }
        #endregion BackgroundOnline

        // --------------------------
        // INPUT/OUTPUT (SAVING DATA)
        // --------------------------
        #region input/output and localstorage
        /// <summary>
        /// Enregistre toutes les informations de l'application
        /// </summary>
        /// <returns></returns>
        public async Task SaveData()
        {
            await SaveSettings();
            await SaveFavorites();
            await SaveMyFavoritesTags();
            await SavePersonal();
        }


        /// <summary>
        /// Restaure toutes les informations de l'application
        /// </summary>
        /// <returns></returns>
        public async Task RestoreData()
        {
            await RestoreSettings();
            await RestoreFavorites();
            await RestoreMyFavoritesTags();
            await RestorePersonal();
        }

        /// <summary>
        /// Sauvegarde les paramètres de l'utilisateur
        /// </summary>
        public async Task SaveSettings()
        {
            _mutex.WaitOne();
            try
            {
                var applicationData = Windows.Storage.ApplicationData.Current;
                var localSettings = applicationData.LocalSettings;
                localSettings.Values["applicationLanguage"] = _applicationLanguage;
                localSettings.Values["isAuthenticated"] = _isAuthenticated;
                localSettings.Values["firstApplicationLaunch"] = _firstApplicationLaunch;
                localSettings.Values["_dynamicBackground"] = _dynamicBackground;
                SaveUser();
            }
            catch
            {
            }
            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// Charge les paramètres de l'utilisateur
        /// </summary>
        public async Task RestoreSettings()
        {
            _mutex.WaitOne();
            try
            {
                var applicationData = Windows.Storage.ApplicationData.Current;
                var localSettings = applicationData.LocalSettings;

                if (localSettings.Values.Keys.Contains("applicationLanguage"))
                {
                    var lang = (string)localSettings.Values["applicationLanguage"];
                    if (lang != null) _applicationLanguage = lang;
                }

                if (localSettings.Values.Keys.Contains("isAuthenticated"))
                {
                    var auth = false;
                    auth = (bool)localSettings.Values["isAuthenticated"];
                    if (auth != false) _isAuthenticated = auth;
                }

                if (localSettings.Values.Keys.Contains("firstApplicationLaunch"))
                {
                    var launched = (bool)localSettings.Values["firstApplicationLaunch"];
                    _firstApplicationLaunch = launched;
                }
                
                if (localSettings.Values.Keys.Contains("_dynamicBackground"))
                {
                    var dynamicBackground = (bool)localSettings.Values["_dynamicBackground"];
                    if (dynamicBackground != null) _dynamicBackground = dynamicBackground;
                }
                

                RestoreUser();
            }
            catch
            {
            }
            _mutex.ReleaseMutex();
        }


        public async Task SaveUser()
        {
            _mutex.WaitOne();
            try
            {
                await MyDataSerializer<User>.SaveObjectsAsync(_user, "_user.xml");
            }
            catch
            {
            }
            _mutex.ReleaseMutex();
        }

        public async Task RestoreUser()
        {
            _mutex.WaitOne();
            try
            {
                User u = await MyDataSerializer<User>.RestoreObjectsAsync("_user.xml");
                if (u != null) _user = u;
            }
            catch
            {
            }
            _mutex.ReleaseMutex();
        }

        public async Task SaveMyObject(object obj)
        {
            _mutex.WaitOne();
            try
            {
                var applicationData = Windows.Storage.ApplicationData.Current;
                var localSettings = applicationData.LocalSettings;
                localSettings.Values[obj.ToString()] = obj.ToString();
            }
            catch
            {
            }
            _mutex.ReleaseMutex();
        }


        /// <summary>
        /// Enregistre la liste de citations favorites dans l'IO
        /// </summary>
        public async Task SaveFavorites()
        {
            _mutex.WaitOne();
            try
            {
                await MyDataSerializer<ObservableCollection<Quote>>.SaveObjectsAsync(CollectionFavorites, "CollectionFavorites.xml");
            }
            catch
            {
            }
            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// Enregistre la liste de citations favorites dans l'IO
        /// </summary>
        public async Task RestoreFavorites()
        {
            _mutex.WaitOne();
            try
            {
                CollectionFavorites = await MyDataSerializer<ObservableCollection<Quote>>.RestoreObjectsAsync("CollectionFavorites.xml");
            }
            catch
            {
            }
            _mutex.ReleaseMutex();
        }


        /// <summary>
        /// Enregistre la liste de citations personnelles dans l'IO
        /// </summary>
        public async Task SavePersonal()
        {
            _mutex.WaitOne();
            try
            {
                await MyDataSerializer<ObservableCollection<Quote>>.SaveObjectsAsync(CollectionPersonal, "CollectionPersonal.xml");
            }
            catch
            {
            }
            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// Enregistre la liste de citations personelles dans l'IO
        /// </summary>
        public async Task RestorePersonal()
        {
            _mutex.WaitOne();
            try
            {
                CollectionPersonal = await MyDataSerializer<ObservableCollection<Quote>>.RestoreObjectsAsync("CollectionPersonal.xml");
            }
            catch
            {
            }
            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// Sauvegarde la liste principale de citations
        /// dans le cas où l'utilisateur n'aurait pas de connexion internet
        /// </summary>
        public async Task SaveMainList()
        {
            _mutex.WaitOne();
            try
            {
                await MyDataSerializer<ObservableCollection<Quote>>.SaveObjectsAsync(_quotes, "_quotes.xml");
            }
            catch
            {
            }
            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// Enregistre la liste des tags favoris
        /// </summary>
        public async Task RestoreMainList()
        {
            _mutex.WaitOne();
            try
            {
                _quotes = await MyDataSerializer<ObservableCollection<Quote>>.RestoreObjectsAsync("_quotes.xml");
            }
            catch
            {
            }
            _mutex.ReleaseMutex();
        }

        public async Task SaveMyFavoritesTags()
        {
            _mutex.WaitOne();
            try
            {
                await MyDataSerializer<List<string>>.SaveObjectsAsync(FavoritesTags, "FavoritesTags.xml");
            }
            catch
            {
            }
            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// Restore la liste des tags favoris
        /// </summary>
        public async Task RestoreMyFavoritesTags()
        {
            _mutex.WaitOne();
            try
            {
                FavoritesTags = await MyDataSerializer<List<string>>.RestoreObjectsAsync("FavoritesTags.xml");
            }
            catch
            {
            }
            _mutex.ReleaseMutex();
        }

        #endregion io
        

        /// <summary>
        /// Met à jour la tuile principale de l'application
        /// </summary>
        public void UpdateMainTile()
        {
            //if (_ListQ.Count > 0)
            //{
            //    string content = _ListQ.FirstOrDefault().Content;
            //    string wideContent = _ListQ.FirstOrDefault().Content;
            //    string user = _ListQ.FirstOrDefault().User;


            //    // Get application's main tile - application tile always first item in the ActiveTiles collection
            //    // wheter it is pinned or not
            //    var mainTile = ShellTile.ActiveTiles.FirstOrDefault();
            //    if (mainTile != null)
            //    {
            //        FlipTileData tileData = new FlipTileData();

            //        tileData.BackContent = content;
            //        tileData.WideBackContent = wideContent;
            //        tileData.BackTitle = user;

            //        mainTile.Update(tileData);
            //    }
            //}
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
