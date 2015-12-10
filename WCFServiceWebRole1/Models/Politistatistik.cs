using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole1.Models
{
    [DataContract]
    public class Politistatistik
    {
        public Politistatistik(int aar, string indbrud)
        {
            Aar = aar;
            Indbrud = indbrud;
        }
        [DataMember]
        public int Aar { get; set; }
        [DataMember]
        public string Indbrud { get; set; }

        public override string ToString()
        {
            return $"Aar: {Aar}, Indbrud: {Indbrud}";
        }
    }
}