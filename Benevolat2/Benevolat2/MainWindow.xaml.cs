using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Aristopattes.Viewmodels;
using System.IO;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Aristopattes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new AristopattesVM();

            if (DataContext is AristopattesVM vm)
            {
                vm.DemanderDisparitionMessage += LancerAnimationDisparition;
                vm.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(vm.HtmlFilePath))
                    {
                        webBrowser.Navigate(new Uri(vm.HtmlFilePath));
                    }
                };
                if (!string.IsNullOrWhiteSpace(vm.HtmlFilePath))
                {
                    webBrowser.Navigate(new Uri(vm.HtmlFilePath));
                }
            }
        }
        private void LancerAnimationDisparition()
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(1)
            };

            Message.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void TelephoneTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, e.Text.Length - 1);
        }
        private void PrenomAndNomTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsLetter(e.Text, e.Text.Length - 1);
        }
        private bool _backspacing = false;
        private void TelephoneKeyDown(object sender, KeyEventArgs e)
        {
            _backspacing = e.Key == Key.Back;
        }

        private void TelephoneTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string numbersOnly = new string(textBox.Text.Where(char.IsDigit).ToArray());

            int cursor = textBox.SelectionStart;

            if (_backspacing)
            {
                _backspacing = false;
                return;
            }

            if (numbersOnly.Length > 0)
            {
                if (numbersOnly.Length <= 3)
                    textBox.Text = numbersOnly;
                else if (numbersOnly.Length <= 6)
                    textBox.Text = $"{numbersOnly.Substring(0, 3)}-{numbersOnly.Substring(3)}";
                else if (numbersOnly.Length <= 10)
                    textBox.Text = $"{numbersOnly.Substring(0, 3)}-{numbersOnly.Substring(3, 3)}-{numbersOnly.Substring(6)}";
                else
                    textBox.Text = $"{numbersOnly.Substring(0, 3)}-{numbersOnly.Substring(3, 3)}-{numbersOnly.Substring(6, 4)}";

                // Repositionner le curseur à la fin
                textBox.SelectionStart = textBox.Text.Length;
            }
        }
    }
}