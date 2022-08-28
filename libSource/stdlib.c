#include <stdio.h>
#include <stdlib.h>
#include <time.h>
/* console output */

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

__declspec(dllexport) __cdecl void nextLine()
{
    printf ("\n");
}

/* console input */
__declspec(dllexport) __cdecl int getInt(int i)
{
    int output;
    scanf ("%d", &output);
    return output;
}

__declspec(dllexport) __cdecl double input_d(double d)
{
    double output;
    scanf ("%s", &output);
    return output;
}

__declspec(dllexport) __cdecl char* input_s(char* ch)
{
    char* output;
    scanf("%c", &output);
    return output;
}

/* memory managment */

__declspec(dllexport) __cdecl void* allocate_area(int size)
{
    return malloc(size);
}
__declspec(dllexport) __cdecl void free_area(void* area)
{
    free(area);
}

/* time-module */

__declspec(dllexport) __cdecl int getHours ()
{
    time_t my_time;
    struct tm * timeinfo; 
    time (&my_time);
    timeinfo = localtime (&my_time);
    return timeinfo->tm_hour;
}

__declspec(dllexport) __cdecl int getMinutes ()
{
    time_t my_time;
    struct tm * timeinfo; 
    time (&my_time);
    timeinfo = localtime (&my_time);
    return timeinfo->tm_min;
}

__declspec(dllexport) __cdecl int getSeconds ()
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
__declspec(dllexport) __cdecl int getMilliseconds ()
{
    struct timeval t0;
    struct timeval t1;
    int elapsed;
 
    gettimeofday(&t0, 0);
    /* ... YOUR CODE HERE ... */
    gettimeofday(&t1, 0);
    elapsed = timedifference_msec(t0, t1);
    return elapsed;
}

__declspec(dllexport) __cdecl void sleep(int s)
{
    _sleep(s);
}

__declspec(dllexport) __cdecl void clear(int s)
{
    system("cls");
}