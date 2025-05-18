namespace AutoCollection.UnitTests.Models;
public record Person(string FirstName, string LastName)
{
	public string FullName => $"{FirstName} {LastName}";
}

public sealed class PersonFixture
{
	private readonly string _firstName = "John";
	private readonly string _lastName = "Doe";
	public static implicit operator Person(PersonFixture fixture) => fixture.Build();
	private Person Build() => new(_firstName, _lastName);
}