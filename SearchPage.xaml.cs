using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Marketplace;
using Microsoft.Phone.Tasks;

using Newtonsoft.Json.Linq;

namespace FlashQuiz
{
    public partial class SearchPage : PhoneApplicationPage
    {
        public LicenseInformation licenseInfo = new LicenseInformation();
        public MarketplaceDetailTask detailTask = new MarketplaceDetailTask();

        private DateTime _unixEpoch = new DateTime(1970, 1, 1);

        // Constructor
        public SearchPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // Handle selection changed on ListBox
        private void FindSetsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected index is -1 (no selection) do nothing
            if (FindSetsListBox.SelectedIndex == -1)
                return;

            SetViewModel svm = (SetViewModel)FindSetsListBox.SelectedItem;
            int id = svm.ID;

            ViewSet(id);


            // Reset selected index to -1 (no selection)
            FindSetsListBox.SelectedIndex = -1;
        }

        // Handle selection changed on ListBox
        private void MySetsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected index is -1 (no selection) do nothing
            if (MySetsListBox.SelectedIndex == -1)
                return;

            SetViewModel svm = (SetViewModel)MySetsListBox.SelectedItem;
            int id = svm.ID;

            ViewSet(id);


            // Reset selected index to -1 (no selection)
            MySetsListBox.SelectedIndex = -1;
        }

        private void ViewSet(int id)
        {

            //// This is where we check to see if they are in a trial
            //// First thing we want to do is increment the number of times we have opened a deck...
            //bool trialExpired = false;
            //if (licenseInfo.IsTrial())
            //{
            //    int numberOfTrialRuns = IsolatedStorageHelper.GetObject<int>("NumberOfTrialRuns");
            //    numberOfTrialRuns++;
            //    if (numberOfTrialRuns > 5)
            //    {
            //        trialExpired = true;
            //        MessageBoxResult result = MessageBox.Show("The trial version of Flash Quiz is limited to 5 views of one or more flashcard sets. Would you like to go to the Marketplace to purchase the full version?", "Trial Expired", MessageBoxButton.OKCancel);
            //        if (result == MessageBoxResult.OK)
            //        {
            //            detailTask.Show();
            //        }
            //    }
            //    else
            //    {
            //        IsolatedStorageHelper.SaveObject<int>("NumberOfTrialRuns", numberOfTrialRuns);
            //    }
            //}

            //// Navigate to the new page
            //if (!trialExpired)
            //{
            //    NavigationService.Navigate(new Uri("/DetailsPage.xaml?id=" + id.ToString(), UriKind.Relative));
            //}

            NavigationService.Navigate(new Uri("/DetailsPage.xaml?id=" + id.ToString(), UriKind.Relative));

        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            Search(1);
        }

        private void Search(int page)
        {
            //Uri serviceUri = new Uri(string.Format("http://quizlet.com/api/1.0/sets?dev_key=crom7kn7xd4osswk&q={0}&page={1}&per_page=30&time_format=fuzzy_date", filterTextBox.Text, page.ToString()));
            // https://api.quizlet.com/2.0/search/sets?client_id=crom7kn7xd4osswk&q=physics
            Uri serviceUri = new Uri(string.Format("https://api.quizlet.com/2.0/search/sets?client_id=crom7kn7xd4osswk&q={0}&page={1}&per_page=30&time_format=fuzzy_date", filterTextBox.Text, page.ToString()));

            WebClient client = new WebClient();
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(client_OpenReadCompleted);
            client.OpenReadAsync(serviceUri);

            App.ViewModel.SearchSets.Clear();
            searchResultsTextBlock.Text = "Searching...";
            previousPageLinkButton.Visibility = System.Windows.Visibility.Collapsed;
            nextPageLinkButton.Visibility = System.Windows.Visibility.Collapsed;
        }

        void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
                try
                {
                    Stream responseStream = e.Result;

                    // Continue working with responseStream here...
                    StreamReader reader = new StreamReader(responseStream);
                    string json = reader.ReadToEnd();

                    JObject response = JObject.Parse(json);
                    string response_type = (string)response["response_type"];

                    App.ViewModel.CurrentPage = (int)response["page"];
                    App.ViewModel.TotalPages = (int)response["total_pages"];

                    if (App.ViewModel.CurrentPage > 1)
                        previousPageLinkButton.Visibility = System.Windows.Visibility.Visible;
                    else
                        previousPageLinkButton.Visibility = System.Windows.Visibility.Collapsed;

                    if (App.ViewModel.CurrentPage < App.ViewModel.TotalPages)
                        nextPageLinkButton.Visibility = System.Windows.Visibility.Visible;
                    else
                        nextPageLinkButton.Visibility = System.Windows.Visibility.Collapsed;

                    JArray sets = (JArray)response["sets"];
                    foreach (JObject set in sets)
                    {
                        int id = (int)set["id"];
                        string title = (string)set["title"];
                        string creator = (string)set["created_by"];
                        int secondsSinceEpoch = (int)set["created_date"];
                        string created = _unixEpoch.AddSeconds(secondsSinceEpoch).ToShortDateString();
                        int termCount = (int)set["term_count"];

                        SetViewModel svm = new SetViewModel() { ID = id, Title = title, Creator = creator, Created = created, TermCount = termCount };
                        App.ViewModel.SearchSets.Add(svm);

                    }

                    searchResultsTextBlock.Text = "Showing page " + App.ViewModel.CurrentPage.ToString() + " of " + App.ViewModel.TotalPages.ToString();

                }
                catch (WebException wex)
                {
                    try
                    {
                        if (wex.Response is HttpWebResponse)
                        {
                            StreamReader reader = new StreamReader(wex.Response.GetResponseStream());
                            string json = reader.ReadToEnd();
                            JObject response = JObject.Parse(json);
                            string short_text = (string)response["error_title"];
                            string long_text = (string)response["error_description"];
                            searchResultsTextBlock.Text = e.Error.Message;
                        }
                    }
                    catch(Exception ex)
                    {
                        searchResultsTextBlock.Text = "Search failed. Please try again later.";
                    }
                }
                catch (Exception ex)
                {
                    searchResultsTextBlock.Text = "Search failed. Please try again later.";
                }

        }

        private void previousPageLinkButton_Click(object sender, RoutedEventArgs e)
        {
            int page = App.ViewModel.CurrentPage - 1;
            Search(page);
        }

        private void nextPageLinkButton_Click(object sender, RoutedEventArgs e)
        {
            int page = App.ViewModel.CurrentPage + 1;
            Search(page);
        }

        private ListBoxItem _searchResultsContextListBoxItem = null;
        private void SearchResultsListItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point tmpPoint = e.GetPosition(null);
            _searchResultsContextListBoxItem = null;
            List<UIElement> oControls = (List<UIElement>)VisualTreeHelper.FindElementsInHostCoordinates(tmpPoint, this);
            foreach (UIElement ctrl in oControls)
            {
                if (ctrl is ListBoxItem)
                {
                    _searchResultsContextListBoxItem = (ListBoxItem)ctrl;
                    break;
                }
            }
        }

        private void SearchResultsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SetViewModel svm = (SetViewModel)_searchResultsContextListBoxItem.Content;
            App.ViewModel.MySets.Add(svm);
        }

        private void SearchResultsContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = (ContextMenu)sender;
            SetViewModel svm = (SetViewModel)_searchResultsContextListBoxItem.Content;
            MenuItem addToMySetsMenuItem = (MenuItem)cm.Items[0];
            addToMySetsMenuItem.IsEnabled = !(App.ViewModel.MySets.Contains(svm));
        }

        private void MySetsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SetViewModel svm = (SetViewModel)_mySetsContextListBoxItem.Content;
            App.ViewModel.MySets.Remove(svm);
        }

        private ListBoxItem _mySetsContextListBoxItem = null;
        private void MySetsListItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point tmpPoint = e.GetPosition(null);
            _mySetsContextListBoxItem = null;
            List<UIElement> oControls = (List<UIElement>)VisualTreeHelper.FindElementsInHostCoordinates(tmpPoint, this);
            foreach (UIElement ctrl in oControls)
            {
                if (ctrl is ListBoxItem)
                {
                    _mySetsContextListBoxItem = (ListBoxItem)ctrl;
                    break;
                }
            }
        }

    }

}