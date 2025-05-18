using System.Threading.Tasks;
using AutoCollection.VerifyTests.Util;
using VerifyXunit;
using Xunit;

namespace AutoCollection.VerifyTests;
public class ReadOnlyListGeneratorTests
{
	[Fact]
	public async Task GeneratesInternalReadOnlyListWithDefaultBackingField()
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
	public async Task GeneratesReadOnlyListWithDefaultBackingField()
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
	public async Task GeneratesReadOnlyListWithCustomBackingField()
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
	public async Task GeneratesReadOnlyListWithComplexType()
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