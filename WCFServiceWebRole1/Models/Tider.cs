using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Web.WebSockets;

namespace WCFServiceWebRole1.Models
{
    [DataContract]
    [Table("Tider")]
    public class Tider
    {
        private TimeSpan _fra;
        private TimeSpan _til;

        [DataMember]
        public int Id { get; set; }

        public void CheckFra(string fra)
        {
            TimeSpan f = TimeSpan.Parse(fra);

            if (f == new TimeSpan(24, 00, 00))
            {
                throw new ArgumentException(" udfyld en af felterne ");
            }
            if (f == new TimeSpan(-1, 0, 0))
            {
                throw new ArgumentException("tallet m� ikke v�re minus ");
            }
        }

        public void Checktil(string til)
        {
            TimeSpan t = TimeSpan.Parse(til);
            if (t == new TimeSpan(24, 00, 00))
            {
                throw new ArgumentException(" udfyld en af felterne ");
            }
            if (t == new TimeSpan(-1, 0, 0))
            {
                throw new ArgumentException("tallet m� ikke v�re minus ");
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
    }
}