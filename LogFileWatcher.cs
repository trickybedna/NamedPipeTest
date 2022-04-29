using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NamedPipeTest
{
    //Class to watch file

    public class LogFileWatcher : INotifyPropertyChanged
    {
        private Control bindingControl;

        public Control BindingControl
        {
            get { return bindingControl; }
        }

        private string path;

        public string Path
        {
            get { return path; }
        }

        private string fileContent;

        public string FileContent
        {
            get { return fileContent; }
            set
            {
                fileContent = value;
                PropertyChanged(this, new PropertyChangedEventArgs("FileContent"));
            }
        }

        private FileSystemWatcher watcher;

        public LogFileWatcher(string path, Control bindingControl)
        {
            this.path = path;
            this.bindingControl = bindingControl;
            fileContent = File.ReadAllText(Directory.GetFiles(this.path)[0]);
            this.PropertyChanged = new PropertyChangedEventHandler(OnContentChanged);
            this.setValue = new SetValueDlg(SetContentValue);

            watcher = new FileSystemWatcher();
            watcher.Changed += new FileSystemEventHandler(watcher_Changed);
            watcher.Path = path;
            watcher.EnableRaisingEvents = true;
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            bindingControl.Invoke(setValue, e.FullPath);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnContentChanged(object sender, PropertyChangedEventArgs e)
        {
            
        }

        #endregion

        public delegate void SetValueDlg(string fileName);

        private SetValueDlg setValue;

        private void SetContentValue(string fileName)
        {
            this.FileContent = File.ReadAllText(fileName);
        }
    }
}
