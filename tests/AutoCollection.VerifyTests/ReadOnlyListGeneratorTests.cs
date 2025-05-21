using System.Threading.Tasks;
using AutoCollection.VerifyTests.Util;
using VerifyXunit;
using Xunit;

namespace AutoCollection.VerifyTests;
public sealed class ReadOnlyListGeneratorTests
{
	[Fact]
	public async Task GivenReadOnlyList_WhenInternal_ThenGeneratesReadOnlyListWithDefaultBackingField()
	{
		const string CODE = """
		                    using AutoCollection;

		                    namespace Example;

		                    [GenerateReadOnlyList(typeof(string))]
		                    internal partial class DemoClass{}
		                    """;

		var generated = Infrastructure.GenerateCode(CODE);
		await Verifier.Verify(generated);
	}

	[Fact]
	public async Task GivenReadOnlyList_WhenNoBackingFieldProvided_ThenGeneratesReadOnlyListWithDefaultBackingField()
	{
		const string CODE = """
		                    using AutoCollection;

		                    namespace Example;

		                    [GenerateReadOnlyList(typeof(string))]
		                    public partial class DemoClass{}
		                    """;

		var generated = Infrastructure.GenerateCode(CODE);
		await Verifier.Verify(generated);
	}

	[Fact]
	public async Task GivenReadOnlyList_WhenBackingFieldSupplied_ThenGeneratesReadOnlyListWithCustomBackingField()
	{
		const string CODE = """
		                    using AutoCollection;

		                    namespace Example;

		                    [GenerateReadOnlyList(typeof(string), nameof(_specialItems))]
		                    public partial class DemoClass
		                    {
		                        public DemoClass(IEnumerable<string> specialItems) =>
		                            _specialItems = specialItems.ToArray();

		                        private readonly string[] _specialItems;
		                    }
		                    """;

		var generated = Infrastructure.GenerateCode(CODE);
		await Verifier.Verify(generated);
	}

	[Fact]
	public async Task GivenReadOnlyList_WhenComplexObjectSupplied_ThenGeneratesReadOnlyListWithComplexType()
	{
		const string CODE = """
		                    using AutoCollection;

		                    namespace Example;

		                    [GenerateReadOnlyList(typeof(Foo))]
		                    public partial class DemoClass{}
		                    """;

		var generated = Infrastructure.GenerateCode(CODE);
		await Verifier.Verify(generated);
	}
}
