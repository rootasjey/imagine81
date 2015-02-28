using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imagine.ViewModel
{
    /// <summary>
    /// Object-class which contains a quote
    /// As the same name of the Azure Table
    /// </summary>
    public class Quote
    {
        /// <summary>
        /// Quote's ID
        /// </summary>
        public string Id { get; set; }

        public DateTime __createdAt { get; set; }

        /// <summary>
        /// The actual quote
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The user who created the quote
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Unique identifier with MS account to retrieve data in the database if the user switch device
        /// </summary>
        public string userid { get; set; }

        /// <summary>
        /// Where is quote comes from
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// Quote's category (ex. love, nature)
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Quote's language
        /// </summary>
        public string Lang { get; set; }
    }
}
