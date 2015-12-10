using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using SendGrid;
using WCFServiceWebRole1.Models;

namespace WCFServiceWebRole1
{
    /// <summary>
    /// Klassenavndeklaration
    /// </summary>
    public class Service1 : IService1
    {
        private const int Port = 7000;
        private static UdpClient _client;
        private static IPEndPoint _ipAddress;
        private static DateTime _senesteDato;
        private static TimeSpan _senesteTid;
        private static Task _ta;
        private static bool _alarmBool;
        private static FileStream _fileStream;
        private static TextWriterTraceListener _tracer;

        /// <summary>
        /// Konstruktør
        /// </summary>
        public Service1()
        {
            try
            {
                if (_client == null)
                {
                    _client = new UdpClient(Port) {EnableBroadcast = true};
                }
                if (_ipAddress == null)
                {
                    _ipAddress = new IPEndPoint(IPAddress.Any, Port);
                }
                if (_fileStream == null)
                {
                    string sti = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName + "/pandpLog";
                    if (Directory.Exists(sti))
                    {
                        _fileStream = new FileStream(sti + "/pandplogfile.txt", FileMode.Append);
                    }
                    else
                    {
                        Directory.CreateDirectory(sti);
                        _fileStream = new FileStream(sti + "/pandplogfile.txt", FileMode.Append);
                    }
                }
                    
                if (_tracer == null)
                {
                    _tracer = new TextWriterTraceListener(_fileStream);
                    Trace.Listeners.Add(_tracer);
                    Trace.AutoFlush = true;
                    Trace.WriteLine("-------------------------------------------------------");
                    Trace.WriteLine("[" + DateTime.Now + "]" + " - " + "Webservice staret..");
                }
                if (_ta == null)
                {
                    _ta = Task.Run((() => SensorLoop()));
                }
        }
            catch (Exception)
            {
               
            }
        }

        /// <summary>
        /// Sletter et bevægelses element med det pågældende id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>bevægelsen med id(id)</returns>
        public Bevaegelser SletHistorik(int id)
        {
            using (DataContext dataContext = new DataContext())
            {
                Bevaegelser b = dataContext.Bevaegelser.FirstOrDefault(bevaegelse => bevaegelse.Id == id);
                if (b != null)
                {
                    dataContext.Bevaegelser.Remove(b);
                    dataContext.SaveChanges();
                    TraceHjaelp(new []{id.ToString()}, b.ToString());
                    return b;
                }
                TraceHjaelp(new []{id.ToString()}, "null");
                return null;
            }
        }

        /// <summary>
        /// Opretter en bruger med de pågældende parametre
        /// </summary>
        /// <param name="brugernavn"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <returns>string med resultat</returns>
        public string OpretBruger(string brugernavn, string password, string email)
        {
            using (DataContext dataContext = new DataContext())
            {
                Brugere exBruger = FindBruger(brugernavn);
                if (exBruger == null)
                {
                    if (brugernavn == null)
                    {
                        string fejlStreng = "Brugernavnet skal udfyldes";
                        TraceHjaelp(new[] { brugernavn, "EncryptedText", email }, fejlStreng);
                        return fejlStreng;
                    }
                    if (password == null)
                    {
                        string fejlStreng1 = "Password skal udfyldes";
                        TraceHjaelp(new[] { brugernavn, "EncryptedText", email }, fejlStreng1);
                        return fejlStreng1;
                    }
                    if (email == null)
                    {
                        string fejlStreng2 = "Email skal udfyldes";
                        TraceHjaelp(new[] {brugernavn, "EncryptedText", email}, fejlStreng2);
                        return fejlStreng2;
                    }
                    try
                    {
                        Brugere b = new Brugere() {Brugernavn = brugernavn, Password = password, Email = email};
                        b.Password = KrypterStreng(password);
                        dataContext.Brugere.Add(b);
                        dataContext.SaveChanges();
                        string succesStreng = brugernavn + " er oprettet i databasen";
                        TraceHjaelp(new[] {brugernavn, "EncryptedText", email}, succesStreng);
                        return succesStreng;
                    }
                    catch (ArgumentException ex)
                    {
                        TraceHjaelp(new[] {brugernavn, "EncryptedText", email}, ex.Message);
                        return ex.Message;
                    }
                }

            }
                string fejlStreng3 = "Brugernavnet findes allerede i databasen";
                TraceHjaelp(new[] { brugernavn, "EncryptedText", email }, fejlStreng3);
                return fejlStreng3;
        }


