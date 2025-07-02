using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aristopattes.Context
{
    internal class AristopattesContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }

        private IConfigurationRoot configuration;

        public AristopattesContext()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public string GetSetting(string key)
        {
            return configuration[key];
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                string connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }
        }

        public void AjoutClient(string prenom, string nom, string telephone, string courriel, int numero)
        {
            var client = new Client
            {
                Prenom = prenom,
                Nom = nom,
                Courriel = courriel,
                Telephone = telephone,
                Numero = numero
            };
            Clients.Add(client);
            SaveChanges();
        }
        public List<Client> LireClients()
        {
            return Clients.ToList(); // ← ou DbSet<Client>.ToList() selon ton ORM (EF ?)
        }
    }
}
