## Contributing

- Pick an analyzer not assigned to anyone from the [todo column of our project board](https://github.com/agoda-com/AgodaAnalyzers/projects/1) - or feel free to propose a new one.
  - For a new analyzer, create a new issue with the title in the format "AG0XXX: My analyzer title" and add the "New rule" label.
  - Ensure your new analyzer uses a DiagnosticID (eg. AG0XXX) that is not already assigned. If in doubt, see existing [issues](https://github.com/agoda-com/AgodaAnalyzers/issues?utf8=%E2%9C%93&q=is%3Aissue) (open and closed) to determine the next free ID.
- Assign the issue to yourself and move it to the "In Progress" column on our [project board](https://github.com/agoda-com/AgodaAnalyzers/projects/1).
- Create a PR and tag the original issue number in the description (eg. "Closes #31"). Your PR should contain:
  - Your analyzer.
  - Unit tests ([example](https://github.com/agoda-com/AgodaAnalyzers/blob/master/src/Agoda.Analyzers.Test/AgodaCustom/AG0027UnitTests.cs)).
  - An HTML description of the analyzer, including what it checks for, why it does it, and compliant and non-compliant code snippets ([example](https://github.com/agoda-com/AgodaAnalyzers/blob/master/src/Agoda.Analyzers/RuleContent/AG0005TestMethodNamesMustFollowConvention.html])).
  - A corresponding automatic Code Fix, if appropriate and you are feeling adventurous ([example](https://github.com/agoda-com/AgodaAnalyzers/blob/master/src/Agoda.Analyzers.CodeFixes/AgodaCustom/AG0020FixProvider.cs)).
- Please limit the changes contained in your PR to a single issue.
