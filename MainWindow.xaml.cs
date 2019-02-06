using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Speech;
using System.Speech.Synthesis;

using System.Collections.ObjectModel;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Globalization;

namespace Dictee
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();
            _vm = new ViewModel(StackPan);
            DataContext = _vm;
        }
    }

    public class ViewModel : NotifyBaseClass
    {
        //public List<string> Phrases { get; set; }
        public ObservableCollection<string> Phrases { get; set; }
        public int PhraseId { get; set; }

        bool IsChecked { get; set; }
        bool IsValidate { get; set; }
        bool InProgress { get; set; }

        SpeechSynthesizer parole;
        StackPanel _stack;

        public ViewModel(StackPanel stack)
        {
            string a = "abcd";
            var c = a[1];
            IsChecked = false;
            InProgress = false;
            IsValidate = false;
            NbrErrorValidate = 0;
            MainIsVisible = false;

            parole = new SpeechSynthesizer();

            _stack = stack;
            Phrases = new ObservableCollection<string>();
            var v = parole.GetInstalledVoices();
            string t = "";
            foreach (InstalledVoice iv in v)
                t += iv.VoiceInfo.Name + ", ";

            try
            {
                parole.SelectVoice("Microsoft Hortense Desktop");
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error set language Virginie : " + t);
                if (v.Count > 0)
                    parole.SelectVoice(v[0].VoiceInfo.Name);
            }

            InitializeCommand();
        }

        public void InitializeCommand()
        {
            CommandStart = new RoutedUICommand( "Start", "TestMenuClick", typeof(ViewModel) );
            CommandBinding commandStartBinding = new CommandBinding(CommandStart, Start, CanStart);
            CommandManager.RegisterClassCommandBinding( typeof(UIElement ), commandStartBinding );

            CommandNext = new RoutedUICommand("Next", "TestMenuClick", typeof(ViewModel));
            CommandBinding commandNextBinding = new CommandBinding(CommandNext, Next, CanNext);
            CommandManager.RegisterClassCommandBinding(typeof(UIElement), commandNextBinding);


            CommandRead = new RoutedUICommand("Read", "TestMenuClick", typeof(ViewModel));
            CommandBinding commandReadBinding = new CommandBinding(CommandRead, Read, CanRead);
            CommandManager.RegisterClassCommandBinding(typeof(UIElement), commandReadBinding);

            CommandCheck = new RoutedUICommand("Check", "TestMenuClick", typeof(ViewModel));
            CommandBinding commandCheckBinding = new CommandBinding(CommandCheck, Check, CanCheck);
            CommandManager.RegisterClassCommandBinding(typeof(UIElement), commandCheckBinding);

            CommandOpen = new RoutedUICommand("Open", "TestMenuClick", typeof(ViewModel));
            CommandBinding commandOpenBinding = new CommandBinding(CommandOpen, Open, CanOpen);
            CommandManager.RegisterClassCommandBinding(typeof(UIElement), commandOpenBinding);

            CommandValidate = new RoutedUICommand("Validate", "TestMenuClick", typeof(ViewModel));
            CommandBinding commandValidateBinding = new CommandBinding(CommandValidate, Validate, CanValidate);
            CommandManager.RegisterClassCommandBinding(typeof(UIElement), commandValidateBinding);

            CommandShowMain = new RoutedUICommand("ShowMain", "TestMenuClick", typeof(ViewModel));
            CommandBinding commandShowMainBinding = new CommandBinding(CommandShowMain, ShowMain, CanShowMain);
            CommandManager.RegisterClassCommandBinding(typeof(UIElement), commandShowMainBinding);
        }

        public RoutedUICommand CommandShowMain { get; set; }
        void ShowMain(object param, ExecutedRoutedEventArgs e)
        {
            MainIsVisible = !MainIsVisible;
        }

        void CanShowMain(object param, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public RoutedUICommand CommandValidate { get; set; }
        void Validate(object param, ExecutedRoutedEventArgs e)
        {
            var list = _stack.Children;
            int err = 0;
            foreach( UIElement p in list )
            {
                if( p is PhraseControl )
                {
                    PhraseControl cont = p as PhraseControl;
                    err += cont.Check(true);
                }
            }

            NbrErrorValidate = err;
            
            IsValidate = true;
        }

        void CanValidate(object param, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsChecked && !IsValidate;
        }

        public RoutedUICommand CommandOpen { get; set; }
        void Open(object param, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.InitialDirectory = Directory.GetCurrentDirectory();
            of.Filter = "Text Files (.txt)|*.txt";
            var res = of.ShowDialog();

            if (res == DialogResult.OK)
            {
                Filename = of.SafeFileName;

                int cpt = 0;
                Phrases.Clear();
                var lines = File.ReadAllLines(of.FileName, Encoding.GetEncoding(1252));
                foreach (string l in lines)
                {
                    if (l[0] != '-' && l != "")
                    {
                        Phrases.Add(l);
                        ((PhraseControl)_stack.Children[cpt]).SetPhrase(l);
                        cpt++;
                    }
                }
            }
        }

        void CanOpen(object param, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public RoutedUICommand CommandCheck { get; set; }
        void Check(object param, ExecutedRoutedEventArgs e)
        {
            var list = _stack.Children;
            int err = 0;
            foreach( UIElement p in list )
            {
                if( p is PhraseControl )
                {
                    PhraseControl cont = p as PhraseControl;
                    err += cont.Check();
                }
            }

            NbrError = err;
            IsChecked = true;

            SaveText("Corr_" + Filename.Split('.')[0] + "_" + err.ToString() + ".txt");
        }

        void CanCheck(object param, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !IsChecked;
        }

        public RoutedUICommand CommandRead { get; set; }
        void Read(object param, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.InitialDirectory = Directory.GetCurrentDirectory();
            of.Filter = "Text Files (.txt)|*.txt";
            var res = of.ShowDialog();

            if (res == DialogResult.OK)
            {
                Filename = of.SafeFileName;

                int cpt = 1;
                Phrases.Clear();
                var lines = File.ReadAllLines(of.FileName, Encoding.GetEncoding(1252));
                foreach (string l in lines)
                {
                    if (l[0] != '-' && l != "")
                    {
                        Phrases.Add(l);
                        _stack.Children.Add(new PhraseControl(cpt, l, parole, this));
                        cpt++;
                    }
                }

                PhraseInProgress = Phrases[0];
                PhraseId = 0;

                InProgress = true;
            }
        }

        void CanRead(object param, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !InProgress;
        }

        public RoutedUICommand CommandNext { get; set; }
        void Next(object param, ExecutedRoutedEventArgs e)
        {
            if (PhraseId < Phrases.Count - 1)
                PhraseId++;

            PhraseInProgress = Phrases[PhraseId];
        }

        void CanNext(object param, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }


        public RoutedUICommand CommandStart { get; set; }
        void Start(object param, ExecutedRoutedEventArgs e)
        {
        }

        void CanStart(object param, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public void SaveText(string filename = "tmp.txt")
        {
            List<string> phrase = new List<string>();
            var list = _stack.Children;
            foreach (UIElement p in list)
            {
                if (p is PhraseControl)
                {
                    PhraseControl cont = p as PhraseControl;
                    phrase.Add(cont.GetPhrase());
                }
            }

            File.WriteAllLines(filename, phrase, Encoding.GetEncoding(1252));
        }

        public bool MainIsVisible
        {
            get => _mainIsVisible;
            set
            {
                if (value != _mainIsVisible)
                {
                    _mainIsVisible = value;
                    NotifyPropertiyChanged(nameof(MainIsVisible));
                }
            }
        }
        private bool  _mainIsVisible;


        public int NbrErrorValidate
        {
            get => _nbrErrorValidate;
            set
            {
                if (value != _nbrErrorValidate)
                {
                    _nbrErrorValidate = value;
                    NotifyPropertiyChanged(nameof(NbrErrorValidate));
                }
            }
        }
        private int _nbrErrorValidate;


        public int NbrError
        {
            get => _nbrError;
            set
            {
                if (value != _nbrError)
                {
                    _nbrError = value;
                    NotifyPropertiyChanged(nameof(NbrError));
                }
            }
        }
        private int _nbrError;


        public string Filename
        {
            get => _filename;
            set
            {
                if (value != _filename)
                {
                    _filename = value;
                    NotifyPropertiyChanged(nameof(Filename));
                }
            }
        }
        private string _filename;


        public string PhraseInProgress
        {
            get => _phraseInProgress;
            set
            {
                if (value != _phraseInProgress)
                {
                    _phraseInProgress = value;
                    NotifyPropertiyChanged(nameof(PhraseInProgress));
                }
            }
        }
        private string _phraseInProgress;

    }


    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
