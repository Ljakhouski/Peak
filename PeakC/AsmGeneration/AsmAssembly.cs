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

            source.Insert(0, "");
            foreach (string S in CodeSection)
                source.Insert(source.Length, S + "\n");

            foreach (string S in ConstSection)
                source.Insert(source.Length, S + "\n");

            foreach (string S in LibSection)
                source.Insert(source.Length, S + "\n");

            return source;
        }

        //public string 
    }
}
