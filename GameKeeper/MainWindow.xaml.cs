using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GameKeeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class ExportImportButton : System.Windows.Controls.Primitives.ButtonBase
    {
        public ExportImportButton() : base() { }
        public Boolean IsExport
        {
            get { return (Boolean)this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
          "IsExport", typeof(Boolean), typeof(ExportImportButton), new PropertyMetadata(false));

        public string GameName
        {
            get { return (string)this.GetValue(GameNameProperty); }
            set { this.SetValue(GameNameProperty, value); }
        }
        public static readonly DependencyProperty GameNameProperty = DependencyProperty.Register(
          "GameName", typeof(string), typeof(ExportImportButton), new PropertyMetadata(""));

        public string GameLibrary
        {
            get { return (string)this.GetValue(GameLibraryProperty); }
            set { this.SetValue(GameLibraryProperty, value); }
        }
        public static readonly DependencyProperty GameLibraryProperty = DependencyProperty.Register(
          "GameLibrary", typeof(string), typeof(ExportImportButton), new PropertyMetadata(""));
    }

    public partial class MainWindow : Window
    {
        public static MainWindow _main;

        private Dictionary<string, ILibrary> _libraries = new Dictionary<string, ILibrary>();
        private List<System.IO.FileSystemWatcher> _watchers = new List<System.IO.FileSystemWatcher>();

        private string _sendToGKImage = "Import_16x.png";
        private string _returnFromGKImage = "Export_16x.png";

        private string _GKLibraryPath;

        private System.Collections.ObjectModel.ObservableCollection<game> _gameView = new System.Collections.ObjectModel.ObservableCollection<game>();

        private class game
        {
            public string library { get; set; }
            public string name { get; set; }
            public string location { get; set; }
            public string path { get; set; }
            public string native_path { get; set; }
            public string image { get; set; }
            public bool exporting { get; set; }

            public game( string library, string name, string location, string path, string native_path, string image, bool exporting)
            {
                this.library = library;
                this.name = name;
                this.location = location;
                this.path = path;
                this.native_path = native_path;
                this.image = image;
                this.exporting = exporting;
            }

        }

        private string GetLibraryPathFromReg()
        {
            try
            {
                var hkcu = Microsoft.Win32.Registry.CurrentUser;
                return hkcu.OpenSubKey("Software\\AJN\\GameKeeper", false).GetValue("Path").ToString();
            }
            catch
            {
                return null;
            }
        }

        private void SetLibraryPathInReg( string path )
        {
            var hkcu = Microsoft.Win32.Registry.CurrentUser;
            hkcu.CreateSubKey("Software\\AJN\\GameKeeper").SetValue("Path", path);
        }

        private void BuildGameLCV()
        {
            //var games = new System.Collections.ObjectModel.ObservableCollection<game>();
            _gameView.Clear(); // TODO: switch to a data type that lets me update selectively.
            foreach ( var p in _libraries )
            {
                foreach (var g in p.Value.GetGameDirectories())
                {
                    var bp = new StackPanel();
                    bp.Orientation = Orientation.Horizontal;
                    bp.Children.Add(new Button());

                    _gameView.Add(new game(
                        p.Key,
                        g,
                        "Steam",
                        System.IO.Path.Combine(p.Value.GetHomePath(), g),
                        System.IO.Path.Combine(p.Value.GetHomePath(), g),
                        _sendToGKImage,
                        true
                    ));
                }
                foreach (var j in p.Value.GetReparsePoints())
                {
                    var bp = new StackPanel();
                    bp.Orientation = Orientation.Horizontal;
                    bp.Children.Add(new Button());

                    Junctions.GetJunctionTarget( System.IO.Path.Combine(p.Value.GetHomePath(), j), out var target );
                   _gameView.Add(new game(
                        p.Key, 
                        j,
                        "GameKeeper",
                        target,
                        System.IO.Path.Combine(p.Value.GetHomePath(), j),
                        _returnFromGKImage,
                        false
                   ));
                }
            }

            return;
        }

        private string GetLibraryPathFromDialog()
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.SelectedPath = System.IO.Path.Combine(
                System.IO.Path.GetPathRoot(Environment.SystemDirectory),
                "GameKeeper"
            );
            fbd.Description = "Set the GameKeeper default library folder";
            var r = fbd.ShowDialog();

            if (r == System.Windows.Forms.DialogResult.OK)
            {
                return fbd.SelectedPath;
            }

            return null;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // This construct is used to fully initialize the main window.
            // Otherwise we lose focus and it ends up down the bottom of the z-axis.
            _GKLibraryPath = GetLibraryPathFromReg();
            if (_GKLibraryPath == null)
            {
                MessageBox.Show(
                    "Before continuing, please select a default directory for game storage.\n"
                    + "This is where GameKeeper will store relocated games.\n\n"
                    + "It can be changed later and GameKeeper will keep track of any previously moved games",
                    "Set GameKeeper storage directory"
                    );
                _GKLibraryPath = GetLibraryPathFromDialog();
                SetLibraryPathInReg(_GKLibraryPath);
            }
        }

        private static void OnDirectoryChanged(object source, System.IO.FileSystemEventArgs e)
        {   
            // This will update the UI not only when we move something within GK,
            // but also when something changes outside.
            MainWindow._main.Dispatcher.Invoke(new Action(delegate ()
            {
                ((MainWindow)Application.Current.MainWindow).BuildGameLCV();
            }));            
        }
        private static void OnDirectoryRenamed(object source, System.IO.RenamedEventArgs e)
        {
            // This will update the UI not only when we move something within GK,
            // but also when something changes outside.
            MainWindow._main.Dispatcher.Invoke(new Action(delegate ()
            {
                ((MainWindow)Application.Current.MainWindow).BuildGameLCV();
            }));
        }

        public MainWindow()
        {
            ILibraryLocator steam = new RegistryLibraryLocator("SOFTWARE\\WOW6432Node\\Valve\\Steam", "InstallPath", "SteamApps\\common");
            if (steam.GetLibraryPath() != null)
            {
                _libraries["Steam"] = new GenericGameLibrary(steam);

                System.IO.FileSystemWatcher watcher = new System.IO.FileSystemWatcher();
                watcher.Path = _libraries["Steam"].GetHomePath();
                watcher.NotifyFilter = System.IO.NotifyFilters.DirectoryName | System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.CreationTime;
                watcher.Created += new System.IO.FileSystemEventHandler(OnDirectoryChanged);
                watcher.Deleted += new System.IO.FileSystemEventHandler(OnDirectoryChanged);
                watcher.Renamed += new System.IO.RenamedEventHandler(OnDirectoryRenamed); // different signature, same thing.
                watcher.Filter = "*";
                watcher.EnableRaisingEvents = true;
                _watchers.Add(watcher);
            }
            
            InitializeComponent();
            _main = this;
            BuildGameLCV();
            CollectionViewSource gameCollectionViewSource = (CollectionViewSource)FindResource("GameCollectionViewSource");
            gameCollectionViewSource.Source = _gameView;

            Loaded += MainWindow_Loaded; // Handle post initialization
        }

        private void ExportButtonClick(object sender, RoutedEventArgs e)
        {
            var game = ((ExportImportButton)sender).GameName;
            var lib = ((ExportImportButton)sender).GameLibrary;
            var exporting = ((ExportImportButton)sender).IsExport;
            
            if (exporting)
            {
                var source = System.IO.Path.Combine(_libraries[lib].GetHomePath().ToString(), game);
                var dest = System.IO.Path.Combine(_GKLibraryPath, lib, game);

                try
                {
                    MoveGameDirectory(source, dest);
                    Junctions.CreateJunction(source, dest);
                }
                catch (OperationCanceledException) {
                    // make sure we dont create the junction 
                }
            }
            else
            {
                Junctions.GetJunctionTarget(
                    System.IO.Path.Combine(_libraries[lib].GetHomePath().ToString(), game),
                    out string source);
                var dest = System.IO.Path.Combine(_libraries[lib].GetHomePath().ToString(), game);

                
                Junctions.DeleteJunction(System.IO.Path.Combine(_libraries[lib].GetHomePath().ToString(), game));
                try
                {
                    MoveGameDirectory(source, dest);
                }
                catch (OperationCanceledException)
                {   // Put the junction back where it was
                    // Note cancelling the copy prevents a partial end product being created.
                    Junctions.CreateJunction(dest, source);
                }
            }
        }


        public void MoveGameDirectory(string dir, string new_path)
        {
            //FileSystem.MoveDirectory(dir, new_path, UIOption.AllDialogs, UICancelOption.ThrowException);
            try
            {
                FileSystem.CopyDirectory(dir, new_path, UIOption.AllDialogs, UICancelOption.ThrowException);               
            } 
            catch (OperationCanceledException)
            {   // In other words, "revert" by deleting the new directory and keeping the old one.
                FileSystem.DeleteDirectory(new_path, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
                throw;
            }
            FileSystem.DeleteDirectory(dir, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
        }
    }
}
