#r "nuget: ARCtrl"
//#r "nuget: ARCExpect"
#r "nuget: ControlledVocabulary"
#r "nuget: Expecto"
#r "nuget: ARCTokenization"


open ARCtrl
//open ARCExpect
open ControlledVocabulary
open Expecto
open ARCTokenization
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

    /// Returns the agent of the LabProcess in the given type if it exists. Else returns None. This corresponds to `performer` in ISA.
    member this.TryGetAgentAs<'T>() = 
        tryGetTypedPropertyValue<'T> "agent" this

    /// Returns the agent of the LabProcess in the given type. This corresponds to `performer` in ISA.
    member this.GetAgentAs<'T>() =
        this.TryGetAgentAs<'T>().Value

    /// Returns the agent of the LabProcess as a string if it exists. Else returns None. This corresponds to `performer` in ISA.
    member this.TryGetAgentAsString() =
        this.TryGetAgentAs<string>()

    /// Returns the agent of the LabProcess as a string. This corresponds to `performer` in ISA.
    member this.GetAgentAsString() =
        this.TryGetAgentAsString()

    /// Returns the agent of the given LabProcess in the given type if it exists. Else returns None. This corresponds to `performer` in ISA.
    static member tryGetAgentAs<'T> (lp : LabProcess) = 
        lp.TryGetAgentAs<'T>()

    /// Returns the agent of the given LabProcess. This corresponds to `performer` in ISA.
    static member getAgentAs<'T> (lp : LabProcess) = 
        lp.GetAgentAs<'T>()

    /// Returns the parameterValue of the LabProcess if it exists. Else returns None. This corresponds to `parameterValues` in ISA.
    member this.TryGetParameterValue() =
        tryGetTypedPropertyValue<PropertyValue list> "parameterValue" this

    /// Returns the parameterValue of the LabProcess. This corresponds to `parameterValues` in ISA.
    member this.GetParameterValue() =
        this.TryGetParameterValue().Value

    /// Returns the parameterValue of the given LabProcess if it exists. Else returns None. This corresponds to `parameterValues` in ISA.
    static member tryGetParameterValues (lp : LabProcess) = 
        lp.TryGetParameterValue()

    /// Returns the parameterValue of the given LabProcess. This corresponds to `parameterValues` in ISA.
    static member getParameterValues (lp : LabProcess) = 
        lp.GetParameterValue()

    /// Returns the additionalType of the LabProcess if it exists. Else returns None. There is no correspondent term in ISA.
    member this.TryGetAdditionalType() =
        tryGetTypedPropertyValue<string> "additionalType" this

    /// Returns the additionalType of the LabProcess. There is no correspondent term in ISA.
    member this.GetAdditionalType() =
        this.TryGetAdditionalType().Value

    /// Returns the additionalType of the given LabProcess if it exists. Else returns None. There is no correspondent term in ISA.
    static member tryGetAdditionalType (lp : LabProcess) = 
        lp.TryGetAdditionalType()

    /// Returns the additionalType of the given LabProcess. There is no correspondent term in ISA.
    static member getAdditionalType (lp : LabProcess) = 
        lp.GetAdditionalType()

    /// Returns the executesLabProtocol of the LabProcess if it exists. Else returns None. This corresponds to `executesProtocol` in ISA.
    member this.TryGetExecutesLabProtocol() =
        tryGetTypedPropertyValue<LabProtocol> "executesLabProtocol" this 

    /// Returns the executesLabProtocol of the LabProcess. This corresponds to `executesProtocol` in ISA.
    member this.GetExecutesLabProtocol() =
        this.TryGetExecutesLabProtocol().Value

    /// Returns the executesLabProtocol of the given LabProcess if it exists. Else returns None. This corresponds to `executesProtocol` in ISA.
    static member tryGetExecutesLabProtocol (lp : LabProcess) = 
        lp.TryGetExecutesLabProtocol()

    /// Returns the executesLabProtocol of the given LabProcess. This corresponds to `executesProtocol` in ISA.
    static member getExecutesLabProtocol (lp : LabProcess) = 
        lp.GetExecutesLabProtocol()

    /// Returns the endTime of the LabProcess if it exists. Else returns None. This corresponds to `date` in ISA.
    member this.TryGetEndTime() =
        tryGetTypedPropertyValue<System.DateTime> "endTime" this

    /// Returns the endTime of the LabProcess. This corresponds to `date` in ISA.
    member this.GetEndTime() =
        this.TryGetEndTime().Value

    /// Returns the endTime of the given LabProcess if it exists. Else returns None. This corresponds to `date` in ISA.
    static member tryGetEndTime (lp : LabProcess) = 
        lp.TryGetEndTime()

    /// Returns the endTime of the given LabProcess. This corresponds to `date` in ISA.
    static member getEndTime (lp : LabProcess) = 
        lp.GetEndTime()

    /// Returns the object of the LabProcess in the form of type Sample if it exists. Else returns None. This corresponds to `Source` or `Sample` (in this case: `Source`) in ISA.
    member this.TryGetObjectAsSample() =
        tryGetTypedPropertyValue<Sample> "object" this

    /// Returns the object of the LabProcess in the form of type Sample. This corresponds to `Source` or `Sample` (in this case: `Source`) in ISA.
    member this.GetObjectAsSample() =
        this.TryGetObjectAsSample().Value

    /// Returns the object of the given LabProcess in the form of type Sample if it exists. Else returns None. This corresponds to `Source` or `Sample` (in this case: `Source`) in ISA.
    static member tryGetObjectAsSample (labProcess : LabProcess) =
        labProcess.TryGetObjectAsSample()

    /// Returns the object of the given LabProcess in the form of type Sample. This corresponds to `Source` or `Sample` (in this case: `Source`) in ISA.
    static member getObjectAsSample (labProcess : LabProcess) =
        labProcess.GetObjectAsSample()

    /// Returns the result of the LabProcess in the form of type Sample if it exists. Else returns None. This corresponds to `Source` or `Sample` (in this case: `Sample`) in ISA.
    member this.TryGetResultAsSample() =
        tryGetTypedPropertyValue<Sample> "result" this

    /// Returns the result of the LabProcess in the form of type Sample. This corresponds to `Source` or `Sample` (in this case: `Sample`) in ISA.
    member this.GetResultAsSample() =
        this.TryGetResultAsSample().Value

    /// Returns the result of the given LabProcess in the form of type Sample if it exists. Else returns None. This corresponds to `Source` or `Sample` (in this case: `Sample`) in ISA.
    static member tryGetResultAsSample (labProcess : LabProcess) =
        labProcess.TryGetResultAsSample()

    /// Returns the result of the given LabProcess in the form of type Sample. This corresponds to `Source` or `Sample` (in this case: `Sample`) in ISA.
    static member getResultAsSample (labProcess : LabProcess) =
        labProcess.GetResultAsSample()


