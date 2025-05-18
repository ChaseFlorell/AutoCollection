namespace AutoCollection.UnitTests.Models;
/// <summary>Generate a Person ReadOnlyList implementation. <see cref="Person" /> is a complex type.</summary>
[GenerateReadOnlyList(typeof(Person))]
public partial class PersonList;