        /// <summary>
        /// Opdaterer den pågældende brugers password til det skrevne password
        /// </summary>
        /// <param name="brugernavn"></param>
        /// <param name="password"></param>
        /// <returns>string med resultat</returns>
        public string OpdaterPassword(string brugernavn, string password)
        {
            using (DataContext dataContext = new DataContext())
            {
                Brugere b = FindBruger(brugernavn);
                if (b != null)
                {
                    try
                    {
                        b.Password = password;
                        b.Password = KrypterStreng(password);
                        dataContext.Brugere.AddOrUpdate(b);
                        dataContext.SaveChanges();
                        string succesStreng = "Password er ændret";
                        TraceHjaelp(new []{brugernavn, "EncryptedText"}, succesStreng);
                        return succesStreng;
                    }
                    catch (ArgumentException ex)
                    {
                        TraceHjaelp(new[] { brugernavn, "EncryptedText" }, ex.Message);
                        return ex.Message;
                    }
                }
                string fejlStreng = "Der gik noget galt med at finde din bruger. Prøv igen";
                TraceHjaelp(new[] { brugernavn, "EncryptedText" }, fejlStreng);
                return fejlStreng;
            }
        }

        /// <summary>
        /// Opdaterer den pågældende brugers email til den skrevne email
        /// </summary>
        /// <param name="brugernavn"></param>
        /// <param name="email"></param>
        /// <returns>string med resultat</returns>
        public string OpdaterEmail(string brugernavn, string email)
        {
            using (DataContext dataContext = new DataContext())
            {
                Brugere b = FindBruger(brugernavn);
                if (b != null)
                {
                    try
                    {
                        b.Email = email;
                        dataContext.Brugere.AddOrUpdate(b);
                        dataContext.SaveChanges();
                        string succesStreng = "Email er ændret";
                        TraceHjaelp(new []{brugernavn, email}, succesStreng);
                        return succesStreng;
                    }
                    catch (ArgumentException ex)
                    {
                        TraceHjaelp(new[] { brugernavn, email }, ex.Message);
                        return ex.Message;
                    }
                }
                string fejlStreng = "Der gik noget galt med at finde din bruger. Prøv igen";
                TraceHjaelp(new[] { brugernavn, email }, fejlStreng);
                return fejlStreng;
            }
        }

        /// <summary>
        /// Tjekker på brugerens login
        /// </summary>
        /// <param name="brugernavn"></param>
        /// <param name="password"></param>
        /// <returns>string med resultat</returns>
        public string Login(string brugernavn, string password)
        {
            Brugere b1 = FindBruger(brugernavn);

            if (b1 != null && password != null)
            {
                if (b1.Password == KrypterStreng(password))
                {
                    TraceHjaelp(new []{brugernavn, "EncryptedText"}, "Login succesfuldt");
                    return b1.Brugernavn;
                }
            }
            string fejlStreng = "Brugernavnet/passwordet er forkert";
            TraceHjaelp(new[] {brugernavn, "EncryptedText"}, fejlStreng);
            return fejlStreng;
        }

