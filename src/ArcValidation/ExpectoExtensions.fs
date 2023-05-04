namespace ArcValidation

[<AutoOpen>]
module Expecto =
    open System
    open FSharpAux
    open FsSpreadsheet
    open ArcGraphModel
    open ArcGraphModel.IO
    open Expecto
    open Expecto.Impl
    open System.Globalization
    open System.IO
    open System.Xml
    open System.Xml.Linq

    
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // taken & modified from: https://github.com/haf/expecto
    // ¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡¡

    let performTest test =
        let w = System.Diagnostics.Stopwatch()
        w.Start()
        evalTests Tests.defaultConfig test
        |> Async.RunSynchronously
        |> fun r -> 
            w.Stop()
            {
                results = r
                duration = w.Elapsed
                maxMemory = 0L
                memoryLimit = 0L
                timedOut = []
            }

    let expectoVersion = 
        // Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()   // fails because of not having the .dll at hand
        let userDir = Environment.SpecialFolder.UserProfile |> Environment.GetFolderPath        // workaround which uses NuGet package version
        Directory.GetDirectories(Path.Combine(userDir, ".nuget", "packages", "expecto"))
        |> Array.last
        |> fun dir -> 
            String.rev dir 
            |> String.takeWhile ((<>) '\\')
            |> String.rev

    let assemblyName = Reflection.Assembly.GetEntryAssembly().GetName().Name

    let xmlSave fileName (doc : XDocument) =
        let path = Path.GetFullPath fileName
        Path.GetDirectoryName path
        |> Directory.CreateDirectory
        |> ignore
        let settings = XmlWriterSettings(CheckCharacters = false)
        use writer = XmlWriter.Create(path, settings)
        doc.Save writer


    /// Generate test results using in a minimal JUnit schema.
    let writeJUnitSummary file (summary: Impl.TestRunSummary) =

        // junit does not have an official xml spec
        // this is a minimal implementation to get gitlab to recognize the tests:
        // https://docs.gitlab.com/ee/ci/junit_test_reports.html
        let totalTests = summary.errored @ summary.failed @ summary.ignored @ summary.passed
        let testCaseElements =
            totalTests
            |> Seq.sortByDescending (fun (_,test) -> test.result.order,test.duration.TotalSeconds)
            |> Seq.map (fun (flatTest, test) ->
            
                // flatTest.name string list gets squashed when upcast to XObject, therefore the list gets folded into a single string
                let fullnameString = 
                    flatTest.name 
                    |> List.fold (fun acc s -> acc + s + "; " ) "[ "
                    |> fun s -> s[.. String.length s - 3] + " ]"

                let content: XObject[] =
                    let makeMessageNode messageType (message: string) =
                        XElement(XName.Get messageType,
                            XAttribute(XName.Get "message", message))
                    match test.result with
                    | Passed -> [||]
                    | Error e ->
                        let message = makeMessageNode "error" e.Message
                        //message.Add(XCData(e.ToString()))     // commented out to tackle unnecessary error stack trace in error message
                        [|message|]
                    | Failed msg -> 
                        
                        [|makeMessageNode "failure" msg|]
                    | Ignored msg -> [|makeMessageNode "skipped" msg|]

                XElement(XName.Get "testcase",
                    [|
                        yield XAttribute(XName.Get "name", fullnameString) :> XObject
                        yield XAttribute(XName.Get "time",
                            System.String.Format(CultureInfo.InvariantCulture,
                                "{0:0.000}", test.duration.TotalSeconds)) :> XObject
                        yield! content
                    |]) :> XObject)
        let element =
            XElement(
                XName.Get "testsuites",
                    XElement(XName.Get "testsuite",
                        [|
                            yield XAttribute(XName.Get "name", assemblyName) :> XObject
                            yield! testCaseElements
                        |])
                )

        element
        |> XDocument
        |> xmlSave file


    /// Generate test results using NUnit v2 schema.
    let writeNUnitSummary (file : string option) (summary : TestRunSummary) =
        // v3: https://github.com/nunit/docs/wiki/Test-Result-XML-Format
        // this impl is v2: http://nunit.org/docs/files/TestResult.xml
        let totalTests = summary.errored @ summary.failed @ summary.ignored @ summary.passed
        let testCaseElements =
            totalTests
            //|> Seq.sortByDescending (fun (_,test) -> test.result.order, test.duration.TotalSeconds)
            |> Seq.map (fun (flatTest, test) ->
                    
                let fullnameString = 
                    flatTest.name 
                    |> List.fold (fun acc s -> acc + s + "; " ) "[ "
                    |> fun s -> s[.. String.length s - 3] + " ]"
                let element = XElement(XName.Get "test-case", XAttribute(XName.Get "name", fullnameString))
                let addAttribute name (content : string) = element.Add(XAttribute(XName.Get name, content))

                match test.result with
                | Ignored _ -> "False"
                | _ -> "True"
                |> addAttribute "executed"

                match test.result with
                | Passed -> "Success"
                | Error _
                | Failed _ -> "Failure"
                | Ignored _ -> "Ignored"
                |> addAttribute "result"

                match test.result with
                | Passed -> addAttribute "success" "True"
                | Error _
                | Failed _ -> addAttribute "success" "False"
                // Ignored tests are neither successful nor failed.
                | Ignored _ -> ()

                String.Format(CultureInfo.InvariantCulture, "{0:0.000}", test.duration.TotalSeconds)
                |> addAttribute "time"

                // TODO: implement it.
                addAttribute "asserts" "0"

                let failureNode = XElement(XName.Get "failure")

                // Some more details that explain why a test was not executed.
                match test.result with
                | Passed -> ()
                | Error e ->
                    failureNode.Add(XName.Get "message", XCData e.Message)
                    //failureNode.Add(XName.Get "stack-trace", XCData e.StackTrace)     // commented out to tackle unnecessary error stack trace in error message
                    element.Add failureNode
                | Failed msg ->
                    // added to tackle unnecessary error stack trace in failure message
                    let eWithoutStackTrace =
                        String.trim msg
                        |> String.split '\n'
                        |> Array.head
                    //failureNode.Add(XName.Get "message", XCData msg)
                    failureNode.Add(XName.Get "message", XCData eWithoutStackTrace)
                    element.Add failureNode
                | Ignored msg -> element.Add(XElement(XName.Get "reason", XElement(XName.Get "message", XCData msg)))
                element)
        let d = DateTime.Now
        let xAttr name data = XAttribute(XName.Get name, data)
        let element =
            XElement(XName.Get "test-results",
                xAttr "date" (d.ToString("yyyy-MM-dd")),
                xAttr "name" assemblyName,
                xAttr "total" totalTests.Length,
                xAttr "errors" summary.errored.Length,
                xAttr "failures" summary.failed.Length,
                xAttr "ignored" summary.ignored.Length,
                xAttr "not-run" "0",
                xAttr "inconclusive" "0",
                xAttr "skipped" "0",
                xAttr "invalid" "0",
                xAttr "time" (d.ToString("HH:mm:ss")),
                XElement(XName.Get "environment",
                    xAttr "expecto-version" expectoVersion,
                    xAttr "clr-version" Environment.Version,
                    xAttr "os-version" Environment.OSVersion.VersionString,
                    xAttr "platform" Environment.OSVersion.Platform,
                    xAttr "cwd" Environment.CurrentDirectory,
                    xAttr "machine-name" Environment.MachineName,
                    xAttr "user" Environment.UserName,
                    xAttr "user-domain" Environment.UserDomainName
                ),
                XElement(XName.Get "culture-info",
                    xAttr "current-culture", CultureInfo.CurrentCulture,
                    xAttr "current-uiculture", CultureInfo.CurrentUICulture
                ),
                XElement(XName.Get "test-suite",
                    xAttr "type" "Assembly",
                    xAttr "name" assemblyName,
                    xAttr "executed" "True",
                    xAttr "result" (if summary.successful then "Success" else "Failure"),
                    xAttr "success" (if summary.successful then "True" else "False"),
                    xAttr "time"
                        (String.Format(CultureInfo.InvariantCulture,
                            "{0:0.000}", summary.duration.TotalSeconds)),
                    xAttr "asserts" "0",
                    XElement(XName.Get "results", testCaseElements)
                )
            )
    
        let xDoc = XDocument element

        match file with
        | Some path -> xmlSave path xDoc
        | None      -> ()

        xDoc.ToString()