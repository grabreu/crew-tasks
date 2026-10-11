namespace SharedTodo.Users.FunctionalTests.TestSupport;

[CollectionDefinition(nameof(FunctionalTestCollection))]
public class FunctionalTestCollection : ICollectionFixture<FunctionalTestFixture>;
