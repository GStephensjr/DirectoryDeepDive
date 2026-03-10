using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryDeepDive
{
    public class FileCopyData
    {
        public string file { get; set; }
        public string destinationPath { get; set; }

        public FileCopyData(string _file, string _destinationPath)
        {
            file = _file;
            destinationPath = _destinationPath; 
        }
    }
}
