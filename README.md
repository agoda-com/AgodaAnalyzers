# Agoda Analyzers

This project is used by Agoda Internally for analysis of our C# projects. We have opened it for community contribution.

It started off with just 3 rules from style cop, and we are building it from here.

In order to debug this project please select the Agoda.Analyzers.Vsix as the start up project and it will launch vs2017 in experimental mode and you can test the Analysis rules.

To generate a jar file from this project for use with soarqube we have prepared a fork of the sonar team's project that has been udpated to 1.3 here https://github.com/agoda-com/sonarqube-roslyn-sdk

## Contributing

Please ensure that:

- Your analyzer uses a DiagnosticID (eg. AG0XXX) that is not already assigned. If in doubt, see existing issues (open and closed) to determine the next free ID.
- You link your pull request to an issue. If an issue does not exist then please create one with the title in the format "AG0XXX: My issue title".
- Your PR includes changes for just a single issue.
