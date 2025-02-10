#r "nuget: ARCtrl"
//#r "nuget: ARCExpect"
#r "nuget: ControlledVocabulary"
#r "nuget: Expecto"
//#r "nuget: ARCTokenization"


open ARCtrl
//open ARCExpect
open ControlledVocabulary
open Expecto
//open ARCTokenization
//open ARCtrl
open ARCtrl.ROCrate
open ARCtrl.Json
open DynamicObj.DynObj


let arc = ARC.load @"C:\Repos\git.nfdi4plants.org\ArcPrototype"

//arc.ISA.Value.Assays |> Seq.iter (fun a -> printfn $"{a.Identifier}")
//arc.ISA.Value.GetAssay ""

//arc.ToROCrateJsonString()



let arc = """{
                                "@id": "#Process_Cell_Lysis",
                                "@type": [
                                    "Process"
                                ],
                                "name": "Cell Lysis",
                                "executesProtocol": {
                                    "@id": "#Protocol_experiment1_material_measurement1_Cell_Lysis",
                                    "@type": [
                                        "Protocol"
                                    ],
                                    "components": [
                                        {
                                            "@id": "#Component/sonicator=Fisherbrand™ Model 705 Sonic Dismembrator",
                                            "@type": "PropertyValue",
                                            "additionalType": "Component",
                                            "alternateName": "Fisherbrand™ Model 705 Sonic Dismembrator (user-specific:user-specific)",
                                            "category": "sonicator",
                                            "categoryCode": "OBI:0400114",
                                            "value": "Fisherbrand™ Model 705 Sonic Dismembrator",
                                            "valueCode": "user-specific"
                                        },
                                        {
                                            "@id": "#Component/centrifuge=Eppendorf™ Centrifuge 5420",
                                            "@type": "PropertyValue",
                                            "additionalType": "Component",
                                            "alternateName": "Eppendorf™ Centrifuge 5420 (user-specific:user-specific)",
                                            "category": "centrifuge",
                                            "categoryCode": "OBI:0400106",
                                            "value": "Eppendorf™ Centrifuge 5420",
                                            "valueCode": "user-specific"
                                        }
                                    ]
                                },
                                "parameterValues": [
                                    {
                                        "@id": "#ProcessParameterValue/cell lysis=Sonication",
                                        "@type": "PropertyValue",
                                        "additionalType": "ProcessParameterValue",
                                        "category": "cell lysis",
                                        "categoryCode": "OBI:0302894",
                                        "value": "Sonication",
                                        "valueCode": "http://purl.obolibrary.org/obo/NCIT_C81871"
                                    },
                                    {
                                        "@id": "#ProcessParameterValue/centrifugation=10g unit",
                                        "@type": "PropertyValue",
                                        "additionalType": "ProcessParameterValue",
                                        "category": "centrifugation",
                                        "categoryCode": "OBI:0302886",
                                        "value": 10,
                                        "unit": "g unit",
                                        "unitCode": "user-specific"
                                    }
                                ],
                                "inputs": [
                                    {
                                        "@id": "#Sample_Cultivation_flask",
                                        "@type": [
                                            "Sample"
                                        ],
                                        "additionalType": "Sample",
                                        "name": "Cultivation flask"
                                    },
                                    {
                                        "@id": "#Sample_Cultivation_flask",
                                        "@type": [
                                            "Sample"
                                        ],
                                        "additionalType": "Sample",
                                        "name": "Cultivation flask"
                                    },
                                    {
                                        "@id": "#Sample_Cultivation_flask",
                                        "@type": [
                                            "Sample"
                                        ],
                                        "additionalType": "Sample",
                                        "name": "Cultivation flask"
                                    },
                                    {
                                        "@id": "#Sample_Cultivation_flask",
                                        "@type": [
                                            "Sample"
                                        ],
                                        "additionalType": "Sample",
                                        "name": "Cultivation flask"
                                    },
                                    {
                                        "@id": "#Sample_Cultivation_flask",
                                        "@type": [
                                            "Sample"
                                        ],
                                        "additionalType": "Sample",
                                        "name": "Cultivation flask"
                                    },
                                    {
                                        "@id": "#Sample_Cultivation_flask",
                                        "@type": [
                                            "Sample"
                                        ],
                                        "additionalType": "Sample",
                                        "name": "Cultivation flask"
                                    },
                                    {
                                        "@id": "#Sample_Cultivation_flask",
                                        "@type": [
                                            "Sample"
                                        ],
                                        "additionalType": "Sample",
                                        "name": "Cultivation flask"
                                    }
                                ],
                                "outputs": [
                                    {
                                        "@id": "#Sample_sample_eppi_1",
                                        "@type": [
                                            "Sample"
                                        ],
                                        "additionalType": "Sample",
                                        "name": "sample eppi 1"
                                    },
                                    {
                                        "@id": "#Sample_sample_eppi_2",
                                        "@type": [
                                            "Sample"
                                        ],
                                        "additionalType": "Sample",
                                        "name": "sample eppi 2"
                                    },
                                    {
                                        "@id": "#Sample_sample_eppi_3",
                                        "@type": [
                                            "Sample"
                                        ],
                                        "additionalType": "Sample",
                                        "name": "sample eppi 3"
                                    },
                                    {
                                        "@id": "#Sample_sample_eppi_4",
                                        "@type": [
                                            "Sample"
                                        ],
                                        "additionalType": "Sample",
                                        "name": "sample eppi 4"
                                    },
                                    {
                                        "@id": "#Sample_sample_eppi_5",
                                        "@type": [
                                            "Sample"
                                        ],
                                        "additionalType": "Sample",
                                        "name": "sample eppi 5"
                                    },
                                    {
                                        "@id": "#Sample_sample_eppi_6",
                                        "@type": [
                                            "Sample"
                                        ],
                                        "additionalType": "Sample",
                                        "name": "sample eppi 6"
                                    },
                                    {
                                        "@id": "#Sample_sample_eppi_7",
                                        "@type": [
                                            "Sample"
                                        ],
                                        "additionalType": "Sample",
                                        "name": "sample eppi 7"
                                    }
                                ]
                            }"""

