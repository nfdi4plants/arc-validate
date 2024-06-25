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
        testSequenced (testList "source: preview index" [
            testSequenced (testList "package test version 2" [
                // run:
                // - arc-validate --verbose package install test -v 2.0.0 --preview
                // - arc-validate validate -p test -v 2.0.0 --preview -i fixtures/arcs/inveniotestarc
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "2.0.0"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Install: Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" , 
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed preview package test@2.0.0.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in preview cache after running package install test" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@2.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                        "Install: Package script does not exist in avpr cache after running package install test" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@2.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                
                    ]
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "validate"; "-p"; "test"; "-v"; "2.0.0"; "--preview"; "-i"; "fixtures/arcs/inveniotestarc"|]
                        (get_gh_api_token())
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
            testSequenced (testList "package test version 3_0_0" [
                // run:
                // - arc-validate --verbose package install test -v 3.0.0 --preview
                // - arc-validate validate -p test -v 3.0.0 --preview -i fixtures/arcs/inveniotestarc
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "3.0.0"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Install: Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" , 
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed preview package test@3.0.0.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in preview cache after running package install test" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@3.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                        "Install: Package script does not exist in avpr cache after running package install test" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@3.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )

                    ]
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "validate"; "-p"; "test"; "-v"; "3.0.0"; "--preview"; "-i"; "fixtures/arcs/inveniotestarc"|]
                        (get_gh_api_token())
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
            testSequenced (testList "package test version 5_0_0" [
                // run:
                // - arc-validate --verbose package install test -v 5.0.0 --preview
                // - arc-validate validate -p test -v 5.0.0 --preview -i fixtures/arcs/inveniotestarc
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "5.0.0"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Install: Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" , 
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed preview package test@5.0.0.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in preview cache after running package install test" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@5.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                        "Install: Package script does not exist in avpr cache after running package install test" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@5.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                
                    ]
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "validate"; "-p"; "test"; "-v"; "5.0.0"; "--preview"; "-i"; "fixtures/arcs/inveniotestarc"|]
                        (get_gh_api_token())
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
                // run:
                // - arc-validate --verbose package install test -v 2.0.0 --preview
                // - arc-validate validate -p test -v 2.0.0 --preview -i fixtures/arcs/inveniotestarc
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "5.0.0-use+suffixes"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Install: Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" , 
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed preview package test@5.0.0-use+suffixes.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in preview cache after running package install test" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@5.0.0-use+suffixes.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                        "Install: Package script does not exist in avpr cache after running package install test" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@5.0.0-use+suffixes.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                
                    ]
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "validate"; "-p"; "test"; "-v"; "5.0.0-use+suffixes"; "--preview"; "-i"; "fixtures/arcs/inveniotestarc"|]
                        (get_gh_api_token())
                    ) [
                        "Validate: Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Validate: Console output does not indicate that package is not installed" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("Package test not installed. You can run run arc-validate package install ")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Validate: Console Output is correct" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("If you can read this in your console, you successfully executed test package v5.0.0-use+suffixes!")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        ]
            ])
            testSequenced (testList "package test version latest" [
                // run:
                // - arc-validate --verbose package install test --preview
                // - arc-validate validate -p test --preview -i fixtures/arcs/inveniotestarc
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Install: Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" , 
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed preview package test@999.999.999.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in preview cache after running package install test" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@999.999.999.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                        "Install: Package script does not exist in avpr cache after running package install test" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@999.999.999.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                
                    ]
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "validate"; "-p"; "test"; "--preview"; "-i"; "fixtures/arcs/inveniotestarc"|]
                        (get_gh_api_token())
                    ) [
                        "Validate: Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Validate: Console output does not indicate that package is not installed" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("Package test not installed. You can run run arc-validate package install ")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Validate: Console Output is correct" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("If you can read this in your console, you successfully executed preview test package v999.999.999!")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        ]
                ])
            ])
        testSequenced (testList "source: AVPR" [
            testSequenced (testList "package test version 2" [
                yield! 
                // run:
                // - arc-validate --verbose package install test -v 2.0.0
                // - arc-validate validate -p test -v 2.0.0 --preview -i fixtures/arcs/inveniotestarc
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "2.0.0"|]
                        (get_gh_api_token())
                    ) [
                        "Install: Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" , 
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed package test@2.0.0.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in avpr cache after running package install test" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@2.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                        "Install: Package script does not exist in preview cache after running package install test" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@2.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                
                    ]
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "validate"; "-p"; "test"; "-v"; "2.0.0"; "-i"; "fixtures/arcs/inveniotestarc"|]
                        (get_gh_api_token())
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
                // - arc-validate validate -p test -v 3.0.0 --preview -i fixtures/arcs/inveniotestarc
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "3.0.0"|]
                        (get_gh_api_token())
                    ) [
                        "Install: Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" , 
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed package test@3.0.0.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in avpr cache after running package install test" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@3.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                        "Install: Package script does not exist in preview cache after running package install test" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@3.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                
                    ]
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "validate"; "-p"; "test"; "-v"; "3.0.0"; "-i"; "fixtures/arcs/inveniotestarc"|]
                        (get_gh_api_token())
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
                // - arc-validate validate -p test -v 5.0.0 --preview -i fixtures/arcs/inveniotestarc
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "5.0.0"|]
                        (get_gh_api_token())
                    ) [
                        "Install: Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" , 
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed package test@5.0.0.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in avpr cache after running package install test" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@5.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                        "Install: Package script does not exist in preview cache after running package install test" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@5.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                
                    ]
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "validate"; "-p"; "test"; "-v"; "5.0.0"; "-i"; "fixtures/arcs/inveniotestarc"|]
                        (get_gh_api_token())
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
                // - arc-validate validate -p test -v 5.0.0-use+suffixes --preview -i fixtures/arcs/inveniotestarc
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "5.0.0-use+suffixes"|]
                        (get_gh_api_token())
                    ) [
                        "Install: Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" , 
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed package test@5.0.0-use+suffixes.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in avpr cache after running package install test" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@5.0.0-use+suffixes.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                        "Install: Package script does not exist in preview cache after running package install test" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@5.0.0-use+suffixes.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                
                    ]
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "validate"; "-p"; "test"; "-v"; "5.0.0-use+suffixes"; "-i"; "fixtures/arcs/inveniotestarc"|]
                        (get_gh_api_token())
                    ) [
                        "Validate: Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Validate: Console output does not indicate that package is not installed" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("Package test not installed. You can run run arc-validate package install ")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Validate: Console Output is correct" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("If you can read this in your console, you successfully executed test package v5.0.0-use+suffixes!")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        ]
            ])
            testSequenced (testList "package test version latest" [
                // run:
                // - arc-validate --verbose package install test
                // - arc-validate validate -p test -i fixtures/arcs/inveniotestarc
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test";|]
                        (get_gh_api_token())
                    ) [
                        "Install: Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Install: Console output indicates that the package was installed" , 
                            fun tool args proc -> Expect.stringContains proc.Result.Output "installed package test@5.0.0.fsx at" (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Install: Package script exists in avpr cache after running package install test" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@5.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                        "Install: Package script does not exist in preview cache after running package install test" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@5.0.0.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                
                    ]
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "validate"; "-p"; "test"; "-i"; "fixtures/arcs/inveniotestarc"|]
                        (get_gh_api_token())
                    ) [
                        "Validate: Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Validate: Console output does not indicate that package is not installed" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("Package test not installed. You can run run arc-validate package install ")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Validate: Console Output is correct" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("If you can read this in your console, you successfully executed test package v5.0.0!")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
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
                        (get_gh_api_token())
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
                        (get_gh_api_token())
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