using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCollection.UnitTests.Models;
///<summary>Generate a Custom ReadOnlyList implementation</summary>
[GenerateReadOnlyList(typeof(int), nameof(_vals))]
public partial class CustomIntReadOnlyList(IEnumerable<int>? vals)
{
	private readonly IReadOnlyList<int> _vals =
		vals?.ToArray() ?? throw new ArgumentNullException(nameof(vals));
}