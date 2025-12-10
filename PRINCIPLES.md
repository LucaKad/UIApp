# OOP Principles & Clean Code Showcase

This document demonstrates how the application showcases classic OOP principles, SOLID principles, and clean code philosophy.

## SOLID Principles

### 1. Single Responsibility Principle (SRP) ✓

**Example: `WikipediaService`**
- **Responsibility**: Only handles Wikipedia API communication
- **Not responsible for**: Tokenization, file storage, UI logic
- **Benefit**: Changes to Wikipedia API only affect this class

**Example: `FileArticleRepository`**
- **Responsibility**: Only handles article persistence
- **Not responsible for**: Tokenization, API calls, business logic
- **Benefit**: Can swap storage mechanism without affecting other code

### 2. Open/Closed Principle (OCP) ✓

**Example: Tokenization Strategy Pattern**
```csharp
public interface ITokenizationStrategy
{
    string Name { get; }
    IEnumerable<string> Tokenize(string text);
}
```

- **Open for extension**: New tokenization algorithms can be added by implementing `ITokenizationStrategy`
- **Closed for modification**: Existing code doesn't need to change when adding new strategies
- **Implementations**: `BasicTokenizationStrategy`, `WordTokenizationStrategy`

### 3. Liskov Substitution Principle (LSP) ✓

**Example: Repository Pattern**
```csharp
IArticleRepository repository = new FileArticleRepository(directory);
// Can be replaced with DatabaseArticleRepository without breaking code
```

- All implementations of `IArticleRepository` are interchangeable
- Client code depends on interface, not implementation
- Any repository implementation can be used without modification

### 4. Interface Segregation Principle (ISP) ✓

**Example: Focused Interfaces**
- `IWikipediaService`: Only Wikipedia operations (Search, GetArticle)
- `ITokenizationService`: Only tokenization operations
- `IArticleRepository`: Only repository operations (Save, Load, Delete)
- `ITokenizationStrategy`: Only tokenization strategy

- **No fat interfaces**: Clients don't depend on methods they don't use
- **Focused contracts**: Each interface has a single, clear purpose

### 5. Dependency Inversion Principle (DIP) ✓

**Example: Dependency Injection**
```csharp
// MainWindow depends on abstractions
private readonly IWikipediaService _wikipediaService;
private readonly ITokenizationService _tokenizationService;
private readonly IArticleRepository _articleRepository;

// Services are injected through ServiceContainer
var container = ServiceContainer.Instance;
_wikipediaService = container.WikipediaService;
```

- **High-level modules** (MainWindow) depend on abstractions (interfaces)
- **Low-level modules** (Services, Repositories) implement interfaces
- **Dependency injection** through ServiceContainer
- **No direct dependencies** on concrete classes

## Clean Code Principles

### Meaningful Names ✓

**Good Examples:**
- `WikipediaService` - clearly indicates it handles Wikipedia operations
- `TokenizationService` - clearly indicates tokenization responsibility
- `FileArticleRepository` - clearly indicates file-based storage
- `BasicTokenizationStrategy` - clearly indicates the strategy type

**Method Names:**
- `SearchArticlesAsync` - clear action and return type
- `Tokenize` - clear, concise action
- `SaveAsync` - clear persistence action
- `ValidateQuery` - clear validation purpose

### Small Functions ✓

**Example: `WikipediaService.ParseSearchResults`**
- Does one thing: parses JSON response
- Easy to understand
- Easy to test
- Can be modified without affecting other code

**Example: `FileArticleRepository.SaveArticleContentAsync`**
- Single responsibility: saves article content
- Clear purpose
- Easy to test

### DRY (Don't Repeat Yourself) ✓

**Example: Validation Methods**
```csharp
private void ValidateQuery(string query)
private void ValidatePageId(int pageId)
private void ValidateTokenizedArticle(TokenizedArticle article)
```

- Validation logic centralized
- Reusable across methods
- Consistent validation behavior

### Error Handling ✓

**Custom Exception Hierarchy:**
```csharp
DomainException (base)
├── WikipediaServiceException
├── ArticleRepositoryException
└── TokenizationException
```

- **Domain-specific exceptions** for better error handling
- **Proper exception propagation**
- **Meaningful error messages**

### Value Objects ✓

**Example: `FilePaths`**
```csharp
public class FilePaths
{
    public string ArticlePath { get; }
    public string TokensPath { get; }
    public string MetadataPath { get; }
    
    public bool AllExist() { ... }
}
```

- **Encapsulates related data**
- **Provides behavior** (AllExist method)
- **Immutable** (read-only properties)

## Design Patterns

### Repository Pattern ✓
- Abstracts data access layer
- `IArticleRepository` interface
- `FileArticleRepository` implementation
- Easy to swap implementations

### Strategy Pattern ✓
- `ITokenizationStrategy` interface
- Multiple implementations
- Runtime algorithm selection
- Extensible without modification

### Service Locator Pattern ✓
- `ServiceContainer` provides centralized access
- Singleton pattern for container
- Thread-safe initialization

### Factory Pattern (Implicit) ✓
- `ServiceContainer` creates service instances
- Centralized object creation
- Configuration in one place

## Rich Domain Models ✓

**Example: `Article`**
```csharp
public class Article
{
    public bool IsValid { get; }
    public int WordCount { get; }
    public int CharacterCount { get; }
}
```

- **Business logic in domain models**
- **Validation built-in**
- **Calculated properties**
- **Rich behavior**

## Encapsulation ✓

**Example: Private Validation Methods**
```csharp
private void ValidateQuery(string query)
private void ValidatePageId(int pageId)
```

- **Internal validation logic** hidden from clients
- **Public interface** exposes only necessary methods
- **Implementation details** encapsulated

## Abstraction ✓

**Example: Service Interfaces**
- Clients depend on `IWikipediaService`, not `WikipediaService`
- Implementation can change without affecting clients
- Clear contracts through interfaces

## Polymorphism ✓

**Example: Tokenization Strategies**
```csharp
ITokenizationStrategy strategy = new BasicTokenizationStrategy();
// Can be replaced with WordTokenizationStrategy
// Same interface, different behavior
```

- **Multiple implementations** of same interface
- **Runtime behavior selection**
- **Polymorphic behavior**

## Summary

This application demonstrates:
- ✅ All 5 SOLID principles
- ✅ Clean code best practices
- ✅ Classic OOP principles (Encapsulation, Abstraction, Inheritance, Polymorphism)
- ✅ Design patterns (Repository, Strategy, Service Locator, Factory)
- ✅ Rich domain models
- ✅ Proper error handling
- ✅ Dependency injection
- ✅ Interface-based design
- ✅ Separation of concerns

The architecture is:
- **Maintainable**: Clear structure, easy to modify
- **Testable**: Interfaces enable mocking, dependency injection
- **Extensible**: New features can be added without breaking existing code
- **Readable**: Meaningful names, small functions, clear structure

