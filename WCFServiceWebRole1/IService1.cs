﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using WCFServiceWebRole1.Models;

namespace WCFServiceWebRole1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        Bevaegelser SletHistorik(int id);

        [OperationContract]
        string OpretBruger(string brugernavn, string password, string email);

        [OperationContract]
        string OpdaterPassword(string brugernavn, string password);

        [OperationContract]
        string OpdaterEmail(string brugernavn, string email);

        [OperationContract]
        string Login(string brugernavn, string password);

        [OperationContract]
        List<Bevaegelser> HentBevaegelser(string kolonne, string faldendeEllerStigende);
            
        [OperationContract]
        int HentTemperatur(int startInterval, int slutInterval);

        [OperationContract]
        int HentTidspunkt(int aarstal, int maaned, int slutdag);

        [OperationContract]
        string GlemtPassword(string brugernavn);

        [OperationContract]
        string GlemtBrugernavn(string email);

        [OperationContract]
        string OpdaterTidsrum(string fra, string til);

        [OperationContract]
        Brugere FindBruger(string brugernavn = null, int id = 0, string email = null);

        [OperationContract]
        string OpdaterTidEfterMaaling(int minutAntal);

        [OperationContract]
        string OpdaterTidEfterAlarmering(int minutAntal);

        [OperationContract]
        List<Politistatistik> ScrapeStatistik();

    }
}