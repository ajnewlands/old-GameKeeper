using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.VisualBasic.FileIO;

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
        Dictionary<string, ILibrary> _libraries = new Dictionary<string, ILibrary>();

        private string _sendToGKImage = "Import_16x.png";
        private string _returnFromGKImage = "Export_16x.png";

        private string _GKLibraryPath = "C:\\GameKeeper";

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

        [STAThread]
        private string GetGKLibraryPath()
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
            var p = GetGKLibraryPath();
        }

        public MainWindow()
        {
            ILibraryLocator steam = new RegistryLibraryLocator("SOFTWARE\\WOW6432Node\\Valve\\Steam", "InstallPath", "SteamApps\\common");
            if (steam.GetLibraryPath() != null)
            {
                _libraries["Steam"] = new GenericGameLibrary(steam);
            }
            
            InitializeComponent();
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
                MessageBox.Show("moving " + source + " to " + dest);
                MoveGameDirectory(source, dest);
                Junctions.CreateJunction(source, dest);
            }
            else
            {
                Junctions.GetJunctionTarget(
                    System.IO.Path.Combine(_libraries[lib].GetHomePath().ToString(), game),
                    out string source);
                var dest = System.IO.Path.Combine(_libraries[lib].GetHomePath().ToString(), game);
                MessageBox.Show("moving " + source + " to " + dest);

                Junctions.DeleteJunction(System.IO.Path.Combine(_libraries[lib].GetHomePath().ToString(), game));
                MoveGameDirectory(source, dest);
            }

            BuildGameLCV();
        }


        public void MoveGameDirectory(string dir, string new_path)
        {
            FileSystem.MoveDirectory(dir, new_path, UIOption.AllDialogs, UICancelOption.ThrowException);
            return;
        }
    }
}
