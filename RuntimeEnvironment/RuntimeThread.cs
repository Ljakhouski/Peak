using RuntimeEnvironment.RuntimeModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace RuntimeEnvironment
{
    struct Frame
    {
        public PeakObject[] localData;
    }
    class RuntimeThread
    {
        private int frameStackPointer;
        private int stackPointer;

        private PeakObject[] stack;
        private Frame[] frameStack;

        public void Execute(MethodDescription method)
        {
            makeMethodProlog(method);
            int instructionPointer = 0;
            Instruction currentInstruction;

            while (true)
            {
                currentInstruction = method.Code[instructionPointer];
                instructionPointer++; // set next instruction BEFORE execute. Made to handle "jump"-command 

                switch(currentInstruction.Name)
                {
                    case InstructionName.Return:
                        frameStackPointer--;
                        return;
                    case InstructionName.Jump:
                        instructionPointer = currentInstruction.Operands[0];
                        break;

                }
            }
        }

        void makeMethodProlog(MethodDescription method)
        {
            if (this.frameStack.Length <= frameStackPointer)
            {
                Array.Resize(ref frameStack, frameStackPointer + 256);
            }

            frameStackPointer++;

            frameStack[frameStackPointer] = new Frame()
            {
                localData = new PeakObject[method.LocalVarsArraySize]
            };
        }
    }
}
