using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCollection.UnitTests.Models;
[GenerateList(typeof(int), nameof(_vals))]
internal partial class CustomIntList(IEnumerable<int> vals)
{
	private readonly IList<int> _vals =
		vals?.ToArray() ?? throw new ArgumentNullException(nameof(vals));
}