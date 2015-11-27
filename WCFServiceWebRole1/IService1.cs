﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
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
        Brugere OpretBruger(string brugernavn, string password, string email);

        [OperationContract]
        Brugere OpdaterPassword(string brugernavn, string password);

        [OperationContract]
        Brugere OpdaterEmail(string brugernavn, string email);

        [OperationContract]
        string Login(string brugernavn, string password); //string -> Brugere så vi kan gemme email

        [OperationContract]
        List<Bevaegelser> HentBevaegelser();
            
        [OperationContract]
        int HentTemperatur(int startInterval, int slutInterval);

        [OperationContract]
        int HentTidspunkt(int aarstal, int maaned, int slutdag);

        [OperationContract]
        string GlemtPassword(string brugernavn);

        [OperationContract]
        string GlemtBrugernavn(string email);
    }
}