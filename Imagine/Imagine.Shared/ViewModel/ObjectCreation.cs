using System;
using System.Collections.Generic;
using System.Text;

namespace Imagine.ViewModel
{
    /// <summary>
    /// Super-class to Quote.css and Wordflow.css
    /// Will be used to create a specified class
    /// </summary>
    public class ObjectCreation
    {
        /// <summary>
        /// Quote's id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Creation's Content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Creation's Author
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Unique identifier with MS account to retrieve data in the database if the user switch device
        /// </summary>
        public string userid { get; set; }

        /// <summary>
        /// Creation's Reference
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// Creation's Tags
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Tells if it will be a quote or a wordflow
        /// </summary>
        public string Type { get; set; }

    }
}
