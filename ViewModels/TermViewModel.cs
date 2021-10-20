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

namespace FlashQuiz
{
    public class TermViewModel : INotifyPropertyChanged
    {
        private string _term;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Term
        {
            get
            {
                return _term;
            }
            set
            {
                if (value != _term)
                {
                    _term = value;
                    NotifyPropertyChanged("Term");
                }
            }
        }

        private string _definition;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Definition
        {
            get
            {
                return _definition;
            }
            set
            {
                if (value != _definition)
                {
                    _definition = value;
                    NotifyPropertyChanged("Definition");
                }
            }
        }

        private string _photo;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Photo
        {
            get
            {
                return _photo;
            }
            set
            {
                if (value != _photo)
                {
                    _photo = value;
                    NotifyPropertyChanged("Photo");
                }
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