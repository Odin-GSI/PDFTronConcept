using Microsoft.Win32;
using pdftron.PDF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PDF_Editor_Concept
{
    public class MainViewModel : INotifyPropertyChanged
    {
        string _current;

        public MainViewModel()
        {
            
        }

        public string currentPDF
        {
            get { return _current; }
            set
            {
                _current = value;
                RaisePropertyChanged("currentPDF");
            }
        }

        //Generic Command
        RelayCommand _openCommand;

        //Command to Open and Load a File
        public ICommand OpenCommand
        {
            get
            {
                if (_openCommand == null)
                {
                    _openCommand = new RelayCommand(param => this.OpenFile());
                }

                return _openCommand;
            }
        }

        //Load PDF logic
        private void OpenFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;
            fileDialog.Filter = "PDF (*.pdf)|*.pdf|All files (*.*)|*.*";
            fileDialog.DefaultExt = ".pdf";

            if (fileDialog.ShowDialog() == true)
            {
                currentPDF = fileDialog.FileName;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
