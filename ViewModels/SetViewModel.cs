using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace FlashQuiz
{
    public class SetViewModel : INotifyPropertyChanged
    {
        public SetViewModel()
        {
            this.Terms = new ObservableCollection<TermViewModel>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<TermViewModel> Terms { get; private set; }

        private int _id;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    NotifyPropertyChanged("ID");
                }
            }
        }

        private string _title;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        private string _creator;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Creator
        {
            get
            {
                return _creator;
            }
            set
            {
                if (value != _creator)
                {
                    _creator = value;
                    NotifyPropertyChanged("Creator");
                }
            }
        }

        private string _created;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Created
        {
            get
            {
                return _created;
            }
            set
            {
                if (value != _created)
                {
                    _created = value;
                    NotifyPropertyChanged("Created");
                }
            }
        }

        private int _termCount;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public int TermCount
        {
            get
            {
                return _termCount;
            }
            set
            {
                if (value != _termCount)
                {
                    _termCount = value;
                    NotifyPropertyChanged("TermCount");
                }
            }
        }

        public string Credits
        {
            get
            {
                return "By " + _creator + " on " + _created + " (" + _termCount + " terms)";
            }
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