using RuntimeEnvironment.RuntimeModule;
using System;
using System.Collections.Generic;
using System.Text;
using static RuntimeEnvironment.NativeMethods.NativeMethods;

namespace RuntimeEnvironment
{
    public class RuntimeThread
    {
        private RuntimeModule.RuntimeModule runtimeModule;

        private int frameStackPointer = -1;
        private int stackPointer = -1;

        private PeakObject[] stack;
        private PeakObject[] frameStack;

        private PeakObject[] constants;

        private Dictionary<string, NativeMethodDelegate> nativeMethods;
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
                    case InstructionName.IfNot:
                        if (stack[stackPointer].BoolValue == false)
                            instructionPointer = currentInstruction.Operands[0];
                        stackPointer--;
                        break;
                    case InstructionName.Call:
                        Execute(runtimeModule.Methods[currentInstruction.Operands[0]]);
                        break;
                    case InstructionName.CallNative:
                        var name = stack[stackPointer];
                        stackPointer--;
                        var argsCount = stack[stackPointer].IntValue;
                        stackPointer--;
                        var args = new PeakObject[argsCount];
                        for (int i = 0; argsCount > 0; argsCount--, stackPointer--)
                        {
                            args[i] = stack[stackPointer];
                        }
                        nativeMethods[name.StringValue](args, this);
                        break;
                    case InstructionName.PushConst:
                        /*stackPointer++;
                        if (stackPointer > constants.Length)
                        {
                            if (stackPointer > 4096)
                            {
                                Console.WriteLine("STACK OVERFLOW");
                                Console.ReadKey();
                                Environment.Exit(-1);
                            }
                            else
                                Array.Resize(ref stack, stack.Length + 1024);
                        }*/
                        PushOnStack(constants[currentInstruction.Operands[0]]);
                        break;
                    case InstructionName.Push:
                        stackPointer++;
                        stack[stackPointer] = frameStack[frameStackPointer].StructValue[currentInstruction.Operands[0]];
                        break;
                    case InstructionName.PushByRef:
                        var reference = stack[instructionPointer];
                        stack[stackPointer] = reference.StructValue[currentInstruction.Operands[0]];
                        break;
                    case InstructionName.PushGlobal:
                        stackPointer++;
                        stack[stackPointer] = frameStack[0].StructValue[currentInstruction.Operands[0]];
                        break;
                    case InstructionName.Store:
                        frameStack[frameStackPointer].StructValue[currentInstruction.Operands[0]] = stack[stackPointer];
                        stackPointer--;
                        break;
                    case InstructionName.StoreByRef: // reference on the top
                        stack[stackPointer].StructValue[currentInstruction.Operands[0]] = stack[stackPointer - 1];
                        stackPointer--;
                        break;
                    case InstructionName.StoreGlobal:
                        frameStack[0].StructValue[currentInstruction.Operands[0]] = stack[stackPointer];
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
                    case InstructionName.EqualsBool:
                    {
                        var v1 = stack[stackPointer];
                        stackPointer--;
                        var v2 = stack[stackPointer];
                        
                        stack[stackPointer] = v1.BoolValue == v2.BoolValue ? constants[1] : constants[0];
                            
                        break;
                    }
                    case InstructionName.EqualsInt:
                    {
                        var v1 = stack[stackPointer];
                        stackPointer--;
                        var v2 = stack[stackPointer];

                        stack[stackPointer] = v1.IntValue == v2.IntValue ? constants[1] : constants[0];

                        break;
                    }
                    case InstructionName.EqualsDouble:
                    {
                        var v1 = stack[stackPointer];
                        stackPointer--;
                        var v2 = stack[stackPointer];

                        stack[stackPointer] = v1.DoubleValue == v2.DoubleValue ? constants[1] : constants[0];

                        break;
                    }
                    case InstructionName.EqualsString:
                    {
                        var v1 = stack[stackPointer];
                        stackPointer--;
                        var v2 = stack[stackPointer];

                        stack[stackPointer] = v1.StringValue == v2.StringValue ? constants[1] : constants[0];

                        break;
                    }
                    case InstructionName.MoreInt:
                    {
                        var v1 = stack[stackPointer];
                        stackPointer--;
                        var v2 = stack[stackPointer];

                        stack[stackPointer] = v2.IntValue > v1.IntValue ? constants[1] : constants[0];
                        break;
                    }
                    case InstructionName.MoreDouble:
                    {
                        var v1 = stack[stackPointer];
                        stackPointer--;
                        var v2 = stack[stackPointer];

                        stack[stackPointer] = v2.DoubleValue > v1.DoubleValue ? constants[1] : constants[0];
                        break;
                    }
                    default:
                        while (true)
                            Console.WriteLine("unknow operation");
                }
            }
        }

        public void PushOnStack(PeakObject obj)
        {
            stackPointer++;
            if (stackPointer > constants.Length)
            {
                if (stackPointer > 4096)
                {
                    Console.WriteLine("STACK OVERFLOW");
                    Console.ReadKey();
                    Environment.Exit(-1);
                }
                else
                    Array.Resize(ref stack, stack.Length + 1024);
            }
            stack[stackPointer] = obj;
        }

        void makeMethodProlog(MethodDescription method)
        {
            if (this.frameStack.Length <= frameStackPointer)
            {
                Array.Resize(ref frameStack, frameStackPointer + 256);
            }

            frameStackPointer++;

            //frameStack[frameStackPointer].StructValue = new PeakObject[method.LocalVarsArraySize];
            //stack = new PeakObject[1024];
            frameStack[frameStackPointer]= new PeakObject() { StructValue = new PeakObject[method.LocalVarsArraySize] };
        }
        
        public RuntimeThread(RuntimeModule.RuntimeModule module, PeakObject[] constants)
        {
            this.runtimeModule = module;
            this.constants = constants;
            this.stack = new PeakObject[1024];
            this.frameStack = new PeakObject[256];
            this.nativeMethods = NativeMethods.NativeMethods.GetNativeMethods();
        }
    }
}