type Person with

    // already given: `.GetGivenName`

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

    /// Returns the jobTitle of the Person if it exists. Else returns None. This corresponds to `roles` in ISA.
    member this.TryGetJobTitle() =
        tryGetTypedPropertyValue<string> "jobTitle" this

    /// Returns the jobTitle of the Person. This corresponds to `roles` in ISA.
    member this.GetJobTitle() =
        this.TryGetJobTitle().Value

    /// Returns the jobTitle of the given Person if it exists. Else returns None. This corresponds to `roles` in ISA.
    static member tryGetJobTitle (person : Person) = 
        person.TryGetJobTitle()

    /// Returns the jobTitle of the given Person. This corresponds to `roles` in ISA.
    static member getJobTitle (person : Person) = 
        person.GetJobTitle()

    /// Returns the email of the Person if it exists. Else returns None. This corresponds to `email` in ISA.
    member this.TryGetEmail() =
        tryGetTypedPropertyValue<string> "email" this

    /// Returns the email of the Person. This corresponds to `email` in ISA.
    member this.GetEmail() =
        this.TryGetEmail().Value

    /// Returns the email of the given Person if it exists. Else returns None. This corresponds to `email` in ISA.
    static member tryGetEmail (person : Person) = 
        person.TryGetEmail()

    /// Returns the email of the given Person. This corresponds to `email` in ISA.
    static member getEmail (person : Person) = 
        person.GetEmail()

    /// Returns the affiliation of the Person if it exists. Else returns None. This corresponds to `affiliation` in ISA.
    member this.TryGetAffiliation() =
        tryGetTypedPropertyValue<string> "affiliation" this

    /// Returns the affiliation of the Person. This corresponds to `affiliation` in ISA.
    member this.GetAffiliation() =
        this.TryGetAffiliation().Value

    /// Returns the affiliation of the given Person if it exists. Else returns None. This corresponds to `affiliation` in ISA.
    static member tryGetAffiliation (person : Person) = 
        person.TryGetAffiliation()

    /// Returns the affiliation of the given Person. This corresponds to `affiliation` in ISA.
    static member getAffiliation (person : Person) = 
        person.GetAffiliation()

    /// Returns the address of the Person if it exists. Else returns None. This corresponds to `address` in ISA.
    member this.TryGetAddress() =
        tryGetTypedPropertyValue<string> "address" this

    /// Returns the address of the Person. This corresponds to `address` in ISA.
    member this.GetAddress() =
        this.TryGetAddress().Value

    /// Returns the address of the given Person if it exists. Else returns None. This corresponds to `address` in ISA.
    static member tryGetAddress (person : Person) = 
        person.TryGetAddress()

    /// Returns the address of the given Person. This corresponds to `address` in ISA.
    static member getAddress (person : Person) = 
        person.GetAddress()

    /// Returns the telephone of the Person if it exists. Else returns None. This corresponds to `phone` in ISA.
    member this.TryGetTelephone() =
        tryGetTypedPropertyValue<string> "telephone" this

    /// Returns the telephone of the Person. This corresponds to `phone` in ISA.
    member this.GetTelephone() =
        this.TryGetTelephone().Value

    /// Returns the telephone of the given Person if it exists. Else returns None. This corresponds to `phone` in ISA.
    static member tryGetTelephone (person : Person) = 
        person.TryGetTelephone()

    /// Returns the telephone of the given Person. This corresponds to `phone` in ISA.
    static member getTelephone (person : Person) = 
        person.GetTelephone()

    /// Returns the fax of the Person if it exists. Else returns None. This corresponds to `faxNumber` in ISA.
    member this.TryGetFaxNumber() =
        tryGetTypedPropertyValue<string> "faxNumber" this

    /// Returns the fax of the Person. This corresponds to `faxNumber` in ISA.
    member this.GetFaxNumber() =
        this.TryGetFaxNumber().Value

    /// Returns the fax of the given Person if it exists. Else returns None. This corresponds to `faxNumber` in ISA.
    static member tryGetFaxNumber (person : Person) = 
        person.TryGetFaxNumber() 

    /// Returns the fax of the given Person. This corresponds to `faxNumber` in ISA.
    static member getFaxNumber (person : Person) = 
        person.GetFaxNumber()


