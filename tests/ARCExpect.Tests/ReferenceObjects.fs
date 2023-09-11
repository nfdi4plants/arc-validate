module ReferenceObjects

open ControlledVocabulary

module CvTerms =

    let ``Investigation Person First Name`` = CvTerm.create("INVMSO:00000023","Investigation Person First Name","INVMSO")

    let ``Investigation Person Email`` = CvTerm.create("INVMSO:00000025","Investigation Person Email","INVMSO")        

module CvParams =
    
    let ``Investigation Person First Name`` = CvParam(CvTerms.``Investigation Person First Name``,ParamValue.Value "Kevin")

    let ``Investigation Person Email`` = CvParam(CvTerms.``Investigation Person Email``, ParamValue.Value "yes@yes.com")