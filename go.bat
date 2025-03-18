@echo off
SETLOCAL EnableDelayedExpansion

REM we want jruby-complete to take care of all things ruby
SET GEM_HOME=
SET GEM_PATH=

REM The first argument is always the Rake task name
SET task=%1

REM Check for arguments
IF "%task%"=="" (
  echo No task specified
  exit /b 1
)

REM Shift the task off and get the remaining arguments
SHIFT

REM Leave task alone if already passing in arguments the normal way
ECHO %task% | FINDSTR /C:"[" >NUL
IF %ERRORLEVEL% EQU 0 (
  GOTO execute
)

REM Process remaining arguments
SET args=
:process_args
IF "%1"=="" GOTO done_args
IF "!args!"=="" (
  SET args=%1
) ELSE (
  SET args=!args!,%1
)
SHIFT
GOTO process_args

:done_args
REM If there are any arguments, format them as task[arg1,arg2,...]
IF NOT "!args!"=="" (
  SET task=%task%[!args!]
  ECHO Executing rake task: %task%
)

:execute
java %JAVA_OPTS% -jar third_party\jruby\jruby-complete.jar -X-C -S rake %task%
