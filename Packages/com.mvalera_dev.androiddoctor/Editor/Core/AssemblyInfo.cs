using System.Runtime.CompilerServices;

// Lets the test assembly call the pure, internal evaluation methods of the rules.
[assembly: InternalsVisibleTo("AndroidDoctor.Editor.Tests")]
