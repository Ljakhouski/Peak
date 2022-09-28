color 1B
mkdir bin\Compiler\FASM
robocopy FASM bin\Compiler\FASM /E
call libSource\compileCPP.cmd
copy libSource\stdlib.dll bin\Compiler\FASM
copy libSource\stdlib.dll tests
pause