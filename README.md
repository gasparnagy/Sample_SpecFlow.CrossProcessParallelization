# SpecFlow.CrossProcessParallelization - Sample application to demonstrate cross process parallelization with SpecFlow

The standard test execution frameworks supported by SpecFlow (MsTest, NUnit, xUnit) support parallel execution, but the parallel threads are going to be hosted in the same process and use the same shared memory. This is usually not a problem with modern applications, because they don't use static fields, so the shared memory does not cause a conflict between the parallel test execution threads.

For older applications this might be a problem though. SpecFlow+ Runner supported AppDomain and process isolation to handle this, but as the tool has been [retired](https://specflow.org/using-specflow/the-retirement-of-specflow-runner/), you need to migration path.

This sample application demonstrates how this can be achieved with very simple tooling. The sample code contains a file `Support/CrossProcessParallelHooks.cs` that does the trick: The idea is that you run multiple test execution processes (invoke `dotnet test` multiple times) and when one process starts executing a particular scenario, it creates a lock file somewhere in the TEMP folder. When another process tries to run the same scenario, it detects the existence of the lock file and skips the test (in NUnit with `Assert.Ignore()`). The different tests processes generate result files (TRX), that would need to be merged (not included in the sample) to get the consolidated result.

To indicate that you would like to execute the tests parallel, you need to set the `SPECFLOW_TEST_RUN_ID` environment variable to a unique value. If the variable is not set (like when you run the tests from Visual Studio), it just simply runs all tests.

The sample contains 20 tests (a scenario outline with 20 examples). Each test takes 2 seconds to complete, so 40 seconds when run single threaded. The file `run-tests-parallel.cmd` runs the same tests with 4 parallel test thread processes and this way the execution time is about 10 seconds. 

## License

The sample application is licensed under the [MIT license](LICENSE).

Copyright (c) 2022 Spec Solutions and Gaspar Nagy, https://www.specsolutions.eu
