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

namespace Dictee
{
    /// <summary>
    /// Interaction logic for PhraseControl.xaml
    /// </summary>
    public partial class PhraseControl : UserControl
    {
        ViewModel _vm;
        SpeechSynthesizer _parole;
        string _phrase;

        public PhraseControl()
        {
            InitializeComponent();
        }

        public PhraseControl(int num, string phrase, SpeechSynthesizer parole, ViewModel vm)
        {
            InitializeComponent();
            NumLine.Content = num.ToString();
            _parole = parole;
            _phrase = phrase;
            NbError.Content = "-";
            _vm = vm;
            PhraseBox.TabIndex = num;

            if (phrase.Length > 60)
                DockCont.Height = 70 * (phrase.Length / 60);
        }

        public int Check(bool showRight = false)
        {
            int err = 0;

            if (!showRight)
                TbPhrase.Text = PhraseBox.Text;
            else
            {
                TbPhrase.Text = _phrase;
                TbPhrase.Foreground = new SolidColorBrush(Colors.ForestGreen);
            }

            string[] words = _phrase.Split(' ');

            PhraseBox.Text = PhraseBox.Text.Replace("  ", " ");

            string[] wordsToCheck = PhraseBox.Text.Split(' ');
            
            int nbrWordMax = words.Length > wordsToCheck.Length ? words.Length : wordsToCheck.Length;

            for (int w = 0; w < nbrWordMax; w++)
            {
                try
                {
                    string word = words[w];
                    int nbChar = word.Length > wordsToCheck[w].Length ? word.Length : wordsToCheck[w].Length;
                    for (int k = 0; k < nbChar; k++)
                    {
                        try
                        {
                            if (word[k] != wordsToCheck[w][k])
                            {
                                err++;
                                break;
                            }
                        }
                        catch
                        {
                            err++;
                            break;
                        }
                    }
                }
                catch
                {
                    err++;
                }
            }

            NbError.Content = err.ToString();

            return err;
        }

        public void SetPhrase(string p)
        {
            PhraseBox.Text = p;
        }

        public string GetPhrase()
        {
            return PhraseBox.Text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _parole.SpeakAsync(_phrase);
        }

        private void DockCont_LostFocus(object sender, RoutedEventArgs e)
        {
            _vm.SaveText();
        }
    }
}
