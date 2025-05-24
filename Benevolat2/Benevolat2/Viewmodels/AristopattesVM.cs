using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Aristopattes.Context;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Aristopattes.Viewmodels
{
    internal class AristopattesVM : ObservableObject
    {
        AristopattesContext context = new AristopattesContext();

        public event Action DemanderDisparitionMessage;
        private string _prenom;
        private string _nom;
        private string _courriel;
        private string _telephone;
        private int _numero;
        private string _message;
        private ObservableCollection<Client> _clients = new ObservableCollection<Client>();

        public string Prenom
        {
            get { return _prenom; }
            set
            {
                _prenom = value;
                OnPropertyChanged(nameof(Prenom));
            }
        }

        public string Nom
        {
            get { return _nom; }
            set
            {
                _nom = value;
                OnPropertyChanged(nameof(Nom));
            }
        }

        public string Courriel
        {
            get { return _courriel; }
            set
            {
                _courriel = value;
                OnPropertyChanged(nameof(Courriel));
            }
        }

        public string Telephone
        {
            get { return _telephone; }
            set
            {
                _telephone = value;
                OnPropertyChanged(nameof(Telephone));
            }
        }
        public int Numero
        {
            get { return _numero; }
            set
            {
                _numero = value;
                OnPropertyChanged(nameof(Numero));
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public ObservableCollection<Client> Clients { get { return _clients; } set { _clients = value; } }

        public AristopattesVM()
        {
            AjouterClient = new RelayCommand(AjoutClient);
        }

        public ICommand AjouterClient { get; }
        public void AjoutClient()
        {
            if (VerifInfos())
            {
                Random random = new Random();
                Numero = random.Next(1, 1000000);
                Numero = verifNumero(Numero, random);
                context.AjoutClient(Prenom, Nom, Telephone, Courriel, Numero);
            }
        }
        public int verifNumero(int numero, Random random)
        {
            bool identique = false;
            while (identique)
            {
                foreach (var client in Clients)
                {
                    if (numero == client.Numero)
                    {
                        identique = true;
                        numero = random.Next(1, 1000000);
                        break;
                        
                    }
                    else
                    {
                        identique = false;
                    }
                }
            }
            return numero;
        }
        public bool VerifInfos()
        {
            string regex = @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$";
            Message = null;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                DemanderDisparitionMessage?.Invoke(); // Vue va réagir ici
            };

            if (Prenom == null) //vérification du nom
            {
                Message = "Le prénom est manquant!";
                timer.Start();
                return false;
            }
            else if (Nom == null)
            {
                Message = "Le nom est manquant!";
                timer.Start();
                return false;
            }
                
            else if(Courriel == null)
            {
                Message = "Le courriel est manquant!";
                timer.Start();
                return false;
            }
            else if (!Regex.IsMatch(Courriel, regex, RegexOptions.IgnoreCase))
            {
                Message = "Le courriel est invalide";
                timer.Start();
                return false;
            }
            else if(Telephone == null)
            {
                Message = "Le numéro de téléphone est manquant";
                timer.Start();
            }
            return true;
        }
        
    }
}
