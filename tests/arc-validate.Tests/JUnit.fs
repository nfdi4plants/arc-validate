module JUnit

open System.Xml

type ValidationResults = {
    PassedTests: string list
    FailedTests: string list
    ErroredTests: string list
} with
    static member fromJUnitFile (path:string) =
        let doc = new XmlDocument()
        doc.Load(path)
        let suite = doc.SelectNodes("/testsuites/testsuite[@name='arc-validate']").Item(0);
        let testCases = suite.SelectNodes("testcase") |> Seq.cast<XmlNode>
        {
            PassedTests =
                testCases
                |> Seq.filter (fun tc -> tc.SelectNodes("failure").Count = 0)
                |> Seq.map (fun tc -> tc.Attributes.["name"].Value)
                |> Seq.toList
                |> List.sort

            FailedTests =
                testCases
                |> Seq.filter (fun tc -> tc.SelectNodes("failure").Count > 0)
                |> Seq.map (fun tc -> tc.Attributes.["name"].Value)
                |> Seq.toList
                |> List.sort

            ErroredTests =
                testCases
                |> Seq.filter (fun tc -> tc.SelectNodes("error").Count > 0)
                |> Seq.map (fun tc -> tc.Attributes.["name"].Value)
                |> Seq.toList
                |> List.sort
        }

    static member getTotalTestCount (res: ValidationResults) =
        res.ErroredTests.Length + res.FailedTests.Length + res.PassedTests.Length