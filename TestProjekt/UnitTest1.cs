using System;
using System.Collections.Generic;
using System.Linq;
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPassword()
        {
            b.Password = null;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPassword1()
        {
            b.Password = "1A3";
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPassword2()
        {
            b.Password = "123456789A23456789123";
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPassword3()
        {
            b.Password = "Brugernavn12";
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPassword4()
        {
            b.Password = "jørgen12";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPassword5()
        {
            b.Password = "Jørgenetto";
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestEmail()
        {
            b.Email = "Meile.com";
        }

        /// <summary>
        /// slethistorik
        /// </summary>


        [TestMethod]
        public void Slethistorikunit()
        {
            Assert.AreEqual(null, service.SletHistorik(750));
        }


        [TestMethod]
        public void Slethistorikintegration()
        {
            Assert.AreEqual(null, client.SletHistorik(750));
        }







        //[TestMethod]
        //public void Opretbrugerunittest1()
        //{
        //    Assert.AreEqual("Brugernavnet findes allerede i databasen", service.OpretBruger("Jari", "Gjhjlkjl", "jari@dinmor.dk"));
        //}
        //[TestMethod]
        //public void Opretbrugerunittest2()
        //{
        //    Assert.AreEqual("Hans" + " er oprettet i databasen", service.OpretBruger("Hans", "J4566785", "Hans@gmail.com"));
        //}



        [TestMethod]
        public void Opretbrugerintegration1()
        {
            Assert.AreEqual("Brugernavnet findes allerede i databasen", client.OpretBruger("Jari", "Gjhjlkjl", "jari@dinmor.dk"));
        }
        [TestMethod]
        public void Opretbrugerintegration2()
        {
            Assert.AreEqual("linda" + " er oprettet i databasen", client.OpretBruger("linda", "J4566785", "Hans@gmail.com"));
        }

        [TestMethod]
        public void Opdaterpasswordunittest()
        {
            Assert.AreEqual("Der gik noget galt med at finde din bruger. Prøv igen", client.OpdaterPassword("dskjfhkfhksd", "Ugsfsdfsdf"));
        }

        [TestMethod]
        public void Opdaterpasswordunittest2()
        {
            Assert.AreEqual("Password er ændret", client.OpdaterPassword("Jari", "Dinmor3"));
        }


        [TestMethod]
        public void Opdaterpasswordintegrationtest()
        {
            Assert.AreEqual("Der gik noget galt med at finde din bruger. Prøv igen", client.OpdaterPassword("dskjfhkfhksd", "Ugsfsdfsdf"));
        }

        [TestMethod]
        public void Opdaterpasswordintegrationtest2()
        {
            Assert.AreEqual("Password er ændret", client.OpdaterPassword("Jari", "Dinmor3"));
        }



        /// <summary>
        /// opdateremail
        /// </summary>

        [TestMethod]
        public void Opdateremailintegrationtest()
        {

            //client.OpdaterEmail("Jari", "dinp67.com");
            Assert.AreEqual("Email er forkert" + " (" + "dinp67.com" + ")", client.OpdaterEmail("Jari", "dinp67.com"));
        }

        [TestMethod]
        public void Opdateremailunittest()
        {
            Assert.AreEqual("Der gik noget galt med at finde din bruger. Prøv igen", service.OpdaterEmail("simba", "s@tyu"));
        }

        //[TestMethod]
        //public void Opdateremilunittest2()
        //{
        //    Assert.AreEqual("Email er ændret", service.OpdaterEmail("Jari", "sercret12@gmail.com" ) );
        //}

        [TestMethod]
        public void Opdateremailintegration1()
        {
            Assert.AreEqual("Der gik noget galt med at finde din bruger. Prøv igen", client.OpdaterEmail("simba", "s@tyu"));
        }

        [TestMethod]
        public void Opdateremilintegration2()
        {
            Assert.AreEqual("Email er ændret", client.OpdaterEmail("Jari", "sercret12@gmail.com"));
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Tid()
        {
            tider.Fra = new TimeSpan(24, 00, 00);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Tid1()
        {
            tider.Fra = new TimeSpan(-1, 00, 00);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Tid2()
        {
            tider.Til = new TimeSpan(24, 00, 00);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Tid3()
        {
            tider.Til = new TimeSpan(-1, 00, 00);
        }
        //[TestMethod]
        //public void Henttemperatur()
        //{
        //    Assert.AreEqual( 161 ,service.HentTemperatur(3,6));
        //}



        //[TestMethod]
        //public void Henttidspunktreturner()
        //{
        //    Assert.AreEqual(0, service.HentTidspunkt(2015, 11, 11));
        //}

        [TestMethod]
        public void Henttidspunktretunerintegration()
        {

            Assert.AreEqual(0, client.HentTidspunkt(2015, 11, 11));
        }

        /// <summary>
        /// Glemt brugernavn test
        /// </summary>

        [TestMethod]
        public void Glemtbrugernavn()
        {
            Assert.AreEqual("Email er ikke gyldig (Skal indeholde @)", service.GlemtBrugernavn("jnnjij"));
        }

        [TestMethod]
        public void Glemtbrugernavnunit()
        {
            Assert.AreEqual("Email eksisterer ikke i databasen", service.GlemtBrugernavn("opop@fgf"));
        }
        [TestMethod]
        public void Glemtbrugernavnunit2()
        {
            Assert.AreEqual("Brugernavn er sendt til danielwinther@hotmail.dk", service.GlemtBrugernavn("danielwinther@hotmail.dk"));
        }

        [TestMethod]
        public void Glemtbrugernavnintegration()
        {
            Assert.AreEqual("Email er ikke gyldig (Skal indeholde @)", client.GlemtBrugernavn("jnnjij"));
        }

        [TestMethod]
        public void Glemtbrugernavnintegration1()
        {
            Assert.AreEqual("Email eksisterer ikke i databasen", client.GlemtBrugernavn("opop@fgf"));
        }
        [TestMethod]
        public void Glemtbrugernavnunintegration2()
        {
            Assert.AreEqual("Brugernavn er sendt til danielwinther@hotmail.dk", client.GlemtBrugernavn("danielwinther@hotmail.dk"));
        }

    }
}