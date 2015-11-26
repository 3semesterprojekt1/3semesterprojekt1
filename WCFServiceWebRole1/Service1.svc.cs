﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Web;
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

        public Service1()
        {
            if (_client == null)
            {
                _client = new UdpClient(Port) {EnableBroadcast = true};
            }
            if (_ipAddress == null)
            {
                _ipAddress = new IPEndPoint(IPAddress.Any, Port);
            }

            if (_ta == null)
            {
                _ta = Task.Run((() => TempLoop()));
            }
        }

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

        public Brugere OpretBruger(string brugernavn, string password, string email)
        {
            using (DataContext dataContext = new DataContext())
            {
                Brugere exBruger = FindBruger(brugernavn);

                if (exBruger == null)
                {
                    Brugere b = new Brugere() {Brugernavn = brugernavn, Password = password, Email = email};
                    dataContext.Brugere.Add(b);
                    dataContext.SaveChanges();
                    return b;
                }
                return null;
            }
        }

        public Brugere OpdaterPassword(string brugernavn, string password)
        {
            using (DataContext dataContext = new DataContext())
            {
                Brugere b = FindBruger(brugernavn);
                if (b != null)
                {
                    b.Password = password;
                    dataContext.Brugere.AddOrUpdate(b);
                    dataContext.SaveChanges();
                    return b;
                }
                return null;
            }
        }

        public Brugere OpdaterEmail(string brugernavn, string email)
        {
            using (DataContext dataContext = new DataContext())
            {
                Brugere b = FindBruger(brugernavn);
                if (b != null)
                {
                    b.Email = email;
                    dataContext.Brugere.AddOrUpdate(b);
                    dataContext.SaveChanges();
                    return b;
                }
                return null;
            }
        }

        public string Login(string brugernavn, string password)
        {
            throw new NotImplementedException();
        }

        public List<Bevaegelser> HentBevægelser()
        {
            throw new NotImplementedException();
        }

        public int HentTemperatur(int startInterval, int slutInterval)
        {
            throw new NotImplementedException();
        }

        public int HentTidspunkt(int aarstal, int maaned, int slutdag)
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

        public string GlemtPassword(string brugernavn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Genfinder brugernavn ud fra email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
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
        /// Sender email med de pågældende parametre
        /// </summary>
        /// <param name="modtager"></param> 
        /// <param name="emne"></param>
        /// <param name="besked"></param>
        /// <param name="uniktIndhold">Så som nyt password eller lign.</param>
        private void SendEmail(string modtager, string emne, string besked, string uniktIndhold)
        {
            // Emailoprettelse
            var email = new SendGridMessage();
            email.From = new MailAddress("Service@pp.org", "Protect and Prevent");
            email.AddTo(@"User <" + modtager + ">");
            email.Subject = emne;
            email.Text = besked + "\r\n" + uniktIndhold;

            // Emailforsendelse
            var username = "azure_263e6d64e115fae67eef2e8d59dd4bbd@azure.com";
            var pswd = "WB0uAl6moFYfCtc";
            var credentials = new NetworkCredential(username, pswd);
            var transportWeb = new Web(credentials);
            transportWeb.DeliverAsync(email);
        }

        private Brugere FindBruger(string brugernavn = null, int id = 0, string email = null)
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

        private void TempLoop()
        {
            while (true)
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
                byte[] bytes = _client.Receive(ref _ipAddress);
                Task.Run(() => DoIt(bytes, ref _senesteDato, ref _senesteTid));
                //Thread.Sleep(1000);
            }
        }

        private static void DoIt(byte[] bytes, ref DateTime senesteDato, ref TimeSpan senesteTid)
        {
            string resp = Encoding.ASCII.GetString(bytes);
            string movementDetected = resp.Split('\r')[7]; // "Movement last detected: 2015 - 10 - 29 09:27:19.001053\r\n";
            string dateTimeString = movementDetected.Split(':')[1]; //  2015-10-29 09
            string[] cutTimeArray = movementDetected.Split(':'); // "[Movement last detected], [2015-10-29 09], [27], [19.001053\r\n]
            string cutTimeSplit = cutTimeArray[3].Split('.')[0]; // 19
            string[] cut3 = dateTimeString.Split(' '); // [""] [2015-10-29], [09]
            string[] cut4 = cut3[1].Split('-');
            string test = cut4[0];
            string test2 = cut4[1];
            DateTime dt = new DateTime(int.Parse(cut4[0]), int.Parse(cut4[1]), int.Parse(cut4[2]));
            //DateTime ddt = new DateTime(cut3[1], cut3[3], cut3[5]);
            TimeSpan ts = new TimeSpan(int.Parse(cut3[2]), int.Parse(cutTimeArray[2]), int.Parse(cutTimeSplit));
            if (senesteDato != dt || senesteTid != ts)
            {
                VejrService.GlobalWeatherSoapClient client = new VejrService.GlobalWeatherSoapClient();
                var response = client.GetWeather("Roskilde", "Denmark");
                XmlDocument doc = new XmlDocument();
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
            }
        }
    }
}
