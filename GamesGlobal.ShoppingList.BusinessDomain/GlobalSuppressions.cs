// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "MA0062:Non-flags enums should not be marked with \"FlagsAttribute\"", Justification = "All is and None are strategically used in this case.", Scope = "type", Target = "~T:GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth.Permissions")]
[assembly: SuppressMessage("Major Code Smell", "S4070:Non-flags enums should not be marked with \"FlagsAttribute\"", Justification = "All is and None are strategically used in this case.", Scope = "type", Target = "~T:GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth.Permissions")]
