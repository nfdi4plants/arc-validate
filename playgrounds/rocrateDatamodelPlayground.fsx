#r "nuget: ARCtrl"
//#r "nuget: ARCExpect"
#r "nuget: ControlledVocabulary"
#r "nuget: Expecto"
//#r "nuget: ARCTokenization"


//open ARCtrl
//open ARCExpect
open ControlledVocabulary
open Expecto
//open ARCTokenization


let arc = ARC.load @"C:\Repos\git.nfdi4plants.org\ArcPrototype"

//arc.ISA.Value.Assays |> Seq.iter (fun a -> printfn $"{a.Identifier}")
//arc.ISA.Value.GetAssay ""

//arc.ToROCrateJsonString()

//open ARCtrl
open ARCtrl.ROCrate
open ARCtrl.Json


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

let lp = LabProcess("#Process_Cell_Lysis", "Cell Lysis", "Kevin Schneider", [||], [|"lal"|])    // postponed till HLW fixes parser due to being too time-consuming to recreate

let p = Person("Oliver", "Maus")

DynamicObj.DynObj.tryGetTypedPropertyValue<obj []> "agent" lp
LabProcess.tryGetAgentAs<'T>
lp.TryGetAgentAsPerson

module Tokenization =

    /// Takes a LabProcess and returns its content tokenized as a sequence of CvParams (where each CvParam represents one property of the process).
    let ofLabProcess (labProcess : LabProcess) : CvParam seq =
        labProcess.Properties.Values 
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


Validate.Check.containsAnyOf



// testings

// replace this later as soon as reading and parsing an ARC to the ROCrate JSON model representation is available:
let studyMetadata =
    Study.parseMetadataSheetsFromTokens() arcDir
    |> List.concat