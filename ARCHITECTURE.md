# Architecture Documentation

This application demonstrates classic OOP architectural principles, SOLID principles, and clean code philosophy.

## Architecture Overview

The application follows a layered architecture with clear separation of concerns:

```
UIApp/
├── Models/              # Domain Models (Rich Domain Models)
├── Interfaces/          # Contracts (Interface Segregation)
├── Services/            # Business Logic Services
├── Repositories/       # Data Access Layer (Repository Pattern)
├── Strategies/         # Strategy Pattern Implementations
├── Exceptions/         # Custom Exception Hierarchy
└── ValueObjects/       # Value Objects
```

## SOLID Principles Implementation

### Single Responsibility Principle (SRP)
Each class has a single, well-defined responsibility:
- **WikipediaService**: Handles Wikipedia API interactions only
- **TokenizationService**: Manages tokenization logic only
- **FileArticleRepository**: Handles file persistence only
- **TokenizationStrategy**: Implements a specific tokenization algorithm

### Open/Closed Principle (OCP)
- **TokenizationStrategy** interface allows adding new tokenization algorithms without modifying existing code
- New strategies can be added by implementing `ITokenizationStrategy`
- Services are open for extension through interfaces

### Liskov Substitution Principle (LSP)
- All implementations of `ITokenizationStrategy` are interchangeable
- Repository implementations can be swapped without breaking client code
- Service implementations follow their interface contracts

### Interface Segregation Principle (ISP)
- Interfaces are focused and specific:
  - `IWikipediaService`: Only Wikipedia-related operations
  - `ITokenizationService`: Only tokenization operations
  - `IArticleRepository`: Only repository operations
  - `ITokenizationStrategy`: Only tokenization strategy
- Clients depend only on methods they use

### Dependency Inversion Principle (DIP)
- High-level modules (MainWindow) depend on abstractions (interfaces)
- Low-level modules (Services, Repositories) implement interfaces
- Dependency injection through `ServiceContainer`
- No direct dependencies on concrete classes in business logic

## Design Patterns

### Repository Pattern
- `IArticleRepository` abstracts data access
- `FileArticleRepository` implements file-based persistence
- Easy to swap with database or other storage implementations

### Strategy Pattern
- `ITokenizationStrategy` defines tokenization algorithm contract
- Multiple implementations: `BasicTokenizationStrategy`, `WordTokenizationStrategy`
- Runtime selection of tokenization algorithm

### Service Locator Pattern
- `ServiceContainer` provides centralized service access
- Singleton pattern for service container
- Thread-safe initialization

### Factory Pattern (Implicit)
- ServiceContainer acts as a factory for service instances
- Centralized object creation

## Clean Code Principles

### Meaningful Names
- Classes, methods, and variables have descriptive names
- No abbreviations or unclear names
- Names reflect intent and responsibility

### Small Functions
- Functions do one thing
- Functions are short and focused
- Easy to understand and test

### DRY (Don't Repeat Yourself)
- Common logic extracted to reusable methods
- Validation logic centralized
- No code duplication

### Error Handling
- Custom exception hierarchy
- Domain-specific exceptions
- Proper exception propagation

### Value Objects
- `FilePaths` encapsulates related file paths
- Immutable and validated
- Rich behavior through methods

## Domain Models

### Rich Domain Models
Models contain business logic and validation:
- `Article.IsValid`: Domain validation
- `Article.WordCount`: Business calculation
- `Article.CharacterCount`: Domain property
- `SavedArticle.ExistsOnDisk`: Domain behavior

## Dependency Injection

Services are injected through `ServiceContainer`:
- Loose coupling between components
- Easy to test (can inject mocks)
- Centralized configuration

## Exception Handling

Custom exception hierarchy:
- `DomainException`: Base for all domain exceptions
- `WikipediaServiceException`: Wikipedia API errors
- `ArticleRepositoryException`: Repository errors
- `TokenizationException`: Tokenization errors

## Testing Considerations

The architecture supports testing:
- Interfaces enable mocking
- Dependency injection allows test doubles
- Small, focused classes are easy to unit test
- Clear separation of concerns

## Future Enhancements

The architecture supports:
- Adding new tokenization strategies
- Swapping repository implementations
- Adding new services
- Implementing caching layers
- Adding logging/monitoring

