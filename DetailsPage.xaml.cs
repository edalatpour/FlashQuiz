using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Newtonsoft.Json.Linq;

namespace FlashQuiz
{
    public partial class DetailsPage : PhoneApplicationPage
    {

        private bool _showBothSides = false;
        private bool _showBackSide = false;

        // Constructor
        public DetailsPage()
        {
            InitializeComponent();
            Color  accentColor = (Color)Application.Current.Resources["PhoneAccentColor"];
            ApplicationBar.BackgroundColor = accentColor;
            ApplicationBar.ForegroundColor = Colors.White;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            SavePageState("CurrentTerm", MyPivot.SelectedIndex);
            SavePageState("ShowBothSides", _showBothSides);
            SavePageState("ShowBackSide", _showBackSide);

            //OPTIONAL: Save other fields here   
            base.OnNavigatedFrom(e);
        }

        private void SavePageState(string key, object value)
        {
            //Remove focused element from previous time if any   
            if (State.ContainsKey(key))
            {
                State.Remove(key);
            }
            State.Add(key, value);
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            string idParam = "";
            if (NavigationContext.QueryString.TryGetValue("id", out idParam))
            {
                int id = int.Parse(idParam);

                //Uri serviceUri = new Uri(string.Format("http://quizlet.com/api/1.0/sets?dev_key=crom7kn7xd4osswk&q=ids:{0}&time_format=fuzzy_date&extended=on", id.ToString()));
                Uri serviceUri = new Uri(string.Format("https://api.quizlet.com/2.0/sets/{0}?client_id=crom7kn7xd4osswk&time_format=fuzzy_date&extended=on", id.ToString()));
                WebClient client = new WebClient();
                client.OpenReadCompleted += new OpenReadCompletedEventHandler(client_OpenReadCompleted);
                client.OpenReadAsync(serviceUri);

                currentCardTextBlock.Text = "Loading...";
            }

            base.OnNavigatedTo(e);
        }

