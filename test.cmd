@echo off
:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: This simple batch script helps you to quickly compile and run a single-file
:: OpenGL/GLFW program on Windows.
::
:: Usage:    .\test {filename}
:: Example:  .\test demo0.cpp
::
:: Happy hacking! - eric
:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

:: display a message if no filename was given
if [%1] equ [] (
    echo Usage:    .\test {filename}
    echo Example:  .\test demo0.cpp
    goto :eof
)

:: show the user what the compile command will look like
echo g++ %1 src\glad.cpp -std=c++17 -Wall -lglfw3 -lgdi32 -Iinclude -Llib-mingw-w64 -o %~n1.exe

:: actually compile the program && run it if compilation succeeds
g++ %1 src\glad.cpp -std=c++17 -Wall -lglfw3 -lgdi32 -Iinclude -Llib-mingw-w64 -o %~n1.exe && %~n1.exe
