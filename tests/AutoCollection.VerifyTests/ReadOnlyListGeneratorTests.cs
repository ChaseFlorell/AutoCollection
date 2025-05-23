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
		var sut = new ReadOnlyListGenerator();
		const string CODE = """
		                    using AutoCollection;

		                    namespace Example;

		                    [GenerateReadOnlyList(typeof(string))]
		                    internal partial class DemoClass{}
		                    """;

		var generated = Infrastructure.GenerateCode(sut, CODE);
		await Verifier.Verify(generated);
	}

	[Fact]
	public async Task GivenReadOnlyList_WhenNoBackingFieldProvided_ThenGeneratesReadOnlyListWithDefaultBackingField()
	{
		var sut = new ReadOnlyListGenerator();
		const string CODE = """
		                    using AutoCollection;

		                    namespace Example;

		                    [GenerateReadOnlyList(typeof(string))]
		                    public partial class DemoClass{}
		                    """;

		var generated = Infrastructure.GenerateCode(sut, CODE);
		await Verifier.Verify(generated);
	}

	[Fact]
	public async Task GivenReadOnlyList_WhenBackingFieldSupplied_ThenGeneratesReadOnlyListWithCustomBackingField()
	{
		var sut = new ReadOnlyListGenerator();
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

		var generated = Infrastructure.GenerateCode(sut, CODE);
		await Verifier.Verify(generated);
	}

	[Fact]
	public async Task GivenReadOnlyList_WhenComplexObjectSupplied_ThenGeneratesReadOnlyListWithComplexType()
	{
		var sut = new ReadOnlyListGenerator();
		const string CODE = """
		                    using AutoCollection;

		                    namespace Example;

		                    [GenerateReadOnlyList(typeof(Foo))]
		                    public partial class DemoClass{}
		                    """;

		var generated = Infrastructure.GenerateCode(sut, CODE);
		await Verifier.Verify(generated);
	}

	[Fact]
	public async Task GivenList_WhenGenerated_ThenShouldReturnNothing()
	{
		var sut = new ReadOnlyListGenerator();
		const string CODE = """
		                    using AutoCollection;

		                    namespace Example;

		                    [GenerateList(typeof(Foo))]
		                    public partial class DemoClass{}
		                    """;

		var generated = Infrastructure.GenerateCode(sut, CODE);
		await Verifier.Verify(generated);
	}
}