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
        testSequenced (testList "specification validation" [
            testSequenced (testList "latest" [
                // run: arc-validate validate -i fixtures/arcs/specification/v2.0.0-draft
                // adapt this when a new latest specification package is available!
                yield!
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "validate"; "-i"; "fixtures/arcs/specification/v2.0.0-draft"; "-o"; "."|]
                        
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Console Output indicates that the tool will validate against specs" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("running `arc-validate validate` without")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Console Output indicates that the chosen spec version is latest" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("Performing validation against version 'latest' of the ARC specification")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Console Output indicates that latest spec version is mapped to a validation package correctly" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("latest spec version supported is 2.0.0-draft")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Ouptput files exist",
                            fun tool args proc -> 
                                Expect.isTrue (Directory.Exists(".arc-validate-results")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/arc_specification@2.0.0-draft/badge.svg")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/arc_specification@2.0.0-draft/badge.svg does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/arc_specification@2.0.0-draft/validation_report.xml")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/arc_specification@2.0.0-draft/validation_report.xml does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/arc_specification@2.0.0-draft/validation_summary.json")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/arc_specification@2.0.0-draft/validation_summary.json does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                        "Test arc passes spec validation",
                            fun tool args proc -> 
                                let summary = 
                                    ".arc-validate-results/arc_specification@2.0.0-draft/validation_summary.json"
                                    |> File.ReadAllText
                                    |> fun x -> x.ReplaceLineEndings("\n")
                                    |> ValidationSummary.fromJson

                                Expect.equal summary.Critical.Failed 0 (ErrorMessage.withProcessDiagnostics "incorrect number of critical failures" proc tool args )
                                Expect.equal summary.Critical.Errored 0 (ErrorMessage.withProcessDiagnostics "incorrect number of critical errors" proc tool args )
                                Expect.isFalse summary.Critical.HasFailures (ErrorMessage.withProcessDiagnostics "expected no critical failures" proc tool args )
                                
                                Expect.equal summary.Critical.Failed 0 (ErrorMessage.withProcessDiagnostics "incorrect number of noncritical failures" proc tool args )
                                Expect.equal summary.Critical.Errored 0 (ErrorMessage.withProcessDiagnostics "incorrect number of noncritical failures" proc tool args )
                                Expect.isFalse summary.Critical.HasFailures (ErrorMessage.withProcessDiagnostics "expected no noncritical failures" proc tool args )

                    ]
            ])
            testSequenced (testList "v2-0-0-draft" [
                // run: arc-validate validate --specification-version 2.0.0-draft -i fixtures/arcs/specification/v2.0.0-draft
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "validate"; "--specification-version"; "2.0.0-draft"; "-i"; "fixtures/arcs/specification/v2.0.0-draft"; "-o"; "."|]
                        
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Console Output indicates that the tool will validate against specs" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("running `arc-validate validate` without")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Console Output indicates that the chosen spec version is correct" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("Performing validation against version '2.0.0-draft' of the ARC specification")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Ouptput files exist",
                            fun tool args proc -> 
                                Expect.isTrue (Directory.Exists(".arc-validate-results")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/arc_specification@2.0.0-draft/badge.svg")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/arc_specification@2.0.0-draft/badge.svg does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/arc_specification@2.0.0-draft/validation_report.xml")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/arc_specification@2.0.0-draft/validation_report.xml does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/arc_specification@2.0.0-draft/validation_summary.json")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/arc_specification@2.0.0-draft/validation_summary.json does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                        "Test arc passes spec validation",
                            fun tool args proc -> 
                                let summary = 
                                    ".arc-validate-results/arc_specification@2.0.0-draft/validation_summary.json"
                                    |> File.ReadAllText
                                    |> fun x -> x.ReplaceLineEndings("\n")
                                    |> ValidationSummary.fromJson

                                Expect.equal summary.Critical.Failed 0 (ErrorMessage.withProcessDiagnostics "incorrect number of critical failures" proc tool args )
                                Expect.equal summary.Critical.Errored 0 (ErrorMessage.withProcessDiagnostics "incorrect number of critical errors" proc tool args )
                                Expect.isFalse summary.Critical.HasFailures (ErrorMessage.withProcessDiagnostics "expected no critical failures" proc tool args )
                                
                                Expect.equal summary.Critical.Failed 0 (ErrorMessage.withProcessDiagnostics "incorrect number of noncritical failures" proc tool args )
                                Expect.equal summary.Critical.Errored 0 (ErrorMessage.withProcessDiagnostics "incorrect number of noncritical failures" proc tool args )
                                Expect.isFalse summary.Critical.HasFailures (ErrorMessage.withProcessDiagnostics "expected no noncritical failures" proc tool args )

                    ]
            ])
        ])
    ])