        /// <summary>
        /// Henter alle bevaegelser
        /// </summary>
        /// <param name="kolonne">Sortere på den valgte kolonne</param>
        /// <param name="faldendeEllerStigende"></param>
        /// <returns>En liste med alle bevægelser</returns>
        public List<Bevaegelser> HentBevaegelser(string kolonne, string faldendeEllerStigende)
        {
            TraceHjaelp(new []{kolonne, faldendeEllerStigende}, "kaldt");
            using (DataContext dataContext = new DataContext())
            {
                switch (kolonne)
                {
                    case "Tidspunkt":
                        switch (faldendeEllerStigende)
                        {
                            case "stigende":
                            {
                                var query = from q in dataContext.Bevaegelser orderby q.Tidspunkt ascending select q;
                                    TraceHjaelp(new[] { kolonne, faldendeEllerStigende }, query.Count().ToString() + " Elementer returneret");
                                    return query.ToList();
                            }
                            case "faldende":
                            {
                                var query = from q in dataContext.Bevaegelser orderby q.Tidspunkt descending select q;
                                    TraceHjaelp(new[] { kolonne, faldendeEllerStigende }, query.Count().ToString() + " Elementer returneret");
                                    return query.ToList();
                            }
                            default:
                                var queryDefault = from q in dataContext.Bevaegelser orderby q.Dato descending orderby q.Tidspunkt descending select q;
                                TraceHjaelp(new[] { kolonne, faldendeEllerStigende }, queryDefault.Count().ToString() + " Elementer returneret");
                                return queryDefault.ToList();
                        }
                    case "Dato":
                        switch (faldendeEllerStigende)
                        {
                            case "stigende":
                            {
                                var query = from q in dataContext.Bevaegelser orderby q.Dato ascending select q;
                                    TraceHjaelp(new[] { kolonne, faldendeEllerStigende }, query.Count().ToString() + " Elementer returneret");
                                    return query.ToList();
                            }
                            case "faldende":
                            {
                                var query = from q in dataContext.Bevaegelser orderby q.Dato descending select q;
                                    TraceHjaelp(new[] { kolonne, faldendeEllerStigende }, query.Count().ToString() + " Elementer returneret");
                                    return query.ToList();
                            }
                            default:
                                var queryDefault = from q in dataContext.Bevaegelser orderby q.Dato descending orderby q.Dato descending select q;
                                TraceHjaelp(new[] { kolonne, faldendeEllerStigende }, queryDefault.Count().ToString() + " Elementer returneret");
                                return queryDefault.ToList();
                        }
                    case "Temperatur":
                        switch (faldendeEllerStigende)
                        {
                            case "stigende":
                            {
                                var query = from q in dataContext.Bevaegelser orderby q.Temperatur ascending select q;
                                    TraceHjaelp(new[] { kolonne, faldendeEllerStigende }, query.Count().ToString() + " Elementer returneret");
                                    return query.ToList();
                            }
                            case "faldende":
                            {
                                var query = from q in dataContext.Bevaegelser orderby q.Temperatur descending select q;
                                    TraceHjaelp(new[] { kolonne, faldendeEllerStigende }, query.Count().ToString() + " Elementer returneret");
                                    return query.ToList();
                            }
                            default:
                                var queryDefault = from q in dataContext.Bevaegelser orderby q.Dato descending orderby q.Temperatur descending select q;
                                TraceHjaelp(new[] { kolonne, faldendeEllerStigende }, queryDefault.Count().ToString() + " Elementer returneret");
                                return queryDefault.ToList();
                        }
                    default:
                        var queryDefault1 = from q in dataContext.Bevaegelser orderby q.Dato descending orderby q.Tidspunkt descending select q;
                        TraceHjaelp(new[] { kolonne, faldendeEllerStigende }, queryDefault1.Count().ToString() + " Elementer returneret");
                        return queryDefault1.ToList();
                }
            }
        }
        /// <summary>
        /// Henter temperatur i det skrevne interval
        /// </summary>
        /// <param name="startInterval"></param>
        /// <param name="slutInterval"></param>
        /// <returns>int med antal bevægelser i intervallet</returns>
        public int HentTemperatur(int startInterval, int slutInterval)
        {
            using (DataContext datacontext = new DataContext())
            {
                var query = from p in datacontext.Bevaegelser
                            where p.Temperatur >= startInterval && p.Temperatur <= slutInterval
                            select p;
                int succesInt = query.Count();
                TraceHjaelp(new []{startInterval.ToString(), slutInterval.ToString()}, succesInt.ToString());
                return succesInt;
            }
            
        }

        /// <summary>
        /// Henter bevægelser i det skrevne interval
        /// </summary>
        /// <param name="aarstal"></param>
        /// <param name="maaned"></param>
        /// <param name="slutdag"></param>
        /// <returns>int med antal bevægelser i intervallet</returns>
        public int HentTidspunkt(int aarstal, int maaned, int slutdag)
        {
            try
            {
                if (aarstal.ToString().Length == 4 && maaned >= 1 && maaned <= 12 && slutdag >= 1 && slutdag <= 31)
                {
                    DateTime startsDato = new DateTime(aarstal, maaned, 1);
                    DateTime slutsDato = new DateTime(aarstal, maaned, slutdag);
                    using (DataContext dataContext = new DataContext())
                    {
                        var query = from q in dataContext.Bevaegelser
                            where q.Dato >= startsDato
                            where q.Dato <= slutsDato
                            select q;
                        int succesInt = query.Count();
                        TraceHjaelp(new[] { aarstal.ToString(), maaned.ToString(), slutdag.ToString() }, succesInt.ToString());
                        return succesInt;
                    }
                }
                int fejlInt = 0;
                TraceHjaelp(new[] { aarstal.ToString(), maaned.ToString(), slutdag.ToString() }, fejlInt.ToString());
                return fejlInt;
            }
            catch (Exception ex)
            {
                TraceHjaelp(new []{aarstal.ToString(), maaned.ToString(), slutdag.ToString()}, ex.Message);
                return 0;
            }
        }


        /// <summary>
        /// Sender nyt password e-mail
        /// </summary>
        /// <param name="brugernavn"></param>
        /// <returns>string med resultat</returns>
        public string GlemtPassword(string brugernavn)
        {
            Brugere b = FindBruger(brugernavn);
            if (b != null)
            {
                string pw = TilfaeldigStreng(6);
                SendEmail(b.Email, "Nyt password", "Du har fået tilsendt nyt password. Passwordet er: " + pw);
                OpdaterPassword(b.Brugernavn, pw);
                string succesStreng = "E-mail er sendt til " + b.Email;
                TraceHjaelp(new []{brugernavn}, succesStreng);
                return succesStreng;
            }
            string fejlStreng = "Brugeren findes ikke";
            TraceHjaelp(new[] { brugernavn }, fejlStreng);
            return fejlStreng;
        }

        /// <summary>
        /// Genfinder brugernavn ud fra email
        /// </summary>
        /// <param name="email"></param>
        /// <returns>string med resultat</returns>
        public string GlemtBrugernavn(string email)
        {
            if (email != null)
            {
                Brugere b = FindBruger(null, 0, email);
                if (!email.Contains("@"))
                {
                    string fejlStreng = "Email er ikke gyldig (Skal indeholde @)";
                    TraceHjaelp(new[] {email}, fejlStreng);
                    return fejlStreng;
                }
                if (b != null)
                {
                    SendEmail(b.Email, "Brugernavn genfindelse", "Dit brugernavn er: ", b.Brugernavn);
                    string succesStreng = "Brugernavn er sendt til " + email;
                    TraceHjaelp(new[] {email}, succesStreng);
                    return succesStreng;
                }
            }
            string fejlStreng1 = "Email eksisterer ikke i databasen";
            TraceHjaelp(new []{email}, fejlStreng1);
            return fejlStreng1;
        }

        /// <summary>
        /// Opdaterer tidsrummet hvori sensoren er aktiv
        /// </summary>
        /// <param name="fra"></param>
        /// <param name="til"></param>
        /// <returns>string med resultat</returns>
        public string OpdaterTidsrum(string fra, string til)
        {
            if (fra == null)
            {
                string succesStreng = "Fra skal udfyldes";
                TraceHjaelp(new[] { fra, til }, succesStreng);
                return succesStreng;
            }
            if (til == null)
            {
                string succesStreng1 = "Til skal udfyldes";
                TraceHjaelp(new[] { fra, til }, succesStreng1);
                return succesStreng1;
            }
            try
            {
                var f = TimeSpan.Parse(fra);
                var t = TimeSpan.Parse(til);
                using (DataContext dataContext = new DataContext())
                {
                    Tider tid = dataContext.Tider.FirstOrDefault(tider => tider.Id == 1);
                    tid.Fra = f;
                    tid.Til = t;
                    dataContext.Tider.AddOrUpdate(tid);
                    dataContext.SaveChanges();
                    _ta = Task.Run((() => SensorLoop()));
                    string succesStreng = "Tidsrummet blev ændret";
                    TraceHjaelp(new []{fra, til}, succesStreng);
                    return succesStreng;
                }
            }
            catch (Exception ex)
            {
                TraceHjaelp(new[] { fra, til }, ex.Message);
                return "Formatet skal være (HH:MM:SS)";
            }

        }

        /// <summary>
        /// Opdaterer tiden hvori sensoren sover efter måling
        /// </summary>
        /// <param name="minutAntal"></param>
        /// <returns>string med resultat</returns>
        public string OpdaterTidEfterMaaling(int minutAntal)
        {
            int milisekundAntal = minutAntal*60000;
            try
            {
                using (DataContext dataContext = new DataContext())
                {
                    Tider tid = dataContext.Tider.FirstOrDefault(t => t.Id == 1);
                    if (tid != null)
                    {
                        tid.SoveTidEfterMaaling = milisekundAntal;
                        dataContext.Tider.AddOrUpdate(tid);
                        dataContext.SaveChanges();
                        string succesStreng = "Måleren sover nu i " + (minutAntal) + " minutter efter at den har målt";
                        TraceHjaelp(new []{minutAntal.ToString()}, succesStreng);
                        return succesStreng;
                    }
                    string fejlStreng = "Der gik noget galt. Tiden kunne ikke findes";
                    TraceHjaelp(new[] { minutAntal.ToString() }, fejlStreng);
                    return fejlStreng;
                }
            }
            catch (Exception ex)
            {
                TraceHjaelp(new[] { minutAntal.ToString() }, ex.Message);
                return ex.Message;
            }
        }

        /// <summary>
        /// Opdaterer tiden hvori sensoren sover efter alarmering
        /// </summary>
        /// <param name="minutAntal"></param>
        /// <returns>string med resultat</returns>
        public string OpdaterTidEfterAlarmering(int minutAntal)
        {
            int milisekundAntal = minutAntal * 60000;
            try
            {
                using (DataContext dataContext = new DataContext())
                {
                    Tider tid = dataContext.Tider.FirstOrDefault(h => h.Id == 1);
                    if (tid != null)
                    {
                        tid.SoveTidEfterAlarmering = milisekundAntal;
                        dataContext.Tider.AddOrUpdate(tid);
                        dataContext.SaveChanges();
                        string succesStreng = "Alarmen sover nu i " + (minutAntal) + " minutter efter den er gået af";
                        TraceHjaelp(new []{minutAntal.ToString()}, succesStreng);
                        return succesStreng;
                    }
                    string fejlStreng = "Der gik noget galt. Tiden kunne ikke findes";
                    TraceHjaelp(new[] { minutAntal.ToString() }, fejlStreng);
                    return fejlStreng;
                }
            }
            catch (Exception ex)
            {
                TraceHjaelp(new[] { minutAntal.ToString() }, ex.Message);
                return ex.Message;
            }
        }
        /// <summary>
        /// Scraper statistik for data
        /// </summary>
        /// <returns>Liste af statistik-objekter</returns>
        public List<Politistatistik> ScrapeStatistik()
        {
            List<Politistatistik> politistatistik = new List<Politistatistik>();
            int aarsTal = 2007;
            using (IWebDriver webDriver = new PhantomJSDriver())
            {
                webDriver.Navigate().GoToUrl("http://www.politistatistik.dk/parameter.aspx?id=27");

                webDriver.FindElement(By.XPath("//*[@id='geo00']/optgroup/option[8]")).Click();
                webDriver.FindElement(By.XPath("//*[@id='kriminalitet01']/optgroup[2]/option[5]")).Click();
                webDriver.FindElement(By.XPath("//*[@id='rightCloBaggr']/div[5]/div[3]/div[2]/input")).Click();
                foreach (var aar in webDriver.FindElements(By.XPath("//*[@name='periodeYear']")))
                {
                    aar.Click();
                }
                webDriver.FindElement(By.XPath("//*[@id='rightCol']/div[2]/div/div[3]/img")).Click();
                webDriver.SwitchTo().Window(webDriver.WindowHandles.Last());

                foreach (var item in webDriver.FindElements(By.ClassName("dataitem")))
                {
                    politistatistik.Add(new Politistatistik(aarsTal++, item.Text));
                }
            }
            List<Politistatistik> list = politistatistik.ToList();
            TraceHjaelp(new []{""}, list.ToString());
            return list;
        }

