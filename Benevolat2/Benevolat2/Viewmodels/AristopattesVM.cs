using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
        private string _htmlFilePath;
        private string _message;
        private ObservableCollection<Client> _clients = new ObservableCollection<Client>();
        private CryptoHelper _cryptoHelper = new();
        private AristopattesContext _context = new();

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

        public string HtmlFilePath
        {
            get => _htmlFilePath;
            set
            {
                _htmlFilePath = value;
                OnPropertyChanged(nameof(HtmlFilePath));
            }
        }

        public ObservableCollection<Client> Clients { get { return _clients; } set { _clients = value; } }

        public AristopattesVM()
        {
            AjouterClient = new RelayCommand(AjoutClient);
            ChargerClientsDepuisBD();
            HtmlFilePath = GenererPageHtml(Clients);
        }

        private void ChargerClientsDepuisBD()
        {
            var listeClients = context.LireClients(); // ← À adapter selon ta méthode exacte
            foreach (var client in listeClients)
            {
                client.Courriel = _cryptoHelper.DecryptString(client.Courriel); // déchiffrement si besoin
                Clients.Add(client);
            }
        }

        public ICommand AjouterClient { get; }
        public void AjoutClient()
        {
            if (VerifInfos())
            {
                Random random = new Random();
                Numero = random.Next(1, 999999);
                Numero = verifNumero(Numero, random);
                string courrielChiffre = _cryptoHelper.EncryptString(Courriel);
                context.AjoutClient(Prenom, Nom, Telephone, courrielChiffre, Numero);
                Courriel = _cryptoHelper.DecryptString(courrielChiffre);
                Clients.Add(new Client
                {
                    Prenom = Prenom,
                    Telephone = Telephone,
                    Nom = Nom,
                    Courriel = Courriel,
                    Numero = Numero,
                });
                var nouveauClient = new Client
                {
                    Prenom = Prenom,
                    Nom = Nom,
                    Courriel = Courriel,
                    Telephone = Telephone,
                    Numero = Numero
                };
                
                EnvoiCourrielAuClient(nouveauClient);
                HtmlFilePath = GenererPageHtml(Clients);
            }
        }
        public int verifNumero(int numero, Random random)
        {
            bool identique;
            do
            {
                identique = false;
                foreach (var client in Clients)
                {
                    if (numero == client.Numero)
                    {
                        identique = true;
                        numero = random.Next(1, 999999);
                        break;

                    }
                    else
                    {
                        identique = false;
                    }
                }
            } while (identique);
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
        
        private void EnvoiCourrielAuClient(Client client)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                DemanderDisparitionMessage?.Invoke(); // Vue va réagir ici
            };
            var message = new MailMessage("etiennetremblay03@gmail.com", client.Courriel);
            message.Subject = "Bienvenue !";
            message.Body = $"Bonjour {client.Prenom} {client.Nom},\n\nMerci pour votre inscription. Votre numéro est : {client.Numero:D6}.";

            using var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("etiennetremblay03@gmail.com", _context.GetSetting("mdpEnvoiCourriel")),
                EnableSsl = true
            };

            try
            {
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                Message = "Erreur lors de l'envoi du courriel : " + ex.Message;
                timer.Start();
            }
        }

        public string GenererPageHtml(ObservableCollection<Client> clients)
        {
            string tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "clients.html");

            string html = @"<!DOCTYPE html>
                            <html>
                            <head>
                                <meta charset='utf-8'>
                                <title>Liste des clients</title>
                                <style>
                                    table { border-collapse: collapse; width: 100%; }
                                    th, td { border: 1px solid black; padding: 8px; }
                                    th { background-color: #f2f2f2; }
                                </style>
                            </head>
                            <body>
                                <h2>Liste des clients</h2>
                                <table>
                                    <tr><th>Prenom</th><th>Nom</th><th>Email</th><th>Téléphone</th><th>Numéro</th></tr>";
            if (clients.Count > 0)
            {
                foreach (var client in clients)
                {
                    html += $"<tr><td>{client.Prenom}</td><td>{client.Nom}</td><td>{client.Courriel}</td><td>{client.Telephone}</td><td>{client.Numero:D6}</td></tr>";
                }
            }
            else
            {
                html += " <tr><td colspan='5'> La liste des clients est actuellement vide</td></tr>";
            }


            html += @"
                                </table>
                            </body>
                            </html>";

            File.WriteAllText(tempFilePath, html);
            return tempFilePath;
        }
    }
}
