using Peak.PeakC;
using Peak.PeakC.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Peak.PeakC.Generation.X86_64
{
    static class CodeGeneration
    {

        public static AsmModel GetAsmAssembly(ProgramNode node)
        {
            // TODO: make asm builder
            var table = new GlobalSymbolTable();
            table.MemoryAllocator = new MemoryAllocator(table);
            //generateForProgramNode(node, table);
            generateProgram(node, table);
            table.MainAssembly.Code.Add(table.MethodCode);
            return table.MainAssembly;
        }
        private static void generateProgram(ProgramNode node, GlobalSymbolTable st)
        {
            st.MethodCode.MethodName = "start";
            var rbpSizeOperand = GenMethodPrologueAndGet_rbp(st);

            generateForProgramNode(node, st);

            int frameSize = MemoryAllocator.AlignUpAbsolute(st.MemoryAllocator.GetFrameSize(), 16);
            rbpSizeOperand.content = "sub rsp, " + frameSize.ToString();
        }


        private static void generateForProgramNode(ProgramNode node, GlobalSymbolTable st)
        {
            //st.MethodCode.MethodName = "start";
            //var rbpSizeOperand = GenMethodPrologueAndGet_rbp(st);

            foreach (Node n in node.Node)
            {
                if (n is LoadNode)
                {
                    applyLoadNode((LoadNode)n, st);
                }
                else
                    CodeBlock.Generate((CodeBlockNode)n, st);
            }
            //int frameSize = MemoryAllocator.AlignUpAbsolute(st.MemoryAllocator.GetFrameSize(), 16);
            //rbpSizeOperand.content = "sub rsp, " + frameSize.ToString();
        }

        public static AsmInstruction GenMethodPrologueAndGet_rbp(GlobalSymbolTable st)
        {
            st.MethodCode.Emit("push rbp");
            st.MethodCode.Emit("mov rbp, rsp");
            st.MethodCode.Emit("sub rsp, ...");

            var frameSizeOperand = st.MethodCode.Code[st.MethodCode.Code.Count - 1];

            var rbp = new MemoryIdTracker(st, size: 8);
            var rbpInStack = new MemoryAreaElement(st.MemoryAllocator) { Size = 8, ContainedData = rbp };
            st.MemoryAllocator.StackModel.Add(rbpInStack);
            st.MemoryAllocator.RBP_dataId = rbp;
            return frameSizeOperand;
        }

        private static void applyLoadNode(LoadNode node, GlobalSymbolTable st)
        {
            string fileName = (node as LoadNode).LoadFileName.Content;

            var paths = new string[6]
            {
                Directory.GetCurrentDirectory() + "\\" + fileName,
                Directory.GetCurrentDirectory() + "\\" + fileName + ".p",
                Directory.GetCurrentDirectory() + "\\lib\\" + fileName,
                Directory.GetCurrentDirectory() + "\\lib\\" + fileName + ".p",
                node.MetaInf.File + "\\" + fileName,
                node.MetaInf.File + "\\" + fileName + ".p"
            };
            /*
            
            if (Directory.Exists(Directory.GetCurrentDirectory() + "/" + fileName)
                ||
                Directory.Exists(Directory.GetCurrentDirectory() + "/" + fileName + ".p")
                ||
                Directory.Exists(Directory.GetCurrentDirectory() + "/lib/" + fileName)
                ||
                Directory.Exists(Directory.GetCurrentDirectory() + "/lib/" + fileName + ".p")
                ||
                Directory.Exists(node.MetaInf.File + "/" + fileName)
                ||
                Directory.Exists(node.MetaInf.File + "/" + fileName + ".p")
                )*/

            foreach (string path in paths)
                if (File.Exists(path))
                    if (st.IsNewFile(fileName))
                    {
                        st.RegisterFile(fileName);
                        var p = new Parser.Parser();
                        generateForProgramNode(p.GetNode(path), st);
                        return;
                    }
                    else
                        Error.WarningMessage(node.MetaInf, "file already loadet");

            Error.FileNotFoundErrMessage(node.LoadFileName);

        }



    }
}
