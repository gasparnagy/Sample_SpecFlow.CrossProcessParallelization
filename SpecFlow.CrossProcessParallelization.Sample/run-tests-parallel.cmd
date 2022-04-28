set SPECFLOW_TEST_RUN_ID=%date:/=-%-%time::=-%

echo %time%

(
   start "thread1" cmd /C "dotnet test --no-build --logger trx;logfilename=thread1.trx"
   start "thread2" cmd /C "dotnet test --no-build --logger trx;logfilename=thread2.trx"
   start "thread3" cmd /C "dotnet test --no-build --logger trx;logfilename=thread3.trx"
   start "thread4" cmd /C "dotnet test --no-build --logger trx;logfilename=thread4.trx"
) | pause

echo %time%
echo tests finished