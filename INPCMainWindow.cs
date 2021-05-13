using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace CSVtoDataBase
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool isDatabaseUpdated = false;

        public bool IsDatabaseUpdated
        {
            get => isDatabaseUpdated;

            set
            {
                if (isDatabaseUpdated != value)
                {
                    isDatabaseUpdated = value;

                    OnPropertyChanged();
                }
            }
        }

        private bool isReportsRead = false;

        public bool IsReportsRead
        {
            get => isReportsRead;

            set
            {
                if (isReportsRead != value)
                {
                    isReportsRead = value;

                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

    }

}