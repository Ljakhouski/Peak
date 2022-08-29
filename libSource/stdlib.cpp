#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include <iostream>
/* console output */

extern "C" __declspec(dllexport) __cdecl void print_i(int i)
{
    std::cout << i;
    //printf ("%d", i);
}

extern "C" __declspec(dllexport) __cdecl void print_d(double d)
{
    std::cout << d;
    //printf ("%s", d);
}

extern "C" __declspec(dllexport) __cdecl void print_s(char* ch)
{
    std::cout << ch;
    //printf (ch);
}

extern "C" __declspec(dllexport) __cdecl void nextLine()
{
    std::cout << '\n';
    //printf ("\n");
}

/* console input */
extern "C" __declspec(dllexport) __cdecl int getInt(int i)
{
    int output;
    scanf ("%d", &output);
    return output;
}

extern "C" __declspec(dllexport) __cdecl double input_d(double d)
{
    double output;
    scanf ("%s", &output);
    return output;
}

extern "C" __declspec(dllexport) __cdecl char* input_s(char* ch)
{
    char* output;
    scanf("%c", &output);
    return output;
}

/* memory managment */

extern "C" __declspec(dllexport) __cdecl void* allocate_area(int size)
{
    return malloc(size);
}
extern "C" __declspec(dllexport) __cdecl void free_area(void* area)
{
    free(area);
}

/* time-module */

extern "C" __declspec(dllexport) __cdecl int getHours ()
{
    time_t my_time;
    struct tm * timeinfo; 
    time (&my_time);
    timeinfo = localtime (&my_time);
    return timeinfo->tm_hour;
}

extern "C" __declspec(dllexport) __cdecl int getMinutes ()
{
    time_t my_time;
    struct tm * timeinfo; 
    time (&my_time);
    timeinfo = localtime (&my_time);
    return timeinfo->tm_min;
}

extern "C" __declspec(dllexport) __cdecl int getSeconds ()
{
    time_t my_time;
    struct tm * timeinfo; 
    time (&my_time);
    timeinfo = localtime (&my_time);
    return timeinfo->tm_sec;
}

float timedifference_msec(struct timeval t0, struct timeval t1)
{
    return (t1.tv_sec - t0.tv_sec) * 1000.0f + (t1.tv_usec - t0.tv_usec) / 1000.0f;
}
extern "C" __declspec(dllexport) __cdecl int getMilliseconds ()
{
    struct timeval t0;
    struct timeval t1;
    int elapsed;
 
   // gettimeofday(&t0, 0);
    /* ... YOUR CODE HERE ... */
   // gettimeofday(&t1, 0);
    elapsed = timedifference_msec(t0, t1);
    return elapsed;
}

extern "C" __declspec(dllexport) __cdecl void sleep(int s)
{
    _sleep(s);
}

extern "C" __declspec(dllexport) __cdecl void clear(int s)
{
    system("cls");
}