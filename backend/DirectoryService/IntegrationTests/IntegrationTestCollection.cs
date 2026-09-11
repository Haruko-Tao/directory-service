namespace IntegrationTests;

#pragma warning disable CA1711
[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebFactory>{}

#pragma warning restore CA1711