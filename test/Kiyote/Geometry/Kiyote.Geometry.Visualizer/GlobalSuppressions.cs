// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage( "Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Text is not for consumers, only for diagnostics.", Scope = "module" )]
[assembly: SuppressMessage( "Performance", "CA1814:Prefer jagged arrays over multidimensional", Justification = "Convenience for testing.", Scope = "module" )]
[assembly: SuppressMessage( "Style", "IDE0130:Namespace does not match folder structure", Justification = "Allows for cleaner imports.", Scope = "module" )]