let ldo = LDObject.fromROCrateJsonString arc
let ldo2 = LDObject.fromROCrateJsonString arc

LDObject.tryGetContext() ldo

let lp = LabProcess.fromROCrateJsonString arc


type LabProcess with

    // already given: .GetAgent, .GetName, .GetResult, .GetObject

    member this.TryGetAgentAs<'T>() = 
        tryGetTypedPropertyValue<'T> "agent" this

    member this.GetAgentAs<'T>() =
        this.TryGetAgentAs<'T>().Value

    member this.TryGetAgentAsString() =
        this.TryGetAgentAs<string>()

    member this.GetAgentAsString() =
        this.TryGetAgentAsString()

    static member tryGetAgentAs<'T> (lp : LabProcess) = 
        lp.TryGetAgentAs<'T>()

    static member getAgentAs<'T> (lp : LabProcess) = 
        lp.GetAgentAs<'T>()

    /// 
    member this.TryGetParameterValues() =
        tryGetTypedPropertyValue<PropertyValue list> "parameterValues" this

    member this.GetParameterValues() =
        this.TryGetParameterValues().Value

    static member tryGetParameterValues (lp : LabProcess) = 
        lp.TryGetParameterValues()

    static member getParameterValues (lp : LabProcess) = 
        lp.GetParameterValues()

    member this.TryGetAdditionalType() =
        tryGetTypedPropertyValue<string> "additionalType" this

    member this.GetAdditionalType() =
        this.TryGetAdditionalType().Value

    static member tryGetAdditionalType (lp : LabProcess) = 
        lp.TryGetAdditionalType()

    static member getAdditionalType (lp : LabProcess) = 
        lp.GetAdditionalType()

    member this.TryGetExecutesLabProtocol() =
        tryGetTypedPropertyValue<LabProtocol> "executesLabProtocol" this 

    member this.GetExecutesLabProtocol() =
        this.TryGetExecutesLabProtocol().Value

    static member tryGetExecutesLabProtocol (lp : LabProcess) = 
        lp.TryGetExecutesLabProtocol()

    static member getExecutesLabProtocol (lp : LabProcess) = 
        lp.GetExecutesLabProtocol()

    member this.TryGetEndTime() =
        tryGetTypedPropertyValue<System.DateTime> "endTime" this

    member this.GetEndTime() =
        this.TryGetEndTime().Value

    static member tryGetEndTime (lp : LabProcess) = 
        lp.TryGetEndTime()

    static member getEndTime (lp : LabProcess) = 
        lp.GetEndTime()


