## Contributing

- Pick an analyzer not assigned to anyone from the [todo column of our project board](https://github.com/agoda-com/AgodaAnalyzers/projects/1) - or feel free to propose a new one.
  - For a new analyzer, create a new issue with the title in the format "AG0XXX: My analyzer title" and add the "New rule" label.
   - Ensure your new analyzer uses a DiagnosticID (eg. AG0XXX) that is not already assigned. If in doubt, see existing [issues](https://github.com/agoda-com/AgodaAnalyzers/issues?utf8=%E2%9C%93&q=is%3Aissue) (open and closed) to determine the next free ID.
- Assign the issue to yourself to your issue and move it to the "In Progress" column on our [project board](https://github.com/agoda-com/AgodaAnalyzers/projects/1).
- Create a PR and tag the original issue number in the description (eg. "Fixes "#31"). Your PR should contain:
  - Your analyzer.
  - Unit tests.
  - An HTML description of the analyzer, including what it checks for, why it does it, and examples on how to fix any problems it finds ([examples](https://github.com/agoda-com/AgodaAnalyzers/tree/master/src/Agoda.Analyzers/RuleContent])).
  - A corresponding automatic Code Fix, if you are feeling adventurous. 
- Your PR should include changes only for a single issue.
