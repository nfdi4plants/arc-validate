module CLITests.ValidateCommand

open Expecto
open Expecto.Tests
open Expecto.Impl
open Expecto.Logging
open System.IO
open System.Diagnostics
open Fake.Core

open ARCExpect

open Common
open Common.TestUtils
open TestUtils

open JUnit

[<Tests>]
let ``ValidateCommand CLI Tests`` =
    testSequenced (testList "arc-validate validate" [
        testSequenced (testList "source: AVPR" [
            testSequenced (testList "package test version 2" [
                yield!
                // run:
                // - arc-validate --verbose package install test -v 2.0.0
                    testFixture (Fixtures.withToolExecution
                        true
                        "../../../../../publish/arc-validate"
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "2.0.0"|]

                    ) [
                        "Install: Exit code is 0" ,
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" ,
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed package test@2.0.0.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in avpr cache after running package install test" ,
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path, "test@2.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )

                    ]
                yield!
                    testFixture (Fixtures.withToolExecution
                        false
                        "../../../../../publish/arc-validate"
                        [|"--verbose"; "validate"; "-p"; "test"; "-v"; "2.0.0"; "-i"; "fixtures/arcs/inveniotestarc"|]

                    ) [
                        "Validate: Exit code is 0" ,
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Validate: Console output does not indicate that package is not installed" ,
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("Package test not installed. You can run run arc-validate package install ")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Validate: Console Output is correct" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("If you can read this in your console, you successfully executed test package v2.0.0!")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Validate: Ouptput files exist",
                            fun tool args proc ->
                                Expect.isTrue (Directory.Exists(".arc-validate-results")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/test/badge.svg")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/test/badge.svg does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/test/validation_report.xml")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/test/validation_report.xml does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                        ]
            ])
            testSequenced (testList "package test version 3" [
                yield!
                // run:
                // - arc-validate --verbose package install test -v 3.0.0
                    testFixture (Fixtures.withToolExecution
                        true
                        "../../../../../publish/arc-validate"
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "3.0.0"|]

                    ) [
                        "Install: Exit code is 0" ,
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" ,
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed package test@3.0.0.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in avpr cache after running package install test" ,
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path, "test@3.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                    ]
                yield!
                    testFixture (Fixtures.withToolExecution
                        false
                        "../../../../../publish/arc-validate"
                        [|"--verbose"; "validate"; "-p"; "test"; "-v"; "3.0.0"; "-i"; "fixtures/arcs/inveniotestarc"|]

                    ) [
                        "Validate: Exit code is 0" ,
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Validate: Console output does not indicate that package is not installed" ,
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("Package test not installed. You can run run arc-validate package install ")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Validate: Console Output is correct" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("If you can read this in your console, you successfully executed test package v3.0.0!")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Validate: Ouptput files exist",
                            fun tool args proc ->
                                Expect.isTrue (Directory.Exists(".arc-validate-results")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/test/badge.svg")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/test/badge.svg does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/test/validation_report.xml")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/test/validation_report.xml does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                        ]
            ])
            testSequenced (testList "package test version 5" [
                yield!
                // run:
                // - arc-validate --verbose package install test -v 5.0.0
                    testFixture (Fixtures.withToolExecution
                        true
                        "../../../../../publish/arc-validate"
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "5.0.0"|]

                    ) [
                        "Install: Exit code is 0" ,
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" ,
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed package test@5.0.0.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in avpr cache after running package install test" ,
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path, "test@5.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                    ]
                yield!
                    testFixture (Fixtures.withToolExecution
                        false
                        "../../../../../publish/arc-validate"
                        [|"--verbose"; "validate"; "-p"; "test"; "-v"; "5.0.0"; "-i"; "fixtures/arcs/inveniotestarc"|]

                    ) [
                        "Validate: Exit code is 0" ,
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Validate: Console output does not indicate that package is not installed" ,
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("Package test not installed. You can run run arc-validate package install ")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Validate: Console Output is correct" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("If you can read this in your console, you successfully executed test package v5.0.0!")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Validate: Ouptput files exist",
                            fun tool args proc ->
                                Expect.isTrue (Directory.Exists(".arc-validate-results")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/test@5.0.0/badge.svg")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/test/badge.svg does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/test@5.0.0/validation_report.xml")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/test/validation_report.xml does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/test@5.0.0/validation_summary.json")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/test/validation_summary.json does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                        ]
            ])
            testSequenced (testList "package test version 5_0_0-use+suffixes" [
                yield!
                // run:
                // - arc-validate --verbose package install test -v 5.0.0-use+suffixes
                    testFixture (Fixtures.withToolExecution
                        true
                        "../../../../../publish/arc-validate"
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "5.0.0-use+suffixes"|]

                    ) [
                        "Install: Exit code is 0" ,
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" ,
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed package test@5.0.0-use+suffixes.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in avpr cache after running package install test" ,
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path, "test@5.0.0-use+suffixes.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )

                    ]
                yield!
                    testFixture (Fixtures.withToolExecution
                        false
                        "../../../../../publish/arc-validate"
                        [|"--verbose"; "validate"; "-p"; "test"; "-v"; "5.0.0-use+suffixes"; "-i"; "fixtures/arcs/inveniotestarc"|]

                    ) [
                        "Validate: Exit code is 0" ,
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Validate: Console output does not indicate that package is not installed" ,
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("Package test not installed. You can run run arc-validate package install ")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Validate: Console Output is correct" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("If you can read this in your console, you successfully executed test package v5.0.0-use+suffixes!")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        ]
            ])
            testSequenced (testList "package test latest version installation" [
                // run:
                // - arc-validate --verbose package install test
                yield!
                    testFixture (Fixtures.withToolExecution
                        true
                        "../../../../../publish/arc-validate"
                        [|"--verbose"; "package"; "install"; "test";|]

                    ) [
                        "Install: Exit code is 0" ,
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" ,
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed package test@7.0.0.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in avpr cache after running package install test" ,
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path, "test@7.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )

                    ]
                ])
            testSequenced (testList "package test-py version 002" [
                yield!
                // run:
                // - arc-validate --verbose package install test-py -v 0.0.2
                    testFixture (Fixtures.withToolExecution
                        true
                        "../../../../../publish/arc-validate"
                        [|"--verbose"; "package"; "install"; "test-py"; "-v"; "0.0.2"|]

                    ) [
                        "Install: Exit code is 0" ,
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" ,
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed package test-py@0.0.2.py at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in avpr cache after running package install test" ,
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path, "test-py@0.0.2.py")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )

                    ]
                yield!
                    testFixture (Fixtures.withToolExecution
                        false
                        "../../../../../publish/arc-validate"
                        [|"--verbose"; "validate"; "-p"; "test-py"; "-v"; "0.0.2"; "-i"; "fixtures/arcs/inveniotestarc"|]

                    ) [
                        "Validate: Exit code is 0" ,
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Validate: Console output does not indicate that package is not installed" ,
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("Package test not installed. You can run run arc-validate package install ")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Validate: Console Output is correct" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("If you can read this in your console, you are executing test-py package v0.0.2!")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                    ]
            ])
        ])
    ])
