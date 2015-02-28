using System;
using System.Collections.Generic;
using System.Text;

namespace Imagine.ViewModel
{
    /// <summary>
    /// Object which represents a image background entry into the SQL database
    /// </summary>
    public class Dailybackground
    {
        /// <summary>
        /// Image's id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Image's created date
        /// </summary>
        public DateTime __createdAt { get; set; }

        /// <summary>
        /// Image's updated date
        /// </summary>
        public DateTime __updatedAt { get; set; }

        /// <summary>
        /// Image's url (http://...)
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// A tag description of the background
        /// </summary>
        public string tag { get; set; }

        /// <summary>
        /// Tells which day the background must be applied
        /// </summary>
        public DateTime day { get; set; }
    }
}
