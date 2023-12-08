```mermaid

flowchart 
	xf[XLSX File]
	ips[IParams]
	of[OBO File]
	fips[filled IParams]
	partiparams[partitioned IParams]
	fipartips[filled partitioned IParams]
	grips[grouped IParams]
	onto[Structural Ontology]
	og[Ontology Graph]
	maps[matched IParams]
	arcisgr["ARC Intermediate (Metadata) Subgraph"]
	arcsgr["ARC (Metadata) Subgraph"]
	arcgr["ARC (Metadata) Graph"]
	adjips[adjusted IParams]

	D[ ]
	D2[ ]
	D3[ ]
	D4[ ]
	D5[ ]

	style D height:0.0000001px,width:0.000001px
	style D2 height:0.0000001px,width:0.000001px
	style D3 height:0.0000001px,width:0.000001px
	style D4 height:0.0000001px,width:0.000001px
	style D5 height:0.0000001px,width:0.000001px
	style of fill:#EEE,stroke-width:0,color:#777

	xf --> |parseMetadataSheetFromFile| ips
	of --> |OboOntology.parseFromFile| onto
	ips & onto --- D
	D --> |addMissingTerms| fips
	fips & og --- D4
	D4 --> |groupWhen isHeader| partiparams
	partiparams & og --- D3
	D3 --> |addMissingTermsInGroup| fipartips
	fipartips --> |groupTerms| grips
	onto --> |ontologyToGraph| og
	grips & og --- D5
	D5 --> |matchTerms| maps
	maps & og --- D2
	D2 --> |constructIntermediateMetadataSubgraph| arcisgr
	arcisgr --> |splitIntermediateMetadataSubgraph| arcsgr
	arcsgr --> |assembleMetadataGraph| arcgr
	arcsgr --> |toFlatList| adjips

	linkStyle 1 stroke:#999,color:#777,fill:#EEE

```