type Person with

    // already given: `.GetGivenName`

    // ROCrate | ISA
    // id = id
    // givenName = firstName
    // familyName = lastName
    // email = email
    // identifier = ? (not assigned in ISA)
    // affiliation = affiliation
    // NB: is `id` here and below in every case only needed programmatically but does NOT occur in the original annotation table?

    /// Returns the familyName of the Person if it exists. Else returns None. This corresponds to `lastName` in ISA.
    member this.TryGetFamilyName() =
        tryGetTypedPropertyValue<string> "familyName" this

    /// Returns the familyName of the Person. This corresponds to `lastName` in ISA.
    member this.GetFamilyName() =
        this.TryGetFamilyName().Value

    /// Returns the familyName of the given Person if it exists. Else returns None. This corresponds to `lastName` in ISA.
    static member tryGetFamilyName (person : Person) = 
        person.TryGetFamilyName()

    /// Returns the familyName of the given Person. This corresponds to `lastName` in ISA.
    static member getFamilyName (person : Person) = 
        person.GetFamilyName()

    /// Returns the additionalName of the Person if it exists. Else returns None. This corresponds to `midInitials` in ISA.
    member this.TryGetAdditionalName() =
        tryGetTypedPropertyValue<string> "additionalName" this

    /// Returns the additionalName of the Person. This corresponds to `midInitials` in ISA.
    member this.GetAdditionalName() =
        this.TryGetAdditionalName().Value

    /// Returns the additionalName of the given Person if it exists. Else returns None. This corresponds to `midInitials` in ISA.
    static member tryGetAdditionalName (person : Person) = 
        person.TryGetAdditionalName()

    /// Returns the additionalName of the given Person. This corresponds to `midInitials` in ISA.
    static member getAdditionalName (person : Person) = 
        person.GetAdditionalName()

    member this.TryGetJobTitle() =
        tryGetTypedPropertyValue<string> "jobTitle" this

    member this.GetJobTitle() =
        this.TryGetJobTitle().Value

    static member tryGetJobTitle (person : Person) = 
        person.TryGetJobTitle()

    static member getJobTitle (person : Person) = 
        person.GetJobTitle()

    member this.TryGetEmail() =
        tryGetTypedPropertyValue<string> "email" this

    member this.GetEmail() =
        this.TryGetEmail().Value

    static member tryGetEmail (person : Person) = 
        person.TryGetEmail()

    static member getEmail (person : Person) = 
        person.GetEmail()

    member this.TryGetAffiliation() =
        tryGetTypedPropertyValue<string> "affiliation" this

    member this.GetAffiliation() =
        this.TryGetAffiliation().Value

    static member tryGetAffiliation (person : Person) = 
        person.TryGetAffiliation()

    static member getAffiliation (person : Person) = 
        person.GetAffiliation()

    member this.TryGetAddress() =
        tryGetTypedPropertyValue<string> "address" this

    member this.GetAddress() =
        this.TryGetAddress().Value

    static member tryGetAddress (person : Person) = 
        person.TryGetAddress()

    static member getAddress (person : Person) = 
        person.GetAddress()

    member this.TryGetTelephone() =
        tryGetTypedPropertyValue<string> "telephone" this

    member this.GetTelephone() =
        this.TryGetTelephone().Value

    static member tryGetTelephone (person : Person) = 
        person.TryGetTelephone()

    static member getTelephone (person : Person) = 
        person.GetTelephone()

    member this.TryGetFaxNumber() =
        tryGetTypedPropertyValue<string> "faxNumber" this

    member this.GetFaxNumber() =
        this.TryGetFaxNumber().Value

    static member tryGetFaxNumber (person : Person) = 
        person.TryGetFaxNumber() 

    static member getFaxNumber (person : Person) = 
        person.GetFaxNumber()


type Sample with

    // already given: .GetName

    member this.TryGetAdditionalType() =
        tryGetTypedPropertyValue<string> "additionalType" this

    member this.GetAdditionalType() =
        this.TryGetAdditionalType().Value

    static member tryGetAdditionalType (sample : Sample) = 
        sample.TryGetAdditionalType()

    static member getAdditionalType (sample : Sample) = 
        sample.GetAdditionalType()

    member this.TryGetAdditionalProperty() =
        tryGetTypedPropertyValue<PropertyValue> "additionalProperty" this

    member this.GetAdditionalProperty() =
        this.TryGetAdditionalProperty().Value

    static member tryGetAdditionalProperty (sample : Sample) = 
        sample.TryGetAdditionalProperty()

    static member getAdditionalProperty (sample : Sample) = 
        sample.GetAdditionalProperty()