        void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {

            if (e.Error == null)
            {

                Stream responseStream = e.Result;

                // Continue working with responseStream here...
                StreamReader reader = new StreamReader(responseStream);
                string json = reader.ReadToEnd();

                JObject response = JObject.Parse(json);
                string response_type = (string)response["response_type"];

                if (response_type == "error")
                {
                    string long_text = (string)response["long_text"];
                    currentCardTextBlock.Text = long_text;
                }
                else
                {
                    int id = (int)response["id"];
                    string title = (string)response["title"];
                    string creator = (string)response["created_by"];
                    //DateTime created = (DateTime)response["created_date"];
                    int termCount = (int)response["term_count"];

                    SetViewModel svm = new SetViewModel() { ID = id, Title = title, Creator = creator, TermCount = termCount };

                    JArray terms = (JArray)response["terms"];
                    for (int i = 0; i < terms.Count; i++)
                    {
                        string termName = (string)terms[i]["term"];
                        string definition = (string)terms[i]["definition"];
                        string url = "";
                        JToken imageToken = terms[i]["image"];
                        if (imageToken.HasValues)
                        {
                            JObject photo = (JObject)(imageToken);
                            if (photo != null)
                            {
                                url = (string)photo["url"];
                                // 1/9/2012 - It looks like this is no longer needed.
                                ////string pattern = "src=\"([^\"]+)";
                                //string pattern = "(?<=<img[^<]+?src=\")[^\"]+";  
                                //Regex r = new Regex(pattern);
                                //Match m = r.Match(url);
                                //if (m.Success)
                                //{
                                //    url = m.ToString();
                                //    //photo = photo.Replace("_m.jpg", "_t.jpg");
                                //}
                            }
                        }

                        TermViewModel tvm = new TermViewModel() { Term = termName, Definition = definition, Photo = url };
                        svm.Terms.Add(tvm);
                    }

                    App.ViewModel.CurrentSet = svm;

                    this.DataContext = App.ViewModel.CurrentSet;

                    MyPivot.SelectedIndex = (int)LoadPageState("CurrentTerm", MyPivot.SelectedIndex);
                    _showBothSides = (bool)LoadPageState("ShowBothSides", _showBothSides);
                    _showBackSide = (bool)LoadPageState("ShowBackSide", _showBackSide);

                    DisplayTerm();

                }

            }
            else
            {
                currentCardTextBlock.Text = "Unable to connect to Quizlet.com";
                (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
                (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = false;
                (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = false;
                (ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = false;
            }

        }

        private object LoadPageState(string key, object value)
        {
            if (State.ContainsKey(key))
            {
                value = State[key];
            }
            return (value);
        }

        private void PreviousTerm()
        {
            if (MyPivot.SelectedIndex > 0)
            {
                _showBackSide = false;
                MyPivot.SelectedIndex--;
            }
        }

        private void NextTerm()
        {
            if (MyPivot.SelectedIndex < App.ViewModel.CurrentSet.Terms.Count - 1)
            {
                _showBackSide = false;
                MyPivot.SelectedIndex++;
            }
        }

        private void DisplayTerm()
        {
            ApplicationBarIconButton previousButton = (ApplicationBar.Buttons[0] as ApplicationBarIconButton);
            ApplicationBarIconButton bothSidesButton = (ApplicationBar.Buttons[1] as ApplicationBarIconButton);
            ApplicationBarIconButton flipButton = (ApplicationBar.Buttons[2] as ApplicationBarIconButton);
            ApplicationBarIconButton nextButton = (ApplicationBar.Buttons[3] as ApplicationBarIconButton);

            //ContentPanel.DataContext = App.ViewModel.CurrentSet.Terms[_termIndex];

            if (App.ViewModel.CurrentSet == null)
            {
                currentCardTextBlock.Text = "";
            }
            else
            {
                currentCardTextBlock.Text = (MyPivot.SelectedIndex + 1).ToString() + " of " + App.ViewModel.CurrentSet.TermCount.ToString();

                if (MyPivot.SelectedIndex <= 0)
                {
                    previousButton.IsEnabled = false;
                }
                else
                {
                    previousButton.IsEnabled = true;
                }

                if (MyPivot.SelectedIndex >= App.ViewModel.CurrentSet.TermCount - 1)
                {
                    nextButton.IsEnabled = false;
                }
                else
                {
                    nextButton.IsEnabled = true;
                }

                if (_showBothSides)
                {
                    bothSidesButton.IconUri = new Uri("/Images/appbar.minus.rest.png", UriKind.Relative);
                    bothSidesButton.Text = "one side";
                    flipButton.IsEnabled = false;
                    MyPivot.ItemTemplate = PivotItemBothDataTemplate;
                }
                else
                {
                    bothSidesButton.IconUri = new Uri("/Images/appbar.plus.rest.png", UriKind.Relative);
                    bothSidesButton.Text = "both sides";
                    flipButton.IsEnabled = true;
                    if (_showBackSide)
                    {
                        MyPivot.ItemTemplate = PivotItemBackDataTemplate;
                    }
                    else
                    {
                        MyPivot.ItemTemplate = PivotItemFrontDataTemplate;
                    }
                }

            }
        }

        private void FlipCard()
        {
            _showBackSide = !_showBackSide;
            SwivelTransition swivelTransition = new SwivelTransition { Mode = SwivelTransitionMode.ForwardOut };
            ITransition transition = swivelTransition.GetTransition(MyPivot);
            transition.Completed += delegate
            {
                transition.Stop();
                if (_showBackSide)
                {
                    MyPivot.ItemTemplate = PivotItemBackDataTemplate;
                }
                else
                {
                    MyPivot.ItemTemplate = PivotItemFrontDataTemplate;
                }
                SwivelTransition swivelTransition2 = new SwivelTransition { Mode = SwivelTransitionMode.ForwardIn };
                ITransition transition2 = swivelTransition2.GetTransition(MyPivot);
                transition2.Completed += delegate { transition2.Stop(); };
                transition2.Begin();
            };
            transition.Begin();
        }

        //void flip_Completed(object sender, EventArgs e)
        //{
        //    if (_showBackSide)
        //    {
        //        MyPivot.ItemTemplate = PivotItemBackDataTemplate;
        //    }
        //    else
        //    {
        //        MyPivot.ItemTemplate = PivotItemFrontDataTemplate;
        //    }
        //    SwivelTransition swivelTransition = new SwivelTransition { Mode = SwivelTransitionMode.ForwardIn };
        //    ITransition transition = swivelTransition.GetTransition(MyPivot);
        //    transition.Completed += delegate { transition.Stop(); };
        //    transition.Begin();
        //}

        private void showBothSidesButton_Click(object sender, EventArgs e)
        {
            _showBothSides = !_showBothSides;         
            DisplayTerm();
        }

        private void flipButton_Click(object sender, EventArgs e)
        {
            if (!_showBothSides)
            {
                FlipCard();
            }
        }

        private void previousButton_Click(object sender, EventArgs e)
        {
            PreviousTerm();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            NextTerm();
        }

         private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _showBackSide = false;
            DisplayTerm();
        }

        private void Pivot_Tap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            if (!_showBothSides)
            {
                FlipCard();
            }
        }

    }

}