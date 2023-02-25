module SummaryFileWriter

open FSharpAux
open Expecto
open Impl
open System
open System.IO
open System.Xml
open System.Xml.Linq

let private expectoVersion = 
    // Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()   // fails because of not having the .dll at hand
    let userDir = Environment.SpecialFolder.UserProfile |> Environment.GetFolderPath        // workaround which uses NuGet package version
    Directory.GetDirectories(Path.Combine(userDir, ".nuget", "packages", "expecto"))
    |> Array.last
    |> fun dir -> 
        String.rev dir 
        |> String.takeWhile ((<>) '\\')
        |> String.rev

let private assemblyName = Reflection.Assembly.GetEntryAssembly().GetName().Name

let private xmlSave fileName (doc : XDocument) =
    let path = Path.GetFullPath fileName
    Path.GetDirectoryName path
    |> Directory.CreateDirectory
    |> ignore
    let settings = XmlWriterSettings(CheckCharacters = false)
    use writer = XmlWriter.Create(path, settings)
    doc.Save writer


// delete when added to FSharpAux
module String =
    /// Returns a new string in which all leading and trailing occurrences of white-space characters from the current string are removed.
    let trim (s : string) = s.Trim()


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
                | Failed msg -> [|makeMessageNode "failure" msg|]
                | Ignored msg -> [|makeMessageNode "skipped" msg|]

            XElement(XName.Get "testcase",
                [|
                    yield XAttribute(XName.Get "name", fullnameString) :> XObject
                    yield XAttribute(XName.Get "time",
                        System.String.Format(System.Globalization.CultureInfo.InvariantCulture,
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