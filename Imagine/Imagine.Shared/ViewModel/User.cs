using System;
using System.Collections.Generic;
using System.Text;

namespace Imagine.ViewModel
{
    public class User
    {
        /// <summary>
        /// Entity's id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User's name (pseudo)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// User's Microsoft ID
        /// </summary>
        public string Msid { get; set; }

        public User(string Id, String Name, String Msid)
        {
            this.Id = Id;
            this.Name = Name;
            this.Msid = Msid;
        }

        public User()
        {

        }
    }
}
