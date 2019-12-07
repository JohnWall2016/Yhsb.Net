using System;

namespace CommandLine
{
    public class AppAttribute : Attribute
    {
        public string Name { get; set; }
        public string Version { get; set; }

        /// Copyright string
        /// It's a copyright string or "@assembly" designating
        /// to use the assembly default information.
        public string Copyright { get; set; }
    }
}