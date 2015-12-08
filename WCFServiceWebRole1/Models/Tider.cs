using System.Runtime.Serialization;
using System.Web.UI.WebControls;

namespace WCFServiceWebRole1.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Tider")]
    public partial class Tider
    {
        private TimeSpan _fra;
        private TimeSpan _til;
        private int _soveTidEfterMaaling;
        private int _soveTidEfterAlarmering;

        [DataMember]
        public int Id { get; set; }

        public void CheckFra(string fra)
        {
            TimeSpan f = TimeSpan.Parse(fra);

            if (f >= new TimeSpan(23, 59, 59) || f == new TimeSpan(-1, 0, 0))
            {
                throw new ArgumentException("Tallet er forkert");
            }

        }

        public void Checktil(string til)
        {
            TimeSpan t = TimeSpan.Parse(til);
            if (t >= new TimeSpan(23, 59, 59) || t == new TimeSpan(-1, 0, 0))
            {
                throw new ArgumentException("Tallet er forkert");
            }

        }

        public void CheckSoveTidEfterMaaling(int antalMilisekunder)
        {
            if (antalMilisekunder <= 0)
            {
                throw new ArgumentException("Værdien skal være større end 0");
            }
            if (antalMilisekunder > 86400000) // 86400000 = 24 timer i milisekunder
            {
                throw new ArgumentException("Værdien må ikke være større end 24 timer (1440 minutter)");
            }
        }

        public void CheckSoveTidEfterAlarmering(int antalMilisekunder)
        {
            if (antalMilisekunder <= 0)
            {
                throw new ArgumentException("Værdien skal være større end 0");
            }
            if (antalMilisekunder > 86400000) // 86400000 = 24 timer i milisekunder
            {
                throw new ArgumentException("Værdien må ikke være større end 24 timer (1440 minutter)");
            }
        }
        [DataMember]
        public TimeSpan Fra
        {
            get { return _fra; }
            set
            {
                CheckFra(value.ToString());
                _fra = value;

            }
        }
        [DataMember]
        public TimeSpan Til
        {
            get { return _til; }
            set
            {
                Checktil(value.ToString());
                _til = value;
            }
        }

        [DataMember]
        public int SoveTidEfterMaaling
        {
            get { return _soveTidEfterMaaling; }
            set
            {
                CheckSoveTidEfterMaaling(value);
                _soveTidEfterMaaling = value;
            }
        }

        [DataMember]
        public int SoveTidEfterAlarmering
        {
            get { return _soveTidEfterAlarmering; }
            set
            {
                CheckSoveTidEfterAlarmering(value);
                _soveTidEfterAlarmering = value;
            }
        }
    }
}