        /// <summary>
        /// Finder bruger ud fra skrevne parametre
        /// </summary>
        /// <param name="brugernavn"></param>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <returns>Bruger-objekt</returns>
        public Brugere FindBruger(string brugernavn = null, int id = 0, string email = null)
        {
            using (DataContext dataContext = new DataContext())
            {
                if (email != null)
                {
                    Brugere b = dataContext.Brugere.FirstOrDefault(bruger => bruger.Email == email);
                    if (b != null)
                    {
                        TraceHjaelp(new[] { brugernavn, id.ToString(), email }, b.ToString());
                        return b;
                    }
                    TraceHjaelp(new []{brugernavn, id.ToString(), email}, "Brugeren er Null");
                    return null;
                }
                if (brugernavn != null)
                {
                    Brugere b = dataContext.Brugere.FirstOrDefault(bruger => bruger.Brugernavn == brugernavn);
                    if (b != null)
                    {
                        TraceHjaelp(new[] { brugernavn, id.ToString(), email }, b.ToString());
                        return b;
                    }
                    TraceHjaelp(new[] { brugernavn, id.ToString(), email }, "Brugeren er Null");
                    return null;
                }
                Brugere br = dataContext.Brugere.FirstOrDefault(bruger => bruger.Id == id);
                if (br != null)
                {
                    TraceHjaelp(new[] { brugernavn, id.ToString(), email }, br.ToString());
                    return br;
                }
                TraceHjaelp(new[] { brugernavn, id.ToString(), email }, "Brugeren er Null");
                return null;


            }
        }

        private void SendEmail(string modtager, string emne, string besked, string uniktIndhold = null)
        {
            // Emailoprettelse
            var email = new SendGridMessage { From = new MailAddress("Service@pp.org", "Protect and Prevent") };
            email.AddTo(@"User <" + modtager + ">");
            email.Subject = emne;
            email.Text = besked + "\r\n" + uniktIndhold;

            // Emailforsendelse
            var username = "azure_263e6d64e115fae67eef2e8d59dd4bbd@azure.com";
            var pswd = "WB0uAl6moFYfCtc";
            var credentials = new NetworkCredential(username, pswd);
            var transportWeb = new Web(credentials);
#pragma warning disable 4014
            transportWeb.DeliverAsync(email);
#pragma warning restore 4014
        }

