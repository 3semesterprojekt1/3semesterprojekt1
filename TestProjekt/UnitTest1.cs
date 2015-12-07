using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProjekt.ServiceReference1;
using WCFServiceWebRole1;
using WCFServiceWebRole1.Models;

namespace TestProjekt
{
    [TestClass]
    public class UnitTest1
    {
        Service1Client client = new Service1Client();
        Service1 service = new Service1();
        private Tider tider;
        private Brugere b;

        [TestInitialize]
        public void BeforeTest()
        {
            tider = new Tider();
            b = new Brugere("Brugernavn", "Secret12", "email@email.dk");
        }

        #region TestPassword

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPasswordNull() //Unittest
        {
            b.Password = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPasswordMinLaengde() //Unittest
        {
            b.Password = "1A3"; //3
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPasswordMaxLaengde() //Unittest
        {
            b.Password = "123456789A23456789123123456789A2345678912"; //41
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPasswordBrugernavn() //Unittest
        {
            b.Password = "Brugernavn12";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPasswordStortBogstav() //Unittest
        {
            b.Password = "jørgen12";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPasswordTal() //Unittest
        {
            b.Password = "Jørgenetto";
        }

        #endregion

        #region TestEmail

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestEmailSnabelA() //Unittest
        {
            b.Email = "Meile.com";
        }

        #endregion

        #region TestSletHistorik

        [TestMethod]
        public void TestSletHistorik() //Unittest
        {
            Random r = new Random();
            bool actual = false;
            Bevaegelser b1 = service.SletHistorik(r.Next(350, 530));
            if (b1 != null)
            {
                actual = true;
            }
            using (DataContext dataContext = new DataContext())
            {
                dataContext.Bevaegelser.Add(b1);
                dataContext.SaveChanges();
            }
            Assert.IsTrue(actual);

        }

        public void TestSletHistorik1() //Integrationstest
        {
            Random r = new Random();
            bool actual = false;
            Bevaegelser b1 = client.SletHistorik(r.Next(350, 530));
            if (b1 != null)
            {
                actual = true;
            }
            using (DataContext dataContext = new DataContext())
            {
                dataContext.Bevaegelser.Add(b1);
                dataContext.SaveChanges();
            }
            Assert.AreEqual(true, actual);
        }

        #endregion

        #region TestOpretBruger

        [TestMethod]
        public void TestOpretBruger() //Integrationstest (Brugernavnet findes allerede)
        {
            Assert.AreEqual("Brugernavnet findes allerede i databasen",
                client.OpretBruger("Benjamin", "Secret12", "Belzamouri@gmail.com"));
        }

        [TestMethod]
        public void TestOpretBruger2() //Integrationstest (Brugernavnet findes ikke allerede)
        {
            Random r = new Random();
            string name = "linda" + r.Next(1000, 2000);
            Assert.AreEqual(name + " er oprettet i databasen",
                client.OpretBruger(name, "J4566785", name + "@gmail.com"));
            using (DataContext dataContext = new DataContext())
            {

                Brugere b = dataContext.Brugere.FirstOrDefault(bruger => bruger.Brugernavn == name);
                if (b != null)
                {
                    dataContext.Brugere.Remove(b);
                    dataContext.SaveChanges();
                }

            }
        }

        #endregion

        #region TestOpdaterPassword

        [TestMethod]
        public void TestOpdaterPassword() //Integrationstest (Brugernavn der ikke findes)
        {
            Assert.AreEqual("Der gik noget galt med at finde din bruger. Prøv igen",
                client.OpdaterPassword("dskjfhkfhksd", "Ugsfsdfsdf"));
        }

        [TestMethod]
        public void TestOpdaterPassword2() //Integrationstest (Brugernavn der findes)
        {
            Assert.AreEqual("Password er ændret", client.OpdaterPassword("Benjamin", "P98dfa50eee50910cb49bb06e65230e7d"));
        }

        #endregion

        #region TestOpdaterEmail

        [TestMethod]
        public void TestOpdaterEmailSnabelA() //Integrationstest (Email mangler "@")
        {
            Assert.AreEqual("Email er forkert" + " (" + "Belzamourimail.com" + ")", client.OpdaterEmail("Benjamin", "Belzamourimail.com"));
        }

        //[TestMethod]
        //public void Opdateremailunittest()
        //{
        //    Assert.AreEqual("Der gik noget galt med at finde din bruger. Prøv igen", service.OpdaterEmail("simba", "s@tyu"));
        //}

        [TestMethod]
        public void TestOpdaterEmailBruger() //Integrationstest (Brugeren findes ikke)
        {
            Assert.AreEqual("Der gik noget galt med at finde din bruger. Prøv igen",
                client.OpdaterEmail("simba", "s@tyu"));
        }

        [TestMethod]
        public void TestOpdaterEmail() //Integrationstest (Brugeren findes)
        {
            Assert.AreEqual("Email er ændret", client.OpdaterEmail("Benjamin", "Belzamouri@gmail.com"));
        }

        #endregion

        #region TestTider

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTiderFraMax() //Unittest (Fra må ikke være større end 23)
        {
            tider.Fra = new TimeSpan(24, 00, 00);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTiderFraNegativ() //Unittest (Fra må ikke være negativ)
        {
            tider.Fra = new TimeSpan(-1, 00, 00);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTiderTilMax() //Unittest (Til må ikke være større end 23)
        {
            tider.Til = new TimeSpan(24, 00, 00);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTiderTilNegativ() //Unittest (Til må ikke være negativ)
        {
            tider.Til = new TimeSpan(-1, 00, 00);
        }

        #endregion

        #region TestOpdaterTidsrum

        [TestMethod]
        public void TestOpdaterTidsrumMax() //Unitttest
        {
            Assert.AreEqual("Tallet er forkert", service.OpdaterTidsrum("19:00:00", "25:00:00"));
        }

        [TestMethod]
        public void TestOpdaterTidsrumMax1() //Unittest
        {
            Assert.AreEqual("Tallet er forkert", service.OpdaterTidsrum("25:00:00", "03:00:00"));
        }

        [TestMethod]
        public void TestOpdaterTidsrumNegativ() //Unittest
        {
            Assert.AreEqual("Tallet er forkert", service.OpdaterTidsrum("22:00:00", "-01:00:00"));
        }

        //[TestMethod]
        //public void TestOpdaterTidsrumMax2() //Integrationstest
        //{
        //    Assert.AreEqual("Tallet er forkert", client.OpdaterTidsrum("25:00:00", "03:00:00"));
        //}

        //[TestMethod]
        //public void TestOpdaterTidsrumNegativ1() //Integrationstest
        //{
        //    Assert.AreEqual("Tallet er forkert", client.OpdaterTidsrum("-01:00:00", "03:00:00"));
        //}

        //[TestMethod]
        //public void TestOpdaterTidsrumMax3() //Integrationstest
        //{
        //    Assert.AreEqual("Tallet er forkert", client.OpdaterTidsrum("19:00:00", "25:00:00"));
        //}

        #endregion
        
        #region TestHentTemperatur

        [TestMethod]
        public void TestHentTemperatur() //Unittest (Antal temperature i intervallet 3-6)
        {
            Assert.AreEqual(3, client.HentTemperatur(3, 11));
        }

        [TestMethod]
        public void TestHentTemperatur1() //Integrationstest (Antal temperature i intervallet 3-6)
        {
            Assert.AreEqual(3, client.HentTemperatur(3, 6));
        }

        #endregion

        #region TestHentTidspunkt

        [TestMethod]
        public void TestHentTidspunkt() //Unittest
        {
            Assert.AreEqual(0, service.HentTidspunkt(2015, 11, 11));
        }

        [TestMethod]
        public void TestHentTidspunkt1() //Integrationstest
        {
            Assert.AreEqual(0, client.HentTidspunkt(2015, 11, 11));
        }

        #endregion

        #region TestGlemtBrugernavn

        [TestMethod]
        public void TestGlemtBrugernavnSnabelA() //Unittest (Email indeholder ikke "@")
        {
            Assert.AreEqual("Email er ikke gyldig (Skal indeholde @)", service.GlemtBrugernavn("jnnjij"));
        }

        [TestMethod]
        public void TestGlemtBrugernavnEmail() //Unittest (Email findes ikke)
        {
            Assert.AreEqual("Email eksisterer ikke i databasen", service.GlemtBrugernavn("opop@fgf"));
        }

        [TestMethod]
        public void TestGlemtBrugernavn() //Unittest (Email findes)
        {
            Assert.AreEqual("Brugernavn er sendt til danielwinther@hotmail.dk",
                service.GlemtBrugernavn("danielwinther@hotmail.dk"));
        }

        [TestMethod]
        public void TestGlemtBrugernavnSnabelA1() //Integrationstest (Email indeholder ikke "@")
        {
            Assert.AreEqual("Email er ikke gyldig (Skal indeholde @)", client.GlemtBrugernavn("jnnjij"));
        }

        [TestMethod]
        public void TestGlemtBrugernavnEmail1() //Integrationstest (Email findes ikke)
        {
            Assert.AreEqual("Email eksisterer ikke i databasen", client.GlemtBrugernavn("opop@fgf"));
        }

        [TestMethod]
        public void TestGlemtBrugernavn1() //Integrationstest (Email findes)
        {
            Assert.AreEqual("Brugernavn er sendt til danielwinther@hotmail.dk",
                client.GlemtBrugernavn("danielwinther@hotmail.dk"));
        }

        #endregion

        #region TestHentBevaegelse

        [TestMethod]
        public void TestHentBevaegelse() //Unittest
        {
            Assert.AreEqual(3, service.HentBevaegelser("lol", "we").Count());
        }

        [TestMethod]
        public void TestHentBevaegelse1() //Integrationstest
        {
            Assert.AreEqual(3, client.HentBevaegelser().Count());
        }

        [TestMethod]
        public void TestHentBevaegelseSorterTidspunktFaldende()
        {
            Bevaegelser b = service.HentBevaegelser("Tidspunkt", "faldende")[0];
            Bevaegelser b1 = new Bevaegelser(new DateTime(2015, 04, 01), new TimeSpan(22, 45, 00), 10);
            Assert.AreEqual(b1.Tidspunkt, b.Tidspunkt);
        }

        [TestMethod]
        public void TestHentBevaegelseSorterTidspunktStigende()
        {
            Bevaegelser b = service.HentBevaegelser("Tidspunkt", "stigende")[0];
            Bevaegelser b1 = new Bevaegelser(new DateTime(2015, 02, 17), new TimeSpan(08, 22, 22), 8);
            Assert.AreEqual(b1.Tidspunkt, b.Tidspunkt);
        }

        [TestMethod]
        public void TestHentBevaegelseSorterDatoFaldende()
        {
            Bevaegelser b = service.HentBevaegelser("Dato", "faldende")[0];
            Bevaegelser b1 = new Bevaegelser(new DateTime(2015, 04, 01), new TimeSpan(22, 45, 00), 10);
            Assert.AreEqual(b1.Tidspunkt, b.Tidspunkt);
        }

        [TestMethod]
        public void TestHentBevaegelseSorterDatoStigende()
        {
            Bevaegelser b = service.HentBevaegelser("Dato", "stigende")[0];
            Bevaegelser b1 = new Bevaegelser(new DateTime(2015, 02, 17), new TimeSpan(08, 22, 22), 8);
            Assert.AreEqual(b1.Tidspunkt, b.Tidspunkt);
        }

        [TestMethod]
        public void TestHentBevaegelseSorterTemperaturFaldende()
        {
            Bevaegelser b = service.HentBevaegelser("Dato", "faldende")[0];
            Bevaegelser b1 = new Bevaegelser(new DateTime(2015, 04, 01), new TimeSpan(22, 45, 00), 10);
            Assert.AreEqual(b1.Tidspunkt, b.Tidspunkt);
        }

        [TestMethod]
        public void TestHentBevaegelseSorterTemperaturStigende()
        {
            Bevaegelser b = service.HentBevaegelser("Dato", "stigende")[0];
            Bevaegelser b1 = new Bevaegelser(new DateTime(2015, 02, 17), new TimeSpan(08, 22, 22), 8);
            Assert.AreEqual(b1.Tidspunkt, b.Tidspunkt);
        }

        #endregion

        #region TestLogin

        [TestMethod]
        public void TestLogin() //Integrationstest
        {
            string s = client.Login("Benjamin", "Secret12");
            Assert.AreEqual("Benjamin", s);
        }

        [TestMethod]
        public void TestLogin1() //Integrationstest
        {
            string s = service.Login("Homo", "Secret12");
            Assert.AreEqual("Brugernavnet/passwordet er forkert", s);
        }

        #endregion


       

    }
}