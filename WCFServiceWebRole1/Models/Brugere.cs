using System.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace WCFServiceWebRole1.Models
{
    

    [Table("Brugere")]
    [DataContract]
    public class Brugere
    {
        private string _password;
        private string _email;
        public int Id { get; set; }

        [DataMember]
        [Required]
        [StringLength(50)]
        public string Brugernavn { get; set; }

        [DataMember]
        [Required]
        [StringLength(100)]
        public string Password
        {
            get { return _password; }
            set
            {
                CheckPassword(value);
                _password = value;
            }
        }
        // ReSharper disable once UnusedParameter.Local
        private void CheckPassword(string password)
        {
            if (password == null)
            {
                throw new ArgumentException("Password er tomt");
            }
            if (password.Length < 4)
            {
                throw new ArgumentException("Password skal være mere end 4 tegn");
            }
            if (password.Length > 40)
            {
                throw new ArgumentException("Password skal være mindre end 20 tegn");
            }
            if (!password.Any(char.IsUpper))
            {
                throw new ArgumentException("Password skal indeholde ét stort bogstav");
            }
            if (!password.Any(char.IsDigit))
            {
                throw new ArgumentException("Password skal indeholde ét tal");
            }
            if (password.Contains(Brugernavn))
            {
                throw new ArgumentException("Password må ikke indeholde dit brugernavn");
            }
        }

        [DataMember]
        [Required]
        [StringLength(50)]
        public string Email
        {
            get { return _email; }
            set
            {
                CheckEmail(value);
                _email = value;
            }
        }

        private void CheckEmail(string email)
        {
            if (!email.Contains("@"))
            {
                throw new ArgumentException("Email er forkert" + " (" + email + ")");
            }
        }

        public Brugere(string brugernavn, string password, string email)
        {
            Brugernavn = brugernavn;
            _password = password;
            _email = email;

        }

        public Brugere()
        {
            
        }
    }
}