        [SuppressMessage("ReSharper", "FunctionNeverReturns")]
        private void SensorLoop()
        {
            Task.Run((() => AktiverAlarm()));
            using (DataContext dataContext = new DataContext())
            {
                while (true)
                {
                    var tid = (from q in dataContext.Tider where q.Id == 1 select q).SingleOrDefault();
                    if (AktiverSensor(DateTime.Now.TimeOfDay, tid.Fra, tid.Til))
                    {
                        //Random r = new Random();

                        //string testmessage = "RoomSensor Broadcasting\r\n" +
                        //                     "Location: Teachers room\r\n" +
                        //                     "Platform: Linux - 3.12.28 + -armv6l - with - debian - 7.6\r\n" +
                        //                     "Machine: armv6l\r\n" +
                        //                     "Potentiometer(8bit): 134\r\n" +
                        //                     "Light Sensor(8bit): 159\r\n" +
                        //                     "Temperature(8bit): 215\r\n" +
                        //                     "Movement last detected: 2015 - 10 - 29 09:27:" + r.Next(1,99) + ".001053\r\n";
                        //byte[] staticBytes = Encoding.ASCII.GetBytes(testmessage);
                        byte[] bytes = _client.Receive(ref _ipAddress);
                        Task.Run(() => DataBehandling(bytes, ref _senesteDato, ref _senesteTid));

                        var firstOrDefault = dataContext.Tider.FirstOrDefault(t => t.Id == 1);
                        if (firstOrDefault != null && firstOrDefault.SoveTidEfterMaaling > 0)
                            Thread.Sleep(firstOrDefault.SoveTidEfterMaaling);
                        else
                        {
                            Thread.Sleep(60000);
                        }
                    }
                }
            }
        }
        private bool AktiverSensor(TimeSpan serverTid, TimeSpan fra, TimeSpan til)
        {
            if (fra < til)
            {
                return fra <= serverTid && serverTid <= til;
            }
            return !(til < serverTid && serverTid < fra);
        }
        private void AktiverAlarm()
        {
            using (DataContext dataContext = new DataContext())
            {
                while (true)
                {
                    _alarmBool = true;
                    var firstOrDefault = dataContext.Tider.FirstOrDefault(t => t.Id == 1);
                    if (firstOrDefault != null && firstOrDefault.SoveTidEfterAlarmering > 0)
                        Thread.Sleep(firstOrDefault.SoveTidEfterAlarmering);
                    else
                    {
                        Thread.Sleep(3600000);
                    }
                }
            }
        }
        private void DataBehandling(byte[] bytes, ref DateTime senesteDato, ref TimeSpan senesteTid)
        {
            string resp = Encoding.ASCII.GetString(bytes);
            string movementDetected = resp.Split('\r')[7];          // "Movement last detected: 2015 - 10 - 29 09:27:19.001053\r\n";
            string dateTimeString = movementDetected.Split(':')[1]; //  2015-10-29 09
            string[] cutTimeArray = movementDetected.Split(':');    // "[Movement last detected], [2015-10-29 09], [27], [19.001053\r\n]
            string cutTimeSplit = cutTimeArray[3].Split('.')[0];    // 19
            string[] cut3 = dateTimeString.Split(' ');              // [""] [2015-10-29], [09]
            string[] cut4 = cut3[1].Split('-');
            DateTime dt = new DateTime(int.Parse(cut4[0]), int.Parse(cut4[1]), int.Parse(cut4[2]));
            TimeSpan ts = new TimeSpan(int.Parse(cut3[2]), int.Parse(cutTimeArray[2]), int.Parse(cutTimeSplit));
            if (senesteDato != dt || senesteTid != ts)
            {
                VejrService.GlobalWeatherSoapClient client = new VejrService.GlobalWeatherSoapClient();
                var response = client.GetWeather("Roskilde", "Denmark");
                var doc = new XmlDocument();
                doc.LoadXml(response);
                XmlNode root = doc.DocumentElement;
                XmlNode node = root.SelectSingleNode("//Temperature");
                var nodeSplit = node.InnerText.Split('(')[1];
                var nodeSplit2 = nodeSplit.Split(' ')[0];

                using (DataContext dataContext = new DataContext())
                {
                    dataContext.Bevaegelser.Add(new Bevaegelser(dt, ts, decimal.Parse(nodeSplit2)));
                    dataContext.SaveChanges();
                }
                dt = senesteDato;
                ts = senesteTid;
                if (_alarmBool)
                {
                    Alarmer();
                }
            }
        }
        private void Alarmer()
        {
            using (DataContext dataContext = new DataContext())
            {
                foreach (var bruger in dataContext.Brugere)
                {
                    SendEmail(bruger.Email, "Indbrud", "Der er indbrud!");
                }
                _alarmBool = false;
            }
        }
        private string TilfaeldigStreng(int passwordLaengde)
        {
            string smaaB = "abcdefghijkmnopqrstuvwxyz";
            string storeB = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            string tal = "0123456789";
            char[] chars = new char[passwordLaengde];
            char[] chars1 = new char[passwordLaengde];
            char[] chars2 = new char[passwordLaengde];
            Random rd = new Random();

            for (int i = 0; i < passwordLaengde; i++)
            {
                chars[i] = smaaB[rd.Next(0, smaaB.Length)];
            }
            for (int i = 0; i < passwordLaengde; i++)
            {
                chars1[i] = storeB[rd.Next(0, storeB.Length)];
            }
            for (int i = 0; i < passwordLaengde; i++)
            {
                chars2[i] = tal[rd.Next(0, tal.Length)];
            }
            string s1 = new string(chars);
            string s2 = new string(chars1);
            string s3 = new string(chars2);
            return s1 + s2 + s3;
        }
        private string KrypterStreng(string streng)
        {
            string source = streng;
            using (MD5 md5Hash = MD5.Create())
            {
                string hash = Md5Hash(md5Hash, source);
                return "P" + hash;
            }
        }
        private string Md5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        private void TraceHjaelp(string[] parametre, string besked)
        {
            int i = 0;
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            var navn = method.Name;
            string message = "[" + DateTime.Now + "]" + " - " + navn + "(";
            foreach (var s in parametre)
            {
                if (parametre.Length > i + 1)
                {
                    if (s == null)
                    {
                        message += "null, ";
                        i++;
                    }
                    else
                    {
                        message += s + ", ";
                        i++;
                    }
                }
                else
                {
                    if (s == null)
                    {
                        message += "null";
                    }
                    else
                    {
                        message += s;
                    }
                }
            }
            message += "): " + besked;
            Trace.WriteLine(message);
        }
    }
}