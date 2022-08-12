using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation.X86_64
{
    public enum InstructionName
    {
        mov,
        push,
        pop,

        add,
        sub,
        mul,
        div,

        jmp,
        call,
        ret,
        label,
    }

    public enum DataSize
    {
        None,
        Byte,
        Word,
        Dword,
        QWord,
    }
    public enum RegisterName
    {
        /* x64 */
        rax, rbx, rcx, rdx, rsi, rdi, r8, r9, r10, r11, r12, r13, r14, r15,     rbp, rsp,

        /* x32 */
        eax, ebx, ecx, edx, esi, edi, r8d, r9d, r10d, r11d, r12d, r13d, r14d, r15d,

        /* x16 */
        ax, bx, cx, dx, si, di, r8w, r9w, r10w, r11w, r12w, r13w, r14w, r15w,

        /* x8 */
        al, bl, cl, dl, sil, dil, r8b, r9b, r10b, r11b, r12b, r13b, r14b, r15b,

        /* SSE 128 bit */
        xmm0,
        xmm1,
        xmm2,
        xmm3,
        xmm4,
        xmm5,
        xmm6,
        xmm7,

        NONE,
    }
    class AsmInstruction
    {
        public string content { get; set; }
        public string comment { get; set; }
    }

    static class X86_64_Model
    {
        private static RegisterName[] x64 = new RegisterName[]
        {
            RegisterName.rax,
            RegisterName.rbx,
            RegisterName.rcx,
            RegisterName.rdx,
            RegisterName.rsi,
            RegisterName.rdi,
            RegisterName.r8,
            RegisterName.r9,
            RegisterName.r10,
            RegisterName.r11,
            RegisterName.r12,
            RegisterName.r13,
            RegisterName.r14,
            RegisterName.r15,
        };

        private static RegisterName[] x32 = new RegisterName[]
        {
            RegisterName.eax,
            RegisterName.ebx,
            RegisterName.ecx,
            RegisterName.edx,
            RegisterName.esi,
            RegisterName.edi,
            RegisterName.r8d,
            RegisterName.r9d,
            RegisterName.r10d,
            RegisterName.r11d,
            RegisterName.r12d,
            RegisterName.r13d,
            RegisterName.r14d,
            RegisterName.r15d,
        };

        private static RegisterName[] x16 = new RegisterName[]
        {
            RegisterName.ax,
            RegisterName.bx,
            RegisterName.cx,
            RegisterName.dx,
            RegisterName.si,
            RegisterName.di,
            RegisterName.r8w,
            RegisterName.r9w,
            RegisterName.r10w,
            RegisterName.r11w,
            RegisterName.r12w,
            RegisterName.r13w,
            RegisterName.r14w,
            RegisterName.r15w,
        };

        private static RegisterName[] x8 = new RegisterName[]
        {
            RegisterName.al,
            RegisterName.bl,
            RegisterName.cl,
            RegisterName.dl,
            RegisterName.sil,
            RegisterName.dil,
            RegisterName.r8b,
            RegisterName.r9b,
            RegisterName.r10b,
            RegisterName.r11b,
            RegisterName.r12b,
            RegisterName.r13b,
            RegisterName.r14b,
            RegisterName.r15b,
        };

        private static RegisterName[] x128SSE = new RegisterName[]
        {
            RegisterName.xmm0,
            RegisterName.xmm1,
            RegisterName.xmm2,
            RegisterName.xmm3,
            RegisterName.xmm4,
            RegisterName.xmm5,
            RegisterName.xmm6,
            RegisterName.xmm7
        };
        public static DataSize GetSizeName(int bytes)
        {
            switch (bytes)
            {
                case 1:
                    return DataSize.Byte;
                case 2:
                    return DataSize.Word;
                case 4:
                    return DataSize.Dword;
                case 8:
                    return DataSize.QWord;
                default:
                    throw new CompileException();
            }
        }

        public static DataSize GetSizeName(Type type)
        {
            switch (type)
            {
                case Type.Bool:
                    return DataSize.Byte;
                case Type.Int:
                    return DataSize.Dword;
                case Type.Double:
                case Type.Str:
                case Type.Method:
                    //case Type.:
                    return DataSize.QWord;
                default:
                    throw new CompileException();
            }
        }

        public static int GetSize(Type type)
        {
            switch (type)
            {
                case Type.Bool:
                    return 1;
                case Type.Int:
                    return 4;
                case Type.Double:
                case Type.Str:
                case Type.Method:
                    //case Type.:
                    return 8;
                default:
                    throw new CompileException();
            }
        }
        public static RegisterName CastRegister(RegisterName name, int size)
        {
            if (IsSSE(name))
            {
                throw new CompileException();
            }
            else
            {
                var currentSize = getRegSize(name);
                var indexX64 = getIndex(name, getArr(currentSize));
                return getArr(size)[indexX64];
            }
        }
        public static bool Equals(RegisterName r1, RegisterName r2)
        {
            if (bothSSE())
            {
                var r1Size = getRegSize(r1);
                var r1Arr = getSSEArr(r1Size);
                var r1Index = getIndex(r1, r1Arr);

                var r2Size = getRegSize(r2);
                var r2Arr = getSSEArr(r2Size);
                var r2Index = getIndex(r2, r2Arr);

                return r1Index == r2Index;
            }
            else if (bothNotSSE())
            {
                var r1Size = getRegSize(r1);
                var r1Arr = getArr(r1Size);
                var r1Index = getIndex(r1, r1Arr);

                var r2Size = getRegSize(r2);
                var r2Arr = getArr(r2Size);
                var r2Index = getIndex(r2, r2Arr);

                return r1Index == r2Index;
            }
            else
                return false;

            bool bothSSE()
            {
                if (IsSSE(r1) && (IsSSE(r2)))
                    return true;
                return false;
            }

            bool bothNotSSE()
            {
                if (IsSSE(r1) == false && (IsSSE(r2)) == false)
                    return true;
                return false;
            }
        }

        private static int getIndex(RegisterName r, RegisterName[] xArr)
        {
            for (int i = 0; i < xArr.Length; i++)
                if (xArr[i] == r)
                    return i;
            throw new CompileException();
        }

        private static int getRegSize(RegisterName name)
        {
            foreach (var e in x64)
                if (e == name)
                    return 8;
            foreach (var e in x32)
                if (e == name)
                    return 4;
            foreach (var e in x16)
                if (e == name)
                    return 2;
            foreach (var e in x8)
                if (e == name)
                    return 1;
            foreach (var e in x128SSE)
                if (e == name)
                    return 8;
            /*foreach (var e in x128FFE)
                if (e == name)
                    return 4;*/
            throw new CompileException();
            /*
            foreach (var e in sse)
                if (e == name)
                    return 16;*/
        }
        private static RegisterName[] getArr(int regSize)
        {
            switch (regSize)
            {
                case 1:
                    return x8;
                case 2:
                    return x16;
                case 4:
                    return x32;
                case 8:
                    return x64;
                default:
                    throw new CompileException();
            }
        }

        private static RegisterName[] getSSEArr(int regSize)
        {
            switch (regSize)
            {
                case 8:
                    return x128SSE;
                default:
                    throw new CompileException();
            }
        }
        public static bool IsSSE(RegisterName r1)
        {
            foreach(var e in x128SSE)
                if (e == r1)
                    return true;
            return false;
        }
    }
    class AsmMethod
    {
        public string MethodName { get; set; }
        public List<AsmInstruction> Code { get; set; } = new List<AsmInstruction>();

        public void Emit(string instruction)
        {
            this.Code.Add(new AsmInstruction() { content = instruction });
        }

        public void Emit(string instruction, string comment)
        {
            this.Code.Add(new AsmInstruction() { content = instruction, comment = comment });
        }
    }
    class AsmModel
    {
        public const int ByteInWord = 8; // for x64 mode 
        public List<string> Head { get; set; } = new List<string>();
        public List<string> RData { get; set; } = new List<string>();
        public List<string> IData { get; set; } = new List<string>();
        public List<AsmMethod> Code { get; set; } = new List<AsmMethod>();


        private List<DllImportSymbol> dllImportSymbols = new List<DllImportSymbol>();
        class DllImportSymbol
        {
            public string DllPath { get; set; }
            public List<string> Symbols { get; set; }
        }
        public string GetFasmListing()
        {
            string output = "format PE64 Console \n entry start  \n";
            output += " include 'win64a.inc'";
            foreach (string heads in Head)
                output += heads + '\n';

            output += "\nsection '.text' code readable executable  \n";

            // code adding

            foreach (AsmMethod method in Code)
            {
                output += "\n";
                output += method.MethodName + ':' + '\n';

                foreach (AsmInstruction instruction in method.Code)
                {
                    string line = getInstructionListing(instruction) + '\n';
                    output += line;
                }
            }

            if (RData.Count > 0)
            {
                output += "\nsection '.rdata' data readable \n";

                foreach (string S in RData)
                    output += S;

            }
            /*
            if (IData.Count > 0)
            {
                output += "\nsection '.idata' data readable import \n";
                foreach (string S in IData)
                    output += S;
            }
            */

            output += "\n\n section '.idata' data readable import\n";
            output += getIdataText();


            return output;
        }

        public void AddDllImportSymbol(string dllPath, string name)
        {
            foreach (var e in dllImportSymbols)
            {
                if (e.DllPath == dllPath)
                {
                    e.Symbols.Add(name);
                    return;
                }
            }

            dllImportSymbols.Add(new DllImportSymbol()
            {
                DllPath = dllPath,
                Symbols = new List<string>() { name }
            });
        }
        private string getIdataText()
        {
            string libOutput = "library ";
            string importStr = "";
            foreach (var e in dllImportSymbols)
            {
                string dllLabel = remove(e.DllPath, '.');

                libOutput += dllLabel;
                libOutput += @$", '{e.DllPath}',\";

                importStr += $"import {dllLabel}";
                foreach (var i in e.Symbols)
                {
                    importStr += $", {i}, \'{i}\'";
                }
            }

            libOutput = trimEnd(libOutput, ",\\");

            return libOutput + "\n" + importStr;
        }

        private string trimEnd(string s, string trimS)
        {
            char[] charArray = trimS.ToCharArray();
            Array.Reverse(charArray);
            trimS = new string(charArray);

            foreach (char ch in trimS)
            {
                if (s[s.Length - 1] == ch)
                    s = s.Remove(s.Length - 1);
                else
                    break;
            }

            return s;
        }

        private string remove(string s, char ch)
        {
            for (int i = 0; i < s.Length; i++)
                if (s[i] == ch)
                {
                    s = s.Remove(i, 1);
                    i--;
                }

            return s;
        }
        private string getInstructionListing(AsmInstruction instruction)
        {
            string output = "    ";
            output += instruction.content;
            InsertPlusSymbolToOffset(ref output);

            if (instruction.comment != null && instruction.comment.Length != 0)
                output += "    ;" + instruction.comment;
            return output;
        }


        // mov r?x, [r?x 64]   ->   mov r?x, [r?x + 64]
        private void InsertPlusSymbolToOffset(ref string input)
        {
            bool isInSqureParents = false;

            for (int i = 0; i < input.Length - 1; i++)
            {
                if (input[i] == '[')
                    isInSqureParents = true;
                if (input[i] == ']')
                    isInSqureParents = false;

                if (isInSqureParents && input[i] == '-')
                    return;

                if (isInSqureParents && IsNumber(input[i]))
                    input.Insert(i, "-");
            }
            return;

            bool IsNumber(char ch)
            {
                var numbers = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

                foreach (char ch2 in numbers)
                    if (ch2 == ch)
                        return true;
                return false;
            }
        }
    }
}
