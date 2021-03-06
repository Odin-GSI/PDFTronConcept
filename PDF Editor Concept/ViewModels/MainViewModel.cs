﻿using Microsoft.Win32;
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
        string _currentPDF;

        private bool _editorEnabled = true;
        public MainViewModel()
        {
            
        }

        public string currentPDF
        {
            get { return _currentPDF; }
            set
            {
                _currentPDF = value;
                RaisePropertyChanged("currentPDF");
            }
        }

        public bool EditorEnabled
        {
            get { return _editorEnabled; }
            set
            {
                _editorEnabled = value;
                RaisePropertyChanged("EditorEnabled");
            }
        }

        //Generic Command
        RelayCommand _openCommand;
        RelayCommand _openURLCommand;

        //Command to Open and Load a File
        public ICommand OpenFileCommand
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

        public ICommand OpenURLCommand
        {
            get
            {
                if (_openURLCommand == null)
                {
                    _openURLCommand = new RelayCommand(param => this.OpenURL());
                }

                return _openURLCommand;
            }
        }

        private void OpenURL()
        {
            string uri;
            uri = @"http://localhost/PDFWebApi/somefile";
            //uri = @"http://localhost:8627/somefile";
            currentPDF = uri;
        }

        //Load PDF logic
        private void OpenFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;
            fileDialog.Filter = "PDF (*.pdf)|*.pdf|All files (*.*)|*.*";
            fileDialog.DefaultExt = ".pdf";
            //this.EditorEnabled = false;
            if (fileDialog.ShowDialog() == true)
            {
                currentPDF = fileDialog.FileName;
            }
            //this.EditorEnabled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
