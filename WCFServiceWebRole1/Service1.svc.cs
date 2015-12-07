using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using SendGrid;
using WCFServiceWebRole1.Models;

namespace WCFServiceWebRole1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF GetCount Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        private const int Port = 7000;
        private static UdpClient _client;
        private static IPEndPoint _ipAddress;
        private static DateTime _senesteDato;
        private static TimeSpan _senesteTid;
        private static Task _ta;
        private static bool _alarmBool;

        public Service1()
        {
            if (_client == null)
            {
                _client = new UdpClient(Port) { EnableBroadcast = true };
            }
            if (_ipAddress == null)
            {
                _ipAddress = new IPEndPoint(IPAddress.Any, Port);
            }
            if (_ta == null)
            {
                _ta = Task.Run((() => SensorLoop()));
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
                    return b;
                }
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
                    try
                    {
                        Brugere b = new Brugere() { Brugernavn = brugernavn, Password = KrypterStreng(password), Email = email };
                        dataContext.Brugere.Add(b);
                        dataContext.SaveChanges();
                        return brugernavn + " er oprettet i databasen";
                    }
                    catch (ArgumentException ex)
                    {
                        return ex.Message;
                    }
                }
                return "Brugernavnet findes allerede i databasen";
            }
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
                        b.Password = KrypterStreng(password);
                        dataContext.Brugere.AddOrUpdate(b);
                        dataContext.SaveChanges();
                        return "Password er ændret";
                    }
                    catch (ArgumentException ex)
                    {

                        return ex.Message;
                    }
                }
                return "Der gik noget galt med at finde din bruger. Prøv igen";
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
                        return "Email er ændret";
                    }
                    catch (ArgumentException ex)
                    {
                        return ex.Message;
                    }
                }
                return "Der gik noget galt med at finde din bruger. Prøv igen";
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

            if (b1 != null)
            {
                if (b1.Password == KrypterStreng(password))
                {
                    return b1.Brugernavn;
                }
            }
            return "Brugernavnet/passwordet er forkert";
        }

        /// <summary>
        /// Henter alle bevaegelser
        /// </summary>
        /// <param name="kolonne">Sortere på den valgte kolonne</param>
        /// <param name="faldendeEllerStigende"></param>
        /// <returns>En liste med alle bevægelser</returns>
        public List<Bevaegelser> HentBevaegelser(string kolonne, string faldendeEllerStigende)
        {
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
                                return query.ToList();
                            }
                            case "faldende":
                            {
                                var query = from q in dataContext.Bevaegelser orderby q.Tidspunkt descending select q;
                                return query.ToList();
                            }
                            default:
                                var queryDefault = from q in dataContext.Bevaegelser orderby q.Dato descending orderby q.Tidspunkt descending select q;
                                return queryDefault.ToList();
                        }
                    case "Dato":
                        switch (faldendeEllerStigende)
                        {
                            case "stigende":
                            {
                                var query = from q in dataContext.Bevaegelser orderby q.Dato ascending select q;
                                return query.ToList();
                            }
                            case "faldende":
                            {
                                var query = from q in dataContext.Bevaegelser orderby q.Dato descending select q;
                                return query.ToList();
                            }
                            default:
                                var queryDefault = from q in dataContext.Bevaegelser orderby q.Dato descending orderby q.Dato descending select q;
                                return queryDefault.ToList();
                        }
                    case "Temperatur":
                        switch (faldendeEllerStigende)
                        {
                            case "stigende":
                            {
                                var query = from q in dataContext.Bevaegelser orderby q.Temperatur ascending select q;
                                return query.ToList();
                            }
                            case "faldende":
                            {
                                var query = from q in dataContext.Bevaegelser orderby q.Temperatur descending select q;
                                return query.ToList();
                            }
                            default:
                                var queryDefault = from q in dataContext.Bevaegelser orderby q.Dato descending orderby q.Temperatur descending select q;
                                return queryDefault.ToList();
                        }
                    default:
                        var queryDefault1 = from q in dataContext.Bevaegelser orderby q.Dato descending orderby q.Tidspunkt descending select q;
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
            Thread.Sleep(5000);
            using (DataContext datacontext = new DataContext())
            {
                var query = from p in datacontext.Bevaegelser
                            where p.Temperatur >= startInterval && p.Temperatur <= slutInterval
                            select p;
                return query.Count();
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
                        return query.Count();
                    }
                }
                return 0;
            }
            catch (Exception)
            {
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
                return "E-mail er sendt til " + b.Email;
            }
            return "E-mailen findes ikke";
        }

        /// <summary>
        /// Genfinder brugernavn ud fra email
        /// </summary>
        /// <param name="email"></param>
        /// <returns>string med resultat</returns>
        public string GlemtBrugernavn(string email)
        {
            Brugere b = FindBruger(null, 0, email);
            if (!email.Contains("@"))
            {
                return "Email er ikke gyldig (Skal indeholde @)";
            }
            if (b != null)
            {
                SendEmail(b.Email, "Brugernavn genfindelse", "Dit brugernavn er: ", b.Brugernavn);
                return "Brugernavn er sendt til " + email;
            }
            return "Email eksisterer ikke i databasen";
        }

        /// <summary>
        /// Opdaterer tidsrummet hvori sensoren er aktiv
        /// </summary>
        /// <param name="fra"></param>
        /// <param name="til"></param>
        /// <returns>string med resultat</returns>
        public string OpdaterTidsrum(string fra, string til)
        {
            try
            {
                var f = TimeSpan.Parse(fra);
                var t = TimeSpan.Parse(til);

                int id = 1;
                Tider tid = new Tider() { Fra = f, Id = id, Til = t };
                using (DataContext client = new DataContext())
                {
                    client.Tider.AddOrUpdate(tid);
                    client.SaveChanges();
                    _ta = Task.Run((() => SensorLoop()));
                    return "Tidsrummet blev ændret";
                }
            }
            catch (Exception)
            {
                return "Tallet er forkert";
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
        public Brugere FindBruger(string brugernavn = null, int id = 0, string email = null)
        {
            using (DataContext dataContext = new DataContext())
            {
                if (email != null)
                {
                    Brugere b = dataContext.Brugere.FirstOrDefault(bruger => bruger.Email == email);
                    return b;
                }
                if (brugernavn != null)
                {
                    Brugere b = dataContext.Brugere.FirstOrDefault(bruger => bruger.Brugernavn == brugernavn);
                    return b;
                }
                Brugere br = dataContext.Brugere.FirstOrDefault(bruger => bruger.Id == id);
                return br;
            }
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
                        Thread.Sleep(60000);
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
            while (true)
            {
                _alarmBool = true;
                Thread.Sleep(3600000);
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
    }
}