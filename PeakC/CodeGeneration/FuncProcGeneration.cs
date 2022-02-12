using System;
using System.Collections.Generic;
using System.Text;
using Peak.PeakC;
using Peak.PeakC.Parser;
using RuntimeEnvironment;
using RuntimeEnvironment.RuntimeModule;

namespace Peak.CodeGeneration
{
    partial class ByteCodeGenerator
    {
        private GenerationResult generateProcDeclaration(ProcedureNode node, SymbolTable currentSymbolTable)
        {
            throw new Exception();
           // if () // ниче не будет работать, поиск по таблицам неверный его надо переписать ибо один ищет только в своей таблице, а другого не существует для поиска по всей таблицы (от локального до глобального, к примеру для поиска есть ли уже такая переменная)
        }
    }
}