type PropertyValue with

    // already given: `.GetName`, `.GetValue`

    member this.TryGetUnit() =
        tryGetTypedPropertyValue<string> "unit" this

    member this.GetUnit() =
        this.TryGetUnit().Value

    static member tryGetUnit (propertyValue : PropertyValue) = 
        propertyValue.TryGetUnit()

    static member getUnit (propertyValue : PropertyValue) = 
        propertyValue.GetUnit()

    member this.TryGetUnitCode() =
        tryGetTypedPropertyValue<string> "unitCode" this

    member this.GetUnitCode() =
        this.TryGetUnitCode().Value

    static member tryGetUnitCode (propertyValue : PropertyValue) = 
        propertyValue.TryGetUnitCode()

    static member getUnitCode (propertyValue : PropertyValue) = 
        propertyValue.GetUnitCode()

    member this.TryGetValueReference() =
        tryGetTypedPropertyValue<string> "valueReference" this

    member this.GetValueReference() =
        this.TryGetValueReference().Value

    static member tryGetValueReference (propertyValue : PropertyValue) = 
        propertyValue.TryGetValueReference()

    static member getValueReference (propertyValue : PropertyValue) = 
        propertyValue.GetValueReference()

    member this.TryGetAdditionalType() =
        tryGetTypedPropertyValue<string> "additionalType" this

    member this.GetAdditionalType() =
        this.TryGetAdditionalType().Value

    static member tryGetAdditionalType (propertyValue : PropertyValue) = 
        propertyValue.TryGetAdditionalType()

    static member getAdditionalType (propertyValue : PropertyValue) = 
        propertyValue.GetAdditionalType()

    member this.TryGetPropertyID() =
        tryGetTypedPropertyValue<string> "propertyID" this

    member this.GetPropertyID() =
        this.TryGetPropertyID().Value

    static member tryGetPropertyID (propertyValue : PropertyValue) = 
        propertyValue.TryGetPropertyID()

    static member getPropertyID (propertyValue : PropertyValue) = 
        propertyValue.GetPropertyID()


type Assay with

    // already given: .GetIdentifier

    member this.TryGetAbout() =
        tryGetTypedPropertyValue<LabProcess> "about" this

    member this.GetAbout() =
        this.TryGetAbout().Value

    static member tryGetAbout (assay : Assay) = 
        assay.TryGetAbout()

    static member getAbout (assay : Assay) = 
        assay.GetAbout()

    member this.TryGetHasPart() =
        tryGetTypedPropertyValue<string seq> "hasPart" this

    member this.GetHasPart() =
        this.TryGetHasPart().Value

    static member tryGetHasPart (assay : Assay) = 
        assay.TryGetHasPart()

    static member getHasPart (assay : Assay) = 
        assay.GetHasPart()

    member this.TryGetMeasurementMethod() =
        tryGetTypedPropertyValue<string> "measurementMethod" this

    member this.GetMeasurementMethod() =
        this.TryGetMeasurementMethod().Value

    static member tryGetMeasurementMethod (assay : Assay) = 
        assay.TryGetMeasurementMethod()

    static member getMeasurementMethod (assay : Assay) = 
        assay.GetMeasurementMethod()

    member this.TryGetMeasurementTechnique() =
        tryGetTypedPropertyValue<string> "measurementTechnique" this

    member this.GetMeasurementTechnique() =
        this.TryGetMeasurementTechnique().Value

    static member tryGetMeasurementTechnique (assay : Assay) = 
        assay.TryGetMeasurementTechnique()

    static member getMeasurementTechnique (assay : Assay) = 
        assay.GetMeasurementTechnique()

    member this.TryGetUrl() =
        tryGetTypedPropertyValue<string> "url" this

    member this.GetUrl() =
        this.TryGetUrl().Value

    static member tryGetUrl (assay : Assay) = 
        assay.TryGetUrl()

    static member getUrl (assay : Assay) = 
        assay.GetUrl()

    member this.TryGetVariableMeasured() =
        tryGetTypedPropertyValue<string> "variableMeasured" this

    member this.GetVariableMeasured() =
        this.TryGetVariableMeasured().Value

    static member tryGetVariableMeasured (assay : Assay) = 
        assay.TryGetVariableMeasured()

    static member getVariableMeasured (assay : Assay) = 
        assay.GetVariableMeasured()


module Tokenization =

    /// Takes a LabProcess and returns its content tokenized as a sequence of CvParams (where each CvParam represents one property of the process).
    let ofLabProcess (labProcess : LabProcess) : CvParam seq =
        labProcess.GetAdditionalType
        |> Seq.map (
            fun processUnit -> 
                let puLdo = LDObject.fromROCrateJsonString (string processUnit)
                CvParam(puLdo.Id, puLdo.)
        )




module Validate =

    module Check =

        /// Checks if a collection of CvParams contains any CvParams that match the projection.
        let containsAnyOfBy projection (cvParams : CvParam seq) =
            cvParams |> Seq.exists projection

        /// Checks if a collection of CvParams contain an expected CvParam.
        let containsAnyOf expected (cvParams : CvParam seq) =
            containsAnyOfBy ((=) expected) cvParams

    /// Lets the unit test fail if actual is not true with given error message.
    // TO DO: in the future: put in CvParam information of missing or wrong stuff, i.e. cell position(s), filepath etc.; HLW might expand the ROCrate JSON model at that point  a bit to allow for this information to be attached as, e.g., property values
    let isTrue actual errorMessage =
        if actual then ()
        else Expecto.Tests.failtestNoStackf errorMessage


module Toys =

    // created in reference to https://github.com/nfdi4plants/isa-ro-crate-profile/blob/release/profile/isa_ro_crate.md and https://github.com/nfdi4plants/isa-ro-crate-profile/blob/release/profile/isa_ro_crate_mapping.md

    // ROCrate | ISA
    // id = id
    // givenName = firstName
    // familyName = lastName
    // email = email
    // identifier = ? (not assigned in ISA)
    // affiliation = affiliation
    // NB: is `id` here and below in every case only needed programmatically but does NOT occur in the original annotation table?
    let person = ROCrate.Person("id1", "Oliver", familyName = "Maus", email = "maus@nfdi4plants.org", identifier = "id1", affiliation = "RPTU Kaiserslautern")

    // ROCrate | ISA
    // id = id
    // name = key name (would probably be the name of the header term)
    // value = text or number (as string) (text for terms and freetext, number for values and freetext when only digits)
    // propertyID = category (key ontology reference, i.e. TermSourceRef (TSR) I think)
    // additionalType = freetext (?) if it's Characteristics, Parameter, Factor, or Component (NB: is it standardized?)
    // unitCode = unit ontology ref (again TSR I guess)
    // unitText = unit name (i.e., unit term name)
    // valueReference = value ontology reference (TSR?)
    // IMPORTANT: TSR could always also be TAN (TermAccessionNumber) – clarify with HLW, FW (Florian Wetzels), and KS (Kevin Schneider) because it's a mess right now
    // TAN would make more sense to me since you can always determine the TSR from the TAN but not vice versa – discuss with people
    let characteristics = PropertyValue("char1", "SourceCharacteristics", "myCharacteristics", propertyID = "DBPO", additionalType = "Characteristics")

    // ROCrate | ISA
    // id = id
    // name = identifier
    // additionalProperty = either Characteristics or Factor in the form of propertyValue, see above: `characteristics`
    // derivesFrom = (seems to be redundant)
    // additionalType = ? (not mentioned in profile and mapping)
    let inputs = Sample("source1", "source1", additionalProperty = characteristics)

    // see above: `inputs` annotation
    // IMPORTANT: Ask people where Characteristics and Factor should be applied to: to the inputs or to the outputs or both?
    let outputs = Sample("sample1", "sample1", additionalProperty = characteristics)

    // see above: `characteristics` annotation
    let parameter = PropertyValue("param1", "SampleParameters", "myParameter", propertyID = "DBPO", additionalType = "Parameter")

    // ROCrate | ISA
    // id = id
    // name = name
    // agent = Performer (i.e., a Person)
    // endTime = date
    // executesLabProtocol = executesProtocol
    // parameterValue = parameterValues (list of Parameters in the form of propertyValues, see above: `parameter`)
    let labProcess = LabProcess("id1", "id1", person, object = [inputs], result = [outputs], parameterValue = [parameter])

    // ROCrate | ISA
    // id = id
    // name = identifier
    // about = Process (i.e., 1 single ISA Process, NOT the whole ProcessSequence. In the annotation table this correlates to 1 row (while the ProcessSequence correlates to ALL rows))
    // measurementMethod = Technology Type
    // measurementTechnique = Technology Platform
    // variableMeasured = Measurement Type
    // hasPart = Data Files
    // url = fileName
    let assay = ROCrate.Assay("assayID", "assayID", about = labProcess)



// testings

// replace this later as soon as reading and parsing an ARC to the ROCrate JSON model representation is available:
//let studyMetadata =
//    Study.parseMetadataSheetsFromTokens() arcDir
//    |> List.concat


let ch