using System;
using System.Collections.Generic;
using System.Text;

namespace PDE
{
    internal static class FileNames
    {
        public static string GetFileName(string fileName)
        {
            string S = "";

            for (int i = fileName.Length - 1; i > 0; i--)
            {
                if (fileName[i] != '\\' && fileName[i] != '/')
                    S = fileName[i] + S;
                else return S;
            }
            return S;
        }

        public static string GetFilePath(string fileName)
        {
            string S = "";

            for (int i = fileName.Length - 1; i > 0; i--)
            {
                if (fileName[i] == '\\' || fileName[i] == '/')
                    return fileName.Substring(0, i + 1);
            }
            return S;
        }
        public static string GetWithoutExtention(string fileName)
        {
            for (int i = fileName.Length - 1; i > 0; i--)
            {
                if (fileName[i] == '.')
                {
                    return fileName.Substring(0, i);
                }
            }
            return fileName;
            //throw new Exception();
        }
    }
}