type Sample with

    // already given: .GetName

    /// Returns the additionalType of the Sample if it exists. Else returns None. There is no corresponding term in ISA.
    member this.TryGetAdditionalType() =
        tryGetTypedPropertyValue<string> "additionalType" this

    /// Returns the additionalType of the Sample. There is no corresponding term in ISA.
    member this.GetAdditionalType() =
        this.TryGetAdditionalType().Value

    /// Returns the additionalType of the given Sample if it exists. Else returns None. There is no corresponding term in ISA.
    static member tryGetAdditionalType (sample : Sample) = 
        sample.TryGetAdditionalType()

    /// Returns the additionalType of the given Sample. There is no corresponding term in ISA.
    static member getAdditionalType (sample : Sample) = 
        sample.GetAdditionalType()

    /// Returns the additionalProperty of the Sample if it exists. Else returns None. This corresponds to `characteristics` or `factor` in ISA.
    member this.TryGetAdditionalProperty() =
        tryGetTypedPropertyValue<PropertyValue seq> "additionalProperty" this

    /// Returns the additionalProperty of the Sample. This corresponds to `characteristics` or `factor` in ISA.
    member this.GetAdditionalProperty() =
        this.TryGetAdditionalProperty().Value

    /// Returns the additionalProperty of the given Sample if it exists. Else returns None. This corresponds to `characteristics` or `factor` in ISA.
    static member tryGetAdditionalProperty (sample : Sample) = 
        sample.TryGetAdditionalProperty()

    /// Returns the additionalProperty of the given Sample. This corresponds to `characteristics` or `factor` in ISA.
    static member getAdditionalProperty (sample : Sample) = 
        sample.GetAdditionalProperty()


type PropertyValue with

    // already given: `.GetName`, `.GetValue`

    /// Returns the unitText of the PropertyValue if it exists. Else returns None. This, together with `unitCode` corresponds to `unit` in ISA.
    member this.TryGetUnitText() =
        tryGetTypedPropertyValue<string> "unitText" this

    /// Returns the unitText of the PropertyValue. This, together with `unitCode` corresponds to `unit` in ISA.
    member this.GetUnitText() =
        this.TryGetUnitText().Value

    /// Returns the unitText of the given PropertyValue if it exists. Else returns None. This, together with `unitCode` corresponds to `unit` in ISA.
    static member tryGetUnitText (propertyValue : PropertyValue) = 
        propertyValue.TryGetUnitText()

    /// Returns the unitText of the given PropertyValue. This, together with `unitCode` corresponds to `unit` in ISA.
    static member getUnitText (propertyValue : PropertyValue) = 
        propertyValue.GetUnitText()

    /// Returns the unitCode of the PropertyValue if it exists. Else returns None. This, together with `unitText` corresponds to `unit` in ISA.
    member this.TryGetUnitCode() =
        tryGetTypedPropertyValue<string> "unitCode" this

    /// Returns the unitCode of the PropertyValue. This, together with `unitText` corresponds to `unit` in ISA.
    member this.GetUnitCode() =
        this.TryGetUnitCode().Value

    /// Returns the unitCode of the given PropertyValue if it exists. Else returns None. This, together with `unitText` corresponds to `unit` in ISA.
    static member tryGetUnitCode (propertyValue : PropertyValue) = 
        propertyValue.TryGetUnitCode()

    /// Returns the unitCode of the given PropertyValue. This, together with `unitText` corresponds to `unit` in ISA.
    static member getUnitCode (propertyValue : PropertyValue) = 
        propertyValue.GetUnitCode()

    /// Returns the valueReference of the PropertyValue if it exists. Else returns None. This corresponds to `value` in ISA.
    member this.TryGetValueReference() =
        tryGetTypedPropertyValue<string> "valueReference" this

    /// Returns the valueReference of the PropertyValue. This corresponds to `value` in ISA.
    member this.GetValueReference() =
        this.TryGetValueReference().Value

    /// Returns the valueReference of the given PropertyValue if it exists. Else returns None. This corresponds to `value` in ISA.
    static member tryGetValueReference (propertyValue : PropertyValue) = 
        propertyValue.TryGetValueReference()

    /// Returns the valueReference of the given PropertyValue. This corresponds to `value` in ISA.
    static member getValueReference (propertyValue : PropertyValue) = 
        propertyValue.GetValueReference()

    /// Returns the valueReference of the PropertyValue if it exists. Else returns None. This corresponds to `value` in ISA.
    member this.TryGetValueReferenceAsCvTerm() =
        tryGetTypedPropertyValue<CvTerm> "valueReference" this

    /// Returns the valueReference of the PropertyValue. This corresponds to `value` in ISA.
    member this.GetValueReferenceAsCvTerm() =
        this.TryGetValueReferenceAsCvTerm().Value

    /// Returns the valueReference of the given PropertyValue if it exists. Else returns None. This corresponds to `value` in ISA.
    static member tryGetValueReferenceAsCvTerm (propertyValue : PropertyValue) = 
        propertyValue.TryGetValueReferenceAsCvTerm()

    /// Returns the valueReference of the given PropertyValue. This corresponds to `value` in ISA.
    static member getValueReferenceAsCvTerm (propertyValue : PropertyValue) = 
        propertyValue.GetValueReferenceAsCvTerm()

    /// Returns the additionalType of the PropertyValue if it exists. Else returns None. There is no corresponding term in ISA.
    member this.TryGetAdditionalType() =
        tryGetTypedPropertyValue<string> "additionalType" this

    /// Returns the additionalType of the PropertyValue. There is no corresponding term in ISA.
    member this.GetAdditionalType() =
        this.TryGetAdditionalType().Value

    /// Returns the additionalType of the given PropertyValue if it exists. Else returns None. There is no corresponding term in ISA.
    static member tryGetAdditionalType (propertyValue : PropertyValue) = 
        propertyValue.TryGetAdditionalType()

    /// Returns the additionalType of the given PropertyValue. There is no corresponding term in ISA.
    static member getAdditionalType (propertyValue : PropertyValue) = 
        propertyValue.GetAdditionalType()

    /// Returns the propertyID of the PropertyValue if it exists. Else returns None. This corresponds to `category` in ISA.
    member this.TryGetPropertyID() =
        tryGetTypedPropertyValue<string> "propertyID" this

    /// Returns the propertyID of the PropertyValue. This corresponds to `category` in ISA.
    member this.GetPropertyID() =
        this.TryGetPropertyID().Value

    /// Returns the propertyID of the given PropertyValue if it exists. Else returns None. This corresponds to `category` in ISA.
    static member tryGetPropertyID (propertyValue : PropertyValue) = 
        propertyValue.TryGetPropertyID()

    /// Returns the propertyID of the given PropertyValue. This corresponds to `category` in ISA.
    static member getPropertyID (propertyValue : PropertyValue) = 
        propertyValue.GetPropertyID()


type Assay with

    // already given: .GetIdentifier

    /// Returns the about of the Assay if it exists. Else returns None. This corresponds to `processSequence` in ISA.
    member this.TryGetAbout() =
        tryGetTypedPropertyValue<LabProcess seq> "about" this

    /// Returns the about of the Assay. This corresponds to `processSequence` in ISA.
    member this.GetAbout() =
        this.TryGetAbout().Value

    /// Returns the about of the given Assay if it exists. Else returns None. This corresponds to `processSequence` in ISA.
    static member tryGetAbout (assay : Assay) = 
        assay.TryGetAbout()

    /// Returns the about of the given Assay. This corresponds to `processSequence` in ISA.
    static member getAbout (assay : Assay) = 
        assay.GetAbout()

    /// Returns the hasPart of the Assay if it exists. Else returns None. This corresponds to `dataFiles` in ISA.
    member this.TryGetHasPart() =
        tryGetTypedPropertyValue<string seq> "hasPart" this

    /// Returns the hasPart of the Assay. This corresponds to `dataFiles` in ISA.
    member this.GetHasPart() =
        this.TryGetHasPart().Value

    /// Returns the hasPart of the given Assay if it exists. Else returns None. This corresponds to `dataFiles` in ISA.
    static member tryGetHasPart (assay : Assay) = 
        assay.TryGetHasPart()

    /// Returns the hasPart of the given Assay. This corresponds to `dataFiles` in ISA.
    static member getHasPart (assay : Assay) = 
        assay.GetHasPart()

    /// Returns the measurementMethod of the Assay if it exists. Else returns None. This corresponds to `technologyType` in ISA.
    member this.TryGetMeasurementMethod() =
        tryGetTypedPropertyValue<string> "measurementMethod" this

    /// Returns the measurementMethod of the Assay. This corresponds to `technologyType` in ISA.
    member this.GetMeasurementMethod() =
        this.TryGetMeasurementMethod().Value

    /// Returns the measurementMethod of the given Assay if it exists. Else returns None. This corresponds to `technologyType` in ISA.
    static member tryGetMeasurementMethod (assay : Assay) = 
        assay.TryGetMeasurementMethod()

    /// Returns the measurementMethod of the given Assay. This corresponds to `technologyType` in ISA.
    static member getMeasurementMethod (assay : Assay) = 
        assay.GetMeasurementMethod()

    /// Returns the measurementTechnique of the Assay if it exists. Else returns None. This corresponds to `technologyPlatform` in ISA.
    member this.TryGetMeasurementTechnique() =
        tryGetTypedPropertyValue<string> "measurementTechnique" this

/// Returns the measurementTechnique of the Assay. This corresponds to `technologyPlatform` in ISA.
    member this.GetMeasurementTechnique() =
        this.TryGetMeasurementTechnique().Value

/// Returns the measurementTechnique of the given Assay if it exists. Else returns None. This corresponds to `technologyPlatform` in ISA.
    static member tryGetMeasurementTechnique (assay : Assay) = 
        assay.TryGetMeasurementTechnique()

/// Returns the measurementTechnique of the given Assay. This corresponds to `technologyPlatform` in ISA.
    static member getMeasurementTechnique (assay : Assay) = 
        assay.GetMeasurementTechnique()

    /// Returns the url of the Assay if it exists. Else returns None. This corresponds to `fileName` in ISA.
    member this.TryGetUrl() =
        tryGetTypedPropertyValue<string> "url" this

    /// Returns the url of the Assay. This corresponds to `fileName` in ISA.
    member this.GetUrl() =
        this.TryGetUrl().Value

    /// Returns the url of the given Assay if it exists. Else returns None. This corresponds to `fileName` in ISA.
    static member tryGetUrl (assay : Assay) = 
        assay.TryGetUrl()

    /// Returns the url of the given Assay. This corresponds to `fileName` in ISA.
    static member getUrl (assay : Assay) = 
        assay.GetUrl()

    /// Returns the variableMeasured of the Assay if it exists. Else returns None. This corresponds to `measurementType` in ISA.
    member this.TryGetVariableMeasured() =
        tryGetTypedPropertyValue<string> "variableMeasured" this

    /// Returns the variableMeasured of the Assay. This corresponds to `measurementType` in ISA.
    member this.GetVariableMeasured() =
        this.TryGetVariableMeasured().Value

    /// Returns the variableMeasured of the given Assay if it exists. Else returns None. This corresponds to `measurementType` in ISA.
    static member tryGetVariableMeasured (assay : Assay) = 
        assay.TryGetVariableMeasured()

    /// Returns the variableMeasured of the given Assay. This corresponds to `measurementType` in ISA.
    static member getVariableMeasured (assay : Assay) = 
        assay.GetVariableMeasured()


module Tokenization =

    // TODO: should belong to another namespace
    module Helper =

        let createCvPFromPropertyValue accession name (pv : PropertyValue) =
            let valu =
                match pv.TryGetValueReferenceAsCvTerm(), pv.TryGetUnitText(), pv.TryGetUnitCode() with
                | None, Some ut, Some uc -> 
                    WithCvUnitAccession (pv.GetValue(), CvTerm.create(uc, ut, CvTerm.refOfAccession uc))
                | None, Some ut, None ->        // user-specific case (i.e., unit given but not as a ontology-drawn term but as a custom term)
                    WithCvUnitAccession (pv.GetValue(), CvTerm.create("<missing>", ut, "<missing>"))
                | Some vr, None, None ->
                    CvValue vr
                | _ -> failwith $"Strange case occured: {pv.TryGetValueReferenceAsCvTerm()}, {pv.TryGetUnitText()}, {pv.TryGetUnitCode()}"      // TODO: Delete this when thoroughly tested
            CvParam(accession, name, CvTerm.refOfAccession accession, valu)


    /// Takes a LabProcess and returns its content tokenized as a sequence of CvParams (where each CvParam represents one property of the process).
    let ofLabProcess (labProcess : LabProcess) : CvParam seq =
        let input = LabProcess.getObjectAsSample labProcess
        let output = LabProcess.getResultAsSample labProcess
        let parameters = LabProcess.getParameterValues labProcess
        let characteristics, factors = 
            printfn "getAdditionalProperty input"
            Sample.getAdditionalProperty input
            |> List.ofSeq
            |> List.partition (
                fun cOrF ->     // characteristics OR factor
                    printfn "partition"
                    match PropertyValue.getValue cOrF with      // get the value of the PropertyValue to determine if it's Characteristics or Factor
                    | "characteristics" | "Characteristics" -> true
                    | "factor" | "Factor" -> false
                    | _ -> failwith $"Partitioning into Characteristics and Factors failed tue to Value being {PropertyValue.getValue cOrF}"
            )
        let inputCvP = CvParam("ISA:(sourceName)", "Source Name", "ISA", Sample.getName input)
        let outputCvP = CvParam("ISA:(output)", "Sample Name", "ISA", Sample.getName output)
        let parameterCvPs = 
            parameters
            |> List.map (Helper.createCvPFromPropertyValue "ISA:(Parameter)" "Parameter")
        let characteristicsCvPs =
            characteristics
            |> List.map (Helper.createCvPFromPropertyValue "ISA:(Characteristics)" "Characteristics")
        let factorCvPs =
            factors
            |> List.map (Helper.createCvPFromPropertyValue "ISA:(Factor)" "Factor")
        seq {inputCvP; yield! parameterCvPs; yield! characteristicsCvPs; yield! factorCvPs; outputCvP}





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


// Toys:

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
let characteristics = PropertyValue("char1", "Characteristics", "myCharacteristics", propertyID = "DBPO", additionalType = "Characteristics")

// ROCrate | ISA
// id = id
// name = identifier
// additionalProperty = either Characteristics or Factor in the form of propertyValue, see above: `characteristics`
// derivesFrom = (seems to be redundant)
// additionalType = ? (not mentioned in profile and mapping)
let inputs = Sample("source1", "source1", additionalProperty = [characteristics])

// see above: `inputs` annotation
// IMPORTANT: Ask people where Characteristics and Factor should be applied to: to the inputs or to the outputs or both?
let outputs = Sample("sample1", "sample1", additionalProperty = [characteristics])

// see above: `characteristics` annotation
let parameter = PropertyValue("param1", "SampleParameters", "myParameter", propertyID = "DBPO", additionalType = "Parameter")

// ROCrate | ISA
// id = id
// name = name
// agent = Performer (i.e., a Person)
// endTime = date
// executesLabProtocol = executesProtocol
// parameterValue = parameterValues (list of Parameters in the form of propertyValues, see above: `parameter`)
// object = inputs (i.e., a series of Samples)
// result = outputs (i.e., a series of Samples)
let labProcess = LabProcess("id1", "id1", person, object = inputs, result = outputs, parameterValue = [parameter])

// ROCrate | ISA
// id = id
// name = identifier
// about = Process (i.e., 1 single ISA Process, NOT the whole ProcessSequence. In the annotation table this correlates to 1 row (while the ProcessSequence correlates to ALL rows))
// measurementMethod = Technology Type
// measurementTechnique = Technology Platform
// variableMeasured = Measurement Type
// hasPart = Data Files
// url = fileName
let assay = ROCrate.Assay("assayID", "assayID", about = [labProcess])



// testings

// replace this later as soon as reading and parsing an ARC to the ROCrate JSON model representation is available:
//let studyMetadata =
//    Study.parseMetadataSheetsFromTokens() arcDir
//    |> List.concat

Tokenization.ofLabProcess (Assay.getAbout assay |> Seq.head)
Tokenization.ofLabProcess labProcess
LabProcess.getObjectAsSample labProcess