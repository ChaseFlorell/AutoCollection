using System.Threading.Tasks;
using AutoCollection.VerifyTests.Util;
using VerifyXunit;
using Xunit;

namespace AutoCollection.VerifyTests;
public class ListGeneratorTests
{
	[Fact]
	public async Task GivenList_WhenInternal_ThenGeneratesListWithDefaultBackingField()
	{
		var sut = new ListGenerator();
		const string CODE = """
		                    using AutoCollection;

		                    namespace Example;

		                    [GenerateList(typeof(string))]
		                    internal partial class DemoClass{}
		                    """;

		var generated = Infrastructure.GenerateCode(sut, CODE);
		await Verifier.Verify(generated);
	}

	[Fact]
	public async Task GivenList_WhenNoBackingFieldProvided_ThenGeneratesListWithDefaultBackingField()
	{
		var sut = new ListGenerator();
		const string CODE = """
		                    using AutoCollection;

		                    namespace Example;

		                    [GenerateList(typeof(string))]
		                    public partial class DemoClass{}
		                    """;

		var generated = Infrastructure.GenerateCode(sut, CODE);
		await Verifier.Verify(generated);
	}

	[Fact]
	public async Task GivenList_WhenBackingFieldSupplied_ThenGeneratesListWithCustomBackingField()
	{
		var sut = new ListGenerator();
		const string CODE = """
		                    using AutoCollection;

		                    namespace Example;

		                    [GenerateList(typeof(string), nameof(_specialItems))]
		                    public partial class DemoClass
		                    {
		                        public DemoClass(IEnumerable<string> specialItems) =>
		                            _specialItems = specialItems.ToArray();

		                        private  string[] _specialItems;
		                    }
		                    """;

		var generated = Infrastructure.GenerateCode(sut, CODE);
		await Verifier.Verify(generated);
	}

	[Fact]
	public async Task GivenList_WhenComplexObjectSupplied_ThenGeneratesListWithComplexType()
	{
		var sut = new ListGenerator();
		const string CODE = """
		                    using AutoCollection;

		                    namespace Example;

		                    [GenerateList(typeof(Foo))]
		                    public partial class DemoClass{}
		                    """;

		var generated = Infrastructure.GenerateCode(sut, CODE);
		await Verifier.Verify(generated);
	}

	[Fact]
	public async Task GivenNoAttribute_WhenGenerated_ThenShouldReturnNothing()
	{
		var sut = new ListGenerator();
		const string CODE = """
		                    using AutoCollection;

		                    namespace Example;

		                    public partial class DemoClass{}
		                    """;

		var generated = Infrastructure.GenerateCode(sut, CODE);
		await Verifier.Verify(generated);
	}
}