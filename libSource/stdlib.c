#include <stdio.h>
#include <stdlib.h>


__declspec(dllexport) __cdecl void print_i(int i)
{
    printf ("%d", i);
}

__declspec(dllexport) __cdecl void print_d(double d)
{
    printf ("%s", d);
}

__declspec(dllexport) __cdecl void print_s(char* ch)
{
    printf (ch);
}



__declspec(dllexport) __cdecl int input_i(int i)
{
    scanf ("%d");
}

__declspec(dllexport) __cdecl double input_d(double d)
{
    scanf ("%s");
}

__declspec(dllexport) __cdecl char* input_s(char* ch)
{
    scanf("%c");
}



__declspec(dllexport) __cdecl void* allocate_area(int size)
{
    return malloc(size);
}
__declspec(dllexport) __cdecl void* free_area(void* area)
{
    free(area);
}
