﻿module CLITests.PackageCommand

open Expecto
open Expecto.Tests
open Expecto.Impl
open Expecto.Logging
open System.IO
open System.Diagnostics
open Fake.Core
open ReferenceObjects

open Common
open Common.TestUtils
open TestUtils

open JUnit

[<Tests>]
let ``PackageCommand CLI Tests`` =
    testSequenced (testList "arc-validate package" [
        testSequenced (testList "source: preview index" [
            testSequenced (testList "list" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (before install)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is not listed (before install)" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("test")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did contain the package" proc tool args)
                
                    ]
            ])
            testSequenced (testList "install test v1" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "1.0.0"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_preview}." tool args)
                        "Cache exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_preview}." tool args)
                        "Package script exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@1.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not installed at expected location." tool args)
                        "Package script has correct content" ,
                            fun tool args proc -> 
                                Expect.equal 
                                    (File.ReadAllText(Path.Combine(expected_package_cache_folder_path_preview, "test@1.0.0.fsx")).ReplaceLineEndings("\n"))
                                    test_package_script_content_v1_0_0
                                    (ErrorMessage.withCLIDiagnostics $"Package script did not have correct content" tool args)
                    ]
            ])
            testSequenced (testList "list v1" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after install)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is listed (after install)" , 
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("test @ version 1.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did not contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "uninstall test v1" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "uninstall"; "test"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder still exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_preview}." tool args)
                        "Cache still exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_preview}." tool args)
                        "test package script does not exist anymore" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@1.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not uninstalled at expected location." tool args)
                    ]
            ])
            testSequenced (testList "list v1" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after uninstall)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is not listed (after uninstall)" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("test @ version 1.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "install test v2" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "2.0.0"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_preview}." tool args)
                        "Cache exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_preview}." tool args)
                        "Package script exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@2.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not installed at expected location." tool args)
                        "Package script has correct content" ,
                            fun tool args proc -> 
                                Expect.equal 
                                    (File.ReadAllText(Path.Combine(expected_package_cache_folder_path_preview, "test@2.0.0.fsx")).ReplaceLineEndings("\n"))
                                    test_package_script_content_v2_0_0
                                    (ErrorMessage.withCLIDiagnostics $"Package script did not have correct content" tool args)
                    ]
            ])
            testSequenced (testList "list v2" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after install)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is listed (after install)" , 
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("test @ version 2.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did not contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "uninstall test v2" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "uninstall"; "test"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder still exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_preview}." tool args)
                        "Cache still exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_preview}." tool args)
                        "test package script does not exist anymore" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@2.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not uninstalled at expected location." tool args)
                    ]
            ])
            testSequenced (testList "list v2" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after uninstall)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is not listed (after uninstall)" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("test @ version 2.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "install test v3" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "3.0.0"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_preview}." tool args)
                        "Cache exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_preview}." tool args)
                        "Package script exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@3.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not installed at expected location." tool args)
                        "Package script has correct content" ,
                            fun tool args proc -> 
                                Expect.equal 
                                    (File.ReadAllText(Path.Combine(expected_package_cache_folder_path_preview, "test@3.0.0.fsx")).ReplaceLineEndings("\n"))
                                    test_package_script_content_v3_0_0
                                    (ErrorMessage.withCLIDiagnostics $"Package script did not have correct content" tool args)
                    ]
            ])
            testSequenced (testList "list v3" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after install)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is listed (after install)" , 
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("test @ version 3.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did not contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "uninstall test v3" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "uninstall"; "test"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder still exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_preview}." tool args)
                        "Cache still exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_preview}." tool args)
                        "test package script does not exist anymore" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@3.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not uninstalled at expected location." tool args)
                    ]
            ])
            testSequenced (testList "list v3" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after uninstall)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is not listed (after uninstall)" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("test @ version 3.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "install test v5" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "5.0.0"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_preview}." tool args)
                        "Cache exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_preview}." tool args)
                        "Package script exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@5.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not installed at expected location." tool args)
                        "Package script has correct content" ,
                            fun tool args proc -> 
                                Expect.equal 
                                    (File.ReadAllText(Path.Combine(expected_package_cache_folder_path_preview, "test@5.0.0.fsx")).ReplaceLineEndings("\n"))
                                    test_package_script_content_v5_0_0
                                    (ErrorMessage.withCLIDiagnostics $"Package script did not have correct content" tool args)
                    ]
            ])
            testSequenced (testList "list v5" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after install)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is listed (after install)" , 
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("test @ version 5.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did not contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "uninstall test v5" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "uninstall"; "test"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder still exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_preview}." tool args)
                        "Cache still exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_preview}." tool args)
                        "test package script does not exist anymore" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@5.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not uninstalled at expected location." tool args)
                    ]
            ])
            testSequenced (testList "list v5" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after uninstall)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is not listed (after uninstall)" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("test @ version 5.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "install test v5-use+suffixes" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "5.0.0-use+suffixes"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_preview}." tool args)
                        "Cache exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_preview}." tool args)
                        "Package script exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@5.0.0-use+suffixes.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not installed at expected location." tool args)
                        "Package script has correct content" ,
                            fun tool args proc -> 
                                Expect.equal 
                                    (File.ReadAllText(Path.Combine(expected_package_cache_folder_path_preview, "test@5.0.0-use+suffixes.fsx")).ReplaceLineEndings("\n"))
                                    ``test_package_script_content_v5_0_0-use+suffixes``
                                    (ErrorMessage.withCLIDiagnostics $"Package script did not have correct content" tool args)
                    ]
            ])
            testSequenced (testList "list v5-use+suffixes" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after install)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is listed (after install)" , 
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("test @ version 5.0.0-use+suffixes")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did not contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "uninstall test v5-use+suffixes" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "uninstall"; "test"; "--preview"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder still exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_preview}." tool args)
                        "Cache still exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_preview)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_preview}." tool args)
                        "test package script does not exist anymore" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_preview, "test@5.0.0-use+suffixes.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not uninstalled at expected location." tool args)
                    ]
            ])
            testSequenced (testList "list v5-use+suffixes" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after uninstall)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is not listed (after uninstall)" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("test @ version 5.0.0-use+suffixes")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "update-index" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "update-index"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                    ]
            ])
        ])
        testSequenced (testList "source: AVPR" [
            testSequenced (testList "list" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (before install)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is not listed (before install)" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("test")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did contain the package" proc tool args)
                
                    ]
            ])
            testSequenced (testList "install test v1" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "1.0.0"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_release}." tool args)
                        "Cache exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_release}." tool args)
                        "Package script exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@1.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not installed at expected location." tool args)
                        "Package script has correct content" ,
                            fun tool args proc -> 
                                Expect.equal 
                                    (File.ReadAllText(Path.Combine(expected_package_cache_folder_path_release, "test@1.0.0.fsx")).ReplaceLineEndings("\n"))
                                    test_package_script_content_v1_0_0
                                    (ErrorMessage.withCLIDiagnostics $"Package script did not have correct content" tool args)
                    ]
            ])
            testSequenced (testList "list v1" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after install)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is listed (after install)" , 
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("test @ version 1.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did not contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "uninstall test v1" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "uninstall"; "test"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder still exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_release}." tool args)
                        "Cache still exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_release}." tool args)
                        "test package script does not exist anymore" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@1.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not uninstalled at expected location." tool args)
                    ]
            ])
            testSequenced (testList "list v1" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after uninstall)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is not listed (after uninstall)" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("test @ version 1.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "install test v2" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "2.0.0"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_release}." tool args)
                        "Cache exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_release}." tool args)
                        "Package script exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@2.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not installed at expected location." tool args)
                        "Package script has correct content" ,
                            fun tool args proc -> 
                                Expect.equal 
                                    (File.ReadAllText(Path.Combine(expected_package_cache_folder_path_release, "test@2.0.0.fsx")).ReplaceLineEndings("\n"))
                                    test_package_script_content_v2_0_0
                                    (ErrorMessage.withCLIDiagnostics $"Package script did not have correct content" tool args)
                    ]
            ])
            testSequenced (testList "list v2" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after install)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is listed (after install)" , 
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("test @ version 2.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did not contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "uninstall test v2" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "uninstall"; "test";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder still exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_release}." tool args)
                        "Cache still exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_release}." tool args)
                        "test package script does not exist anymore" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@2.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not uninstalled at expected location." tool args)
                    ]
            ])
            testSequenced (testList "list v2" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after uninstall)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is not listed (after uninstall)" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("test @ version 2.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "install test v3" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "3.0.0"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_release}." tool args)
                        "Cache exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_release}." tool args)
                        "Package script exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@3.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not installed at expected location." tool args)
                        "Package script has correct content" ,
                            fun tool args proc -> 
                                Expect.equal 
                                    (File.ReadAllText(Path.Combine(expected_package_cache_folder_path_release, "test@3.0.0.fsx")).ReplaceLineEndings("\n"))
                                    test_package_script_content_v3_0_0
                                    (ErrorMessage.withCLIDiagnostics $"Package script did not have correct content" tool args)
                    ]
            ])
            testSequenced (testList "list v3" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after install)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is listed (after install)" , 
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("test @ version 3.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did not contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "uninstall test v3" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "uninstall"; "test";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder still exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_release}." tool args)
                        "Cache still exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_release}." tool args)
                        "test package script does not exist anymore" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@3.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not uninstalled at expected location." tool args)
                    ]
            ])
            testSequenced (testList "list v3" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after uninstall)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is not listed (after uninstall)" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("test @ version 3.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "install test v5" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "5.0.0"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_release}." tool args)
                        "Cache exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_release}." tool args)
                        "Package script exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@5.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not installed at expected location." tool args)
                        "Package script has correct content" ,
                            fun tool args proc -> 
                                Expect.equal 
                                    (File.ReadAllText(Path.Combine(expected_package_cache_folder_path_release, "test@5.0.0.fsx")).ReplaceLineEndings("\n"))
                                    test_package_script_content_v5_0_0
                                    (ErrorMessage.withCLIDiagnostics $"Package script did not have correct content" tool args)
                    ]
            ])
            testSequenced (testList "list v5" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after install)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is listed (after install)" , 
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("test @ version 5.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did not contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "uninstall test v5" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "uninstall"; "test";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder still exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_release}." tool args)
                        "Cache still exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_release}." tool args)
                        "test package script does not exist anymore" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@5.0.0.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not uninstalled at expected location." tool args)
                    ]
            ])
            testSequenced (testList "list v5" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after uninstall)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is not listed (after uninstall)" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("test @ version 5.0.0")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "install test v5-use+suffixes" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        true
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "install"; "test"; "-v"; "5.0.0-use+suffixes"|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_release}." tool args)
                        "Cache exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_release}." tool args)
                        "Package script exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@5.0.0-use+suffixes.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not installed at expected location." tool args)
                        "Package script has correct content" ,
                            fun tool args proc -> 
                                Expect.equal 
                                    (File.ReadAllText(Path.Combine(expected_package_cache_folder_path_release, "test@5.0.0-use+suffixes.fsx")).ReplaceLineEndings("\n"))
                                    ``test_package_script_content_v5_0_0-use+suffixes``
                                    (ErrorMessage.withCLIDiagnostics $"Package script did not have correct content" tool args)
                    ]
            ])
            testSequenced (testList "list v5-use+suffixes" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after install)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is listed (after install)" , 
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("test @ version 5.0.0-use+suffixes")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did not contain the package" proc tool args)
                    ]
            ])
            testSequenced (testList "uninstall test v5-use+suffixes" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "uninstall"; "test";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "Cache folder still exists" ,  
                            fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache folder was not created at {expected_package_cache_folder_path_release}." tool args)
                        "Cache still exists" ,  
                            fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path_release)) (ErrorMessage.withCLIDiagnostics $"package cache was not created at {expected_package_cache_file_path_release}." tool args)
                        "test package script does not exist anymore" ,  
                            fun tool args proc -> Expect.isFalse (File.Exists(Path.Combine(expected_package_cache_folder_path_release, "test@5.0.0-use+suffixes.fsx"))) (ErrorMessage.withCLIDiagnostics $"package file was not uninstalled at expected location." tool args)
                    ]
            ])
            testSequenced (testList "list v5-use+suffixes" [
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "package"; "list";|]
                        (get_gh_api_token())
                    ) [
                        "Exit code is 0 (after uninstall)" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args)
                        "test package is not listed (after uninstall)" , 
                            fun tool args proc -> Expect.isFalse (proc.Result.Output.Contains("test @ version 5.0.0-use+suffixes")) (ErrorMessage.withProcessDiagnostics $"Console output {proc.Result.Output} did contain the package" proc tool args)
                    ]
            ])
        ])
    ])