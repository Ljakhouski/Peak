using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    class AsmAssembly
    {
        public List<string> CodeSection { get; set; }
        public List<string> ConstSection{ get; set; }
        public List<string> LibSection { get; set; }

        public string GetAsmSource()
        {
            string source="";

            source.Insert(0, "format PE64 Console \nentry start");

            foreach (string S in CodeSection)
                source.Insert(source.Length, S + "\n");

            source.Insert(source.Length, "section '.rdata' data readable ");

            foreach (string S in ConstSection)
                source.Insert(source.Length, S + "\n");

            source.Insert(source.Length, "section '.idata' data readable import ");

            foreach (string S in LibSection)
                source.Insert(source.Length, S + "\n");

            return source;
        }

        //public string 
    }
}
