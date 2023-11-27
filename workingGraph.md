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
	arcgr["ARC (Metadata) Graph"]

	D[ ]:::empty
	D2[ ]:::empty
	D3[ ]:::empty

	style D width:0,height:0
	style D2 width:0,height:0
	style D3 width:0,height:0
	classDef empty width:1px,height:0px
	style of fill:#333,stroke-width:0,color:#777

	xf -->|parseMetadataSheetFromFile| ips
	of --> |OboOntology.parseFromFile| onto
	ips & onto --- D
	D -->|addMissingTerms| fips
	fips -->|groupWhen isHeader| partiparams
	partiparams & onto --- D3
	D3 -->|addMissingTerms| fipartips
	fipartips -->|groupTerms| grips
	onto -->|ontologyToGraph| og
	grips -->|matchTerms| maps
	maps & og --- D2
	D2 -->|constructMetadataGraph| arcgr

	linkStyle 1 stroke:#777,color:#777,arrow:#111

```