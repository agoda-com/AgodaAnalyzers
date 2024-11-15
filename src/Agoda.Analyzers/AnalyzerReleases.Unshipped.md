; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
AG0002 | Agoda.CSharp.CustomQualityRules | Warning | AG0002PrivateMethodsShouldNotBeTested
AG0003 | Agoda.CSharp.CustomQualityRules | Warning | AG0003HttpContextCannotBePassedAsMethodArgument, [Documentation](https://agoda-com.github.io/standards-c-sharp/services/framework-abstractions.html)
AG0004 | Agoda.CSharp.CustomQualityRules | Warning | AG0004DoNotUseHardCodedStringsToIdentifyTypes, [Documentation](https://agoda-com.github.io/standards-c-sharp/reflection/hard-coded-strings.html)
AG0005 | Agoda.CSharp.CustomQualityRules | Warning | AG0005TestMethodNamesMustFollowConvention, [Documentation](https://agoda-com.github.io/standards-c-sharp/testing/test-method-names-should-clearly-indicate-what-they-are-testing.html)
AG0006 | Agoda.CSharp.CustomQualityRules | Warning | AG0006RegisteredComponentShouldHaveExactlyOnePublicConstructor
AG0009 | Agoda.CSharp.CustomQualityRules | Warning | AG0009IHttpContextAccessorCannotBePassedAsMethodArgument
AG0010 | Agoda.CSharp.CustomQualityRules | Warning | AG0010PreventTestFixtureInheritance, [Documentation](https://agoda-com.github.io/standards-c-sharp/unit-testing/be-wary-of-refactoring-tests.html)
AG0012 | Agoda.CSharp.CustomQualityRules | Warning | AG0012TestMethodMustContainAtLeastOneAssertion, [Documentation](https://agoda-com.github.io/standards-c-sharp/testing/tests-as-a-specification.html)
AG0018 | Agoda.CSharp.CustomQualityRules | Warning | AG0018PermitOnlyCertainPubliclyExposedEnumerables, [Documentation](https://agoda-com.github.io/standards-c-sharp/collections/choosing-collection-implementation.html)
AG0020 | Agoda.CSharp.CustomQualityRules | Warning | AG0020AvoidReturningNullEnumerables, [Documentation](https://agoda-com.github.io/standards-c-sharp/collections/null-empty-enumerables.html)
AG0021 | Agoda.CSharp.CustomQualityRules | Info | AG0021PreferAsyncMethods, [Documentation](https://agoda-com.github.io/standards-c-sharp/async/consume-async-method.html)
AG0022 | Agoda.CSharp.CustomQualityRules | Warning | AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods, [Documentation](https://agoda-com.github.io/standards-c-sharp/async/expose-async-method.html)
AG0027 | Agoda.CSharp.CustomQualityRules | Warning | AG0027EnsureOnlyDataSeleniumIsUsedToFindElements, [Documentation](https://agoda-com.github.io/standards-c-sharp/gui-testing/data-selenium.html)
AG0030 | Agoda.CSharp.CustomQualityRules | Warning | AG0030PreventUseOfDynamics, [Documentation](https://agoda-com.github.io/standards-c-sharp/code-style/dynamics.html)
AG0037 | Agoda.CSharp.CustomQualityRules | Error | AG0037EnsureSeleniumTestHasOwnedByAttribute
AG0038 | Agoda.CSharp.CustomQualityRules | Warning | AG0038PreventUseOfRegionPreprocessorDirective, [Documentation](https://agoda-com.github.io/standards-c-sharp/code-style/regions.html)
AG0039 | Documentation | Hidden | AG0039MethodLineLengthAnalyzer, [Documentation](https://github.com/agoda-com/AgodaAnalyzers/blob/master/src/Agoda.Analyzers/RuleContent/AG0039MethodLineLengthAnalyzer.html)
AG0041 | Best Practices | Warning | AG0041LogTemplateAnalyzer, [Documentation](https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/AG0041.md)
AG0042 | Agoda.CSharp.CustomQualityRules | Warning | AG0042QuerySelectorShouldNotBeUsed, [Documentation](https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/AG0042.md)
AG0043 | Agoda.CSharp.CustomQualityRules | Error | AG0043NoBuildServiceProvider, [Documentation](https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/AG0043.md)
AG0044 | Usage | Warning | AG0044ForceOptionShouldNotBeUsed
SA1106 | StyleCop.CSharp.ReadabilityRules | Warning | SA1106CodeMustNotContainEmptyStatements
SA1107 | StyleCop.CSharp.ReadabilityRules | Warning | SA1107CodeMustNotContainMultipleStatementsOnOneLine
SA1123 | StyleCop.CSharp.ReadabilityRules | Warning | SA1123DoNotPlaceRegionsWithinElements