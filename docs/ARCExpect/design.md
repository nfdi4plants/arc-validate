# ARCExpect API design

The main goal of ARCExpect is providing an API surface that creates the common validatiopn cases without the need for much custom input.
The following principles guide the design of the API:

- **Semantic**: Creating a validation case should be as easy as expressing it in words. Therefore, the API should be as close as possible to the natural language.
- **Simple**: The API should be simple to use and understand. It should not require a lot of boilerplate code to create a validation case.
- **Customizable**: As is always the case, there are exceptions to the rule where the API should be flexible enough to allow for custom validation cases.

# API Design

## Semantic Methods for validation case creation in the `Validate` namespace

Methods in the `Validate` (sub)namespace(s) should mirror an english sentence in the imperative mood of the following form, where words in brackets `()` are left out for brevity and/or are optional, and words in angle brackets `<>` are placeholders for the actual values:

`Validate.`_(for `<article>`)_`<object>`_(that)_`<predicate>`_(is true)_

examples:

- `Validate.Param.ValueIsEqualTo(expectedValue)` method represents:

  > `Validate` _(for a)_ `param`_(eter that it's)_ `value is equal to` _(the)_ `value <expected value>`
  
- `Validate.ParamCollection.ContainsParamWithTerm(expectedTerm)` method represents:
  > `Validate` (for a) `param`_(eter)_ `collection` _(that it)_ `contains` _(a)_ `parameter with` _(the)_ `term <expected term>`

### Objects

As validation cases are fundamentally built for validating [ControlledVocabulary Tokens]() (`CvParam`s / `IParam`s), this reflected in the objects of the _'sentences'_ provided as methods of the API.

The following objects should be supported:

- `Param` represents a single object implementing the `IParam` interface.
- `ParamCollection` represents a collection of `IParam`s.

### Predicates

Predicates are the conditions that the object(s) should satisfy. Generic methods should be provided to cover predicates that are not directly provided by the API to satisfy the _Customizable_ principle. The following predicates should be supported out of the box:

| Object | Predicate | Signature | Description |
| --- | --- | --- | --- |
| `IParam` (_Param_) | `SatisfiesPredicate` | `(IParam -> bool) -> IParam -> unit` | generic method to validate wether a single `IParam` satisfies any kind of predicate (`predicate` here refers to a function that projects `IParam -> bool`) |
| `IParam` (_Param_) | `IsEqualTo` | `IParam -> IParam -> unit` | validates wether the given param is exactly equal (structural equality regarding all object fields) to the expected param |
| `IParam` (_Param_) | `ValueIsEqualTo` | `ParamValue -> IParam -> unit` | validates wether the given param's value is equal to the expected `ParamValue`. |
| `IParam` (_Param_) | `TermIsEqualTo` | `CvTerm -> IParam -> unit` | validates wether the given param's term is equal to the expected `CvTerm` |
| `seq<#IParam>` (_ParamCollection_) | `SatisfiesPredicate` | `(seq<#IParam> -> bool) -> seq<#IParam> -> unit` | generic method to validate wether a collection of `IParam`s satisfies any kind of predicate (`predicate` here refers to a function that projects the whole sequence `seq<#IParam> -> bool`) |
| `seq<#IParam>` (_ParamCollection_) | `IsNotEmpty` | `seq<#IParam> -> unit` | validates that the given collection of `IParam`s is not empty (has a length > 0) |
| `seq<#IParam>` (_ParamCollection_) | `HasLength` | `int -> seq<#IParam> -> unit` | validates wether the given collection of `IParam`s has as many items as expected  |
| `seq<#IParam>` (_ParamCollection_) | `AllItemsSatisfyPredicate` | `(IParam> -> bool) -> seq<#IParam> -> unit` | generic method to validate wether all items in a collection of `IParam`s satisfy any kind of predicate (`predicate` here refers to a function that projects one item at a time `IParam -> bool`) |
| `seq<#IParam>` (_ParamCollection_) | `AtLeastOneItemSatisfiesPredicate` | `(IParam> -> bool) -> seq<#IParam> -> unit` | generic method to validate wether at least one item in a collection of `IParam`s satisfies any kind of predicate (`predicate` here refers to a function that projects one item at a time `IParam -> bool`) |
| `seq<#IParam>` (_ParamCollection_) | `ContainsItem` | `IParam -> seq<#IParam> -> unit` | validates wether the given collection of `IParam`s contains the expected `IParam` (by structural equality regarding all object fields)|
| `seq<#IParam>` (_ParamCollection_) | `ContainsItemWithValue` | `ParamValue -> seq<#IParam> -> unit` | validates wether the given collection of `IParam`s contains an item with the expected `ParamValue` |
| `seq<#IParam>` (_ParamCollection_) | `ContainsItemWithTerm` | `CvTerm -> seq<#IParam> -> unit` | validates wether the given collection of `IParam`s contains an item with the expected `CvTerm` |
