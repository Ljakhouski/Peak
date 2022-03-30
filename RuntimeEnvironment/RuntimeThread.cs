using RuntimeEnvironment.RuntimeModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace RuntimeEnvironment
{
    class RuntimeThread
    {
        private RuntimeModule.RuntimeModule runtimeModule;

        private int frameStackPointer;
        private int stackPointer;

        private PeakObject[] stack;
        private PeakObject[][] frameStack;

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
                    case InstructionName.Call:
                        Execute(runtimeModule.Methods[currentInstruction.Operands[0]]);
                        break;
                    case InstructionName.CallNative:
                        var name = stack[stackPointer];
                        stackPointer--;


                        break;
                    case InstructionName.Push:
                        stackPointer++;
                        stack[stackPointer] = frameStack[frameStackPointer][currentInstruction.Operands[0]];
                        break;
                    case InstructionName.PushByRef:
                        var reference = stack[instructionPointer];
                        stack[stackPointer] = reference.StructValue[currentInstruction.Operands[0]];
                        break;
                    case InstructionName.PushGlobal:
                        stackPointer++;
                        stack[stackPointer] = frameStack[0][currentInstruction.Operands[0]];
                        break;
                    case InstructionName.Store:
                        frameStack[frameStackPointer][currentInstruction.Operands[0]] = stack[stackPointer];
                        stackPointer--;
                        break;
                    case InstructionName.StoreByRef: // reference on the top
                        stack[stackPointer].StructValue[currentInstruction.Operands[0]] = stack[stackPointer - 1];
                        stackPointer--;
                        break;
                    case InstructionName.StoreGlobal:
                        frameStack[0][currentInstruction.Operands[0]] = stack[stackPointer];
                        stackPointer--;
                        break;
                    case InstructionName.Add:
                    {
                        var v1 = stack[stackPointer];
                        var v2 = stack[stackPointer - 1];
                        stackPointer--;

                        stack[stackPointer] = new PeakObject()
                        {
                            IntValue = v1.IntValue + v2.IntValue,
                            DoubleValue = v1.DoubleValue + v2.DoubleValue
                        };
                        break;
                    }
                    case InstructionName.Sub:
                    {
                        var v1 = stack[stackPointer];
                        var v2 = stack[stackPointer - 1];
                        stackPointer--;

                        stack[stackPointer] = new PeakObject()
                        {
                            IntValue = v1.IntValue - v2.IntValue,
                            DoubleValue = v1.DoubleValue - v2.DoubleValue
                        };
                        break;
                    }
                    case InstructionName.Mul:
                    {
                        var v1 = stack[stackPointer];
                        var v2 = stack[stackPointer - 1];
                        stackPointer--;

                        stack[stackPointer] = new PeakObject()
                        {
                            IntValue = v1.IntValue * v2.IntValue,
                            DoubleValue = v1.DoubleValue * v2.DoubleValue
                        };
                        break;
                    }
                    case InstructionName.Div:
                    {
                        var v1 = stack[stackPointer];
                        var v2 = stack[stackPointer - 1];
                        stackPointer--;

                        stack[stackPointer] = new PeakObject()
                        {
                            IntValue = v1.IntValue / v2.IntValue,
                            DoubleValue = v1.DoubleValue / v2.DoubleValue
                        };
                        break;
                    }
                        
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

            frameStack[frameStackPointer] = new PeakObject[method.LocalVarsArraySize];
        }
        
        public RuntimeThread(RuntimeModule.RuntimeModule module)
        {
            this.runtimeModule = module;
        }
    }
}
