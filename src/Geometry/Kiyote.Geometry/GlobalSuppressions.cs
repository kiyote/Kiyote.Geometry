// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage( "Performance", "CA1812:An internal (assembly-level) type is never instantiated.", Justification = "Analysis does not understand Dependency Injection.", Scope = "module" )]
