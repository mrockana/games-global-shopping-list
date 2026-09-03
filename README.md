# Introduction 
A WebAPI that allows a user to login and maintain a shopping list.

## Compose Stack Containers

1. **gamesglobal.shoppinglist.webapi**
   - This container runs our web api GamesGlobal.ShoppingList.WebAPI. It is exposed on ports 8000 (https) and 8001 (http) and exports its telemetry to the OpenTelemetry collector.
2. **sql.database**
   - PostgreSQL 17 (`postgres:17`) that our application uses to persist data. It listens on port 5432 and stores its data in the `postgres-data` Docker named volume.
   - pgAdmin 4 web client for browsing and querying the PostgreSQL database. Its UI is available on port 5050 (`http://localhost:5050`), it logs in with `PGADMIN_DEFAULT_EMAIL` / `PGADMIN_DEFAULT_PASSWORD` and keeps its settings in the `pgadmin-data` named volume. From inside the stack connect to host `webapi-app-database` on port 5432.
4. **otel-collector**
   - OpenTelemetry Collector that receives logs, traces and metrics from the web api over OTLP (ports 4317/4318). It then fans that telemetry out to Jaeger, Prometheus and Loki.
5. **jaeger**
   - Distributed tracing backend used to store and explore request traces. Its UI is available on port 16686.
6. **prometheus**
   - Time series database that scrapes and stores the application and collector metrics. Its UI is available on port 9090.
7. **loki**
   - Log aggregation store for the application logs, listening on port 3100. Grafana queries it to search and filter our logs.
8. **promtail**
   - Log shipping agent that tails the log files under `./appData/promatail/log` and pushes them into Loki.
9. **minio**
   - S3 compatible object storage used to store uploaded files such as shopping item images. The S3 API is exposed on port 9000 and the web console on port 9001 (`http://localhost:9001`), signing in with `MINIO_ROOT_USER` / `MINIO_ROOT_PASSWORD`. Its objects are kept in the `minio-data` named volume and from inside the stack it is reachable at `http://minio:9000`.
   - Dashboarding and visualisation tool served on port 3000. It is the single place to view the metrics, logs and traces coming from Prometheus, Loki and Jaeger.

> These observability containers are intended for local development only; a hosted observability solution should be used for production.


# Prerequisite

1. Download and install dotnet 10 [SDK](https://dotnet.microsoft.com/en-us/download).
2. Download and install [Docker Desktop Windows](https://docs.docker.com/desktop/setup/install/windows-install/) or [Docker Desktop Mac](https://docs.docker.com/desktop/setup/install/mac-install/).
3. Optionally download and install [pgAdmin](https://www.pgadmin.org/download/) (or any PostgreSQL client, e.g. `psql`) - the compose stack already ships a pgAdmin container on `http://localhost:5050`.
4. Download and install free [Visual Studio || Visual Studio Code](https://visualstudio.microsoft.com/free-developer-offers/)
5. Download and install [Git](https://git-scm.com/downloads).
6. Download dotnet-ef tool globally using the following command
   ```bash
   dotnet tool install --global dotnet-ef
   ```
   
# Getting Started
See below, to start running and testing the solution.
1.	Make sure you have everything in the Prerequisite section above
2.	Clone the solution to your local machine.
3.	Open terminal on your solution file and run the following command
    - `dotnet restore ./GamesGlobal.ShoppingList.sln`
4.	Open the solution in Visual Studio / VS Code.
5.  Create a `.env` file beside `.env.example` in the repository root. Do not edit `.env.example`; it is the committed template. Use one of the following commands:
   - PowerShell: `Copy-Item .env.example .env`
   - Bash: `cp .env.example .env`
6.  Replace every placeholder value in `.env` with local development values. At minimum, use the same `POSTGRES_DB`, `POSTGRES_USER`, and `POSTGRES_PASSWORD` values that you will use in the connection string below. Generate strong, unique values for `IDENTITY_JWT_SIGNING_KEY` and `IDENTITY_HASHED_TOKEN_SIGNING_KEY`.

7.  Configure the Web API's local user secrets. Replace each `<...>` placeholder with the matching value from your `.env` file. These values are needed when running EF Core commands locally:
   ```powershell
   dotnet user-secrets set --project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj "ConnectionStrings:postgres" "Host=localhost;Port=5432;Database=<POSTGRES_DB>;Username=<POSTGRES_USER>;Password=<POSTGRES_PASSWORD>;"
   dotnet user-secrets set --project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj "IdentityModuleOptions:JwtSigningKey" "<IDENTITY_JWT_SIGNING_KEY>"
   dotnet user-secrets set --project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj "IdentityModuleOptions:HashedTokenSigningKey" "<IDENTITY_HASHED_TOKEN_SIGNING_KEY>"
   dotnet user-secrets set --project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj "FileObjectStoreOptions:User" "<FILE_OBJECT_STORE_USER>"
   dotnet user-secrets set --project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj "FileObjectStoreOptions:Secret" "<FILE_OBJECT_STORE_SECRET>"
   ```
8.  Make sure you set **docker-compose** project as startup project.
9. Connect with pgAdmin (`http://localhost:5050`, or a locally installed client / `psql`) using the connection details from the previous steps. This is just to check if you are able to access the database. From the pgAdmin container use host `webapi-app-database`, from a local client use `localhost`.
10. Run the following [EF Core Migration commands](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli)
    - ApplicationModule `dotnet ef database update --project .\GamesGlobal.ShoppingList.Infrastructure\GamesGlobal.ShoppingList.Infrastructure.csproj --startup-project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj --context ApplicationDbContext`
11. Check the database and make sure you can see the newly created tables with seeded data. The application tables live in the `public` schema and the identity tables in the `identity` schema.
14. You can now Build, run and test the solution.
        

# Testing with `.http` Files

The API request files are kept with their features under `GamesGlobal.ShoppingList.WebApi`. To test an authenticated request, first run a request from `Identity/Features/Login/Login.http` using the matching local environment, then copy the `Token` value from the response into the corresponding Web API user secret:

| Login request | HTTP environment | User-secret key |
| --- | --- | --- |
| End-User Log-in | `local-user` | `HttpTest:EndUserToken` |
| Super Admin Log-in | `local-admin` | `HttpTest:AdminToken` |
| Auditor General Log-in | `local-auditor` | `HttpTest:AuditorToken` |

For example, after using the End-User Log-in request, run:
```powershell
dotnet user-secrets set --project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj "HttpTest:EndUserToken" "<Token from the login response>"
```

Select the same `local-user`, `local-admin`, or `local-auditor` environment when running an authenticated `.http` request. The environment configuration reads the token from user secrets, and the request sends it as `Authorization: Bearer {{token}}`. When the token expires, log in again and update the relevant user secret.


# Build and Test
1. Open your solution in Visual Studio / VS Code.
2. Run the solution and play around with available functionality.
3. Run the following command to Unit test the solution
  `dotnet test .\GamesGlobal.ShoppingList.sln`

# Contribute

1. Please keep the changes on your branch/pull request small.
2. Name your branches in the following manner `Task/something-you-doing-{story-number}`, in cases of bugs `Bug/something-you-doing-{bug-number}`
3. Always run `dotnet format ./GamesGlobal.ShoppingList.sln` before committing your changes.
4. Always add Trello, Jira story/task links with commits
5. If we using Azure DevOps board you should add hashtag ticket number with your commit message. See the example commit message below.
   `this is my commit message with associated devops ticket number #8979`
6. When you are happy with your change create a pull request with meaning full name and Bug/Story number in the. see below for example.
   `Pull request name 8979`
7. After creating the Pull Request ask your team-mate to review the pull request.
8. Always select a checkbox to Delete your branch after merging your pull request, ask your team if you are not sure where to find this.
9. Updating the database (column or table update)
    9.1. After making changes to the database models or adding new EntityModel DbSet you should run the following commands
    ```
        // Add migration - the {NameOfMigration} should describe the changes you are doing to the db
        
        // Identity
        dotnet ef migrations add {NameOfMigration} --project .\GamesGlobal.ShoppingList.Infrastructure\GamesGlobal.ShoppingList.Infrastructure.csproj --startup-project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj --output-dir DataAccess/Identity/Migrations --context IdentityDbContext

        // Application
        dotnet ef migrations add {NameOfMigration} --project .\GamesGlobal.ShoppingList.Infrastructure\GamesGlobal.ShoppingList.Infrastructure.csproj --startup-project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj --output-dir DataAccess/Application/Migrations --context ApplicationDbContext


        // Update Database.
        
        // Identity
        dotnet ef database update --project .\GamesGlobal.ShoppingList.Infrastructure\GamesGlobal.ShoppingList.Infrastructure.csproj --startup-project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj --context IdentityDbContext
        
       // Application
       dotnet ef database update --project .\GamesGlobal.ShoppingList.Infrastructure\GamesGlobal.ShoppingList.Infrastructure.csproj --startup-project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj --context ApplicationDbContext

    ```
    9.2. Please make sure you only submit one migration per feature. if you have multiple migrations please use the following commands to consolidate your migrations into one before asking a fellow dev to review your pull request .

```
    // ef database update {MigrationName} 

    // Identity
    dotnet ef database update {MigrationName} --project .\GamesGlobal.ShoppingList.Infrastructure\GamesGlobal.ShoppingList.Infrastructure.csproj --startup-project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj --context IdentityDbContext
    
    // Application
    dotnet ef database update {MigrationName} --project .\GamesGlobal.ShoppingList.Infrastructure\GamesGlobal.ShoppingList.Infrastructure.csproj --startup-project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj --context ApplicationDbContext

    
    // ef migrations remove to revert the migrations and repeat step 9.1

    // Identity
    dotnet ef migrations remove --project .\GamesGlobal.ShoppingList.Infrastructure\GamesGlobal.ShoppingList.Infrastructure.csproj --startup-project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj --context IdentityDbContext

    // Application
    dotnet ef migrations remove --project .\GamesGlobal.ShoppingList.Infrastructure\GamesGlobal.ShoppingList.Infrastructure.csproj --startup-project .\GamesGlobal.ShoppingList.WebApi\GamesGlobal.ShoppingList.WebApi.csproj --context ApplicationDbContext
```

# Architecture Ideas

## Modular Application

You have notice in the section above we tend to have separate DbContext for Application and Identity. This is because we are following a modular architecture approach.
The Identity functionality are on separate module from the application this allows us to have a clear separation of concerns and allows us to develop and test. 
And if required in the future we can easily move a module into its own solution or application.
So when developing the application we should always keep in mind points of the applications that could be treated as separate modules and develop in a way that it is always easy to move into separate module afterward.


## CQRS

**Command Query Responsibility Segregation (CQRS)** is an architectural principle that separates the responsibilities of reading data (queries) from writing or modifying data (commands). This separation helps us design systems that are easier to maintain, scale, and test.

### How We Use CQRS in This Solution

- **Queries** are used to retrieve data. They do not modify the state of the system.
- **Commands** are used to change data or trigger actions.

In our codebase:
- Queries implement the `IQuery` interface.
- Commands implement the `ICommand` interface.
- Each query or command is handled by a dedicated MediatR handler, which encapsulates the business logic for that operation.
- All EF Core read calls inside Query handler should be No tracking/ non-tracking.

#### Example Usage

```
    // Query 
    public record GetShoppingItemsRequest(int UserId)
    : IQuery<IList<GetShoppingItemResponse>>
    {
    }


    // Command 
    public record UpdateShoppingItemRequest(int ShoppingItemId, int UserId, string Name, string? Description)
        : ICommand<UpdateShoppingItemResponse>
    {
    }
```

## Test Driven Development

### How We Use TDD in This Solution

Our solution follows Test Driven Development (TDD) principles to ensure high code quality, maintainability, and confidence in our features. TDD is a development approach where tests are written before the actual implementation code. This helps us clarify requirements, design robust APIs, and catch regressions early.

#### TDD Workflow in Our Projects

1. **Write a Failing Test First**  
   For every new feature or bug fix, start by writing an xUnit test that describes the expected behavior. The test should fail initially, as the feature is not yet implemented.

2. **Implement the Minimum Code to Pass the Test**  
   Write just enough code to make the test pass. Focus on correctness and simplicity.

3. **Refactor**  
   Once the test passes, refactor the code for readability, performance, or maintainability. Ensure all tests still pass after refactoring.

4. **Repeat**  
   Continue this cycle for each new feature, edge case, or bug fix.

#### Example: TDD in Practice

- For every MediatR handler (e.g., `ResetPasswordCommandHandler`, `UpdateRolePermissionsCommandHandler`), you will find corresponding xUnit test classes such as:
  - `ResetPasswordCommandHandlerTests`
  - `UpdateRolePermissionsCommandHandlerTests`
  
- These test classes cover all logical branches, including success scenarios, error handling, and edge cases.

Use descriptive test names and instead of putting commands in the handler please rather put comments in the tests to explain unusual patterns and implementation that are not obvisous from reading the code.

## Vertical Slice

**Vertical Slice Architecture** is a way of organizing code so that each feature is grouped together, rather than splitting code by technical layers (like controllers, services, repositories, etc.). In a vertical slice, everything needed for a feature�such as requests, handlers, validation, domain logic, and tests�is kept together. This makes it easier to understand, maintain, and extend features.

### How We Use Vertical Slice in This Solution

- **Feature-Folder Structure:**  
  Each feature (for example, "Reset Password", "Update User Roles", "Session Login") has its own folder under the `Application/Identity/Features` directory. Inside each folder, you will find:
  - The request. command or query object
  - The handler class
  - Any related validation classes
  - Feature-specific domain logic
  - If any repeating domain logic is detected across multiple feature then it can be moved to `GamesGlobal.ShoppingList.BusinessDomain`

- **Self-Contained Features:**  
  All code related to a feature is grouped together. For example, the "Reset Password" feature includes:
  - `ResetPasswordCommand`
  - `ResetPasswordCommandHandler`
  - `ResetPasswordValidation`
  - Related tests in the corresponding test project

- **Easy to Add or Update Features:**  
  When you need to add a new feature, you create a new folder and add all related files there. When updating a feature, you only need to look in one place.

## Clean Architecture

**Clean Architecture** is a software design approach that helps us build systems that are maintainable, testable, and scalable. It organizes code into layers with clear boundaries, so dependencies always point inward toward the core business logic. This makes it easy to change frameworks, databases, or UI without affecting the core of the application.


### How We Use Clean Architecture in This Solution

Our solution is organized into several projects, each representing a layer in Clean Architecture:

- **Business Domain Layer (`GamesGlobal.ShoppingList.BusinessDomain`):**
  - Contains core domain entities, shared business rules, and domain exceptions.
  - No dependencies on other layers.

- **Application Layer (`GamesGlobal.ShoppingList.Application`):**
  - Contains application logic, MediatR handlers, commands, queries, and validation.
  - Coordinates business rules and interacts with repositories.
  - It is the core of our application, where we define how features work.

- **Infrastructure Layer ('`GamesGlobal.ShoppingList.Infrastructure`'):**
  - Persistance - Handles data access, repositories, and database context.
  - Persistance -Implements data-access interfaces defined in the domain or application layer.
  - Infrastructure - Handles external services, such as email, and third-party APIs.
  - Infrastructure - Implements interfaces defined in domain or application layer for handling (Handles external services, such as email, and third-party APIs),

- **Presentation Layer (`GamesGlobal.ShoppingList.WebApi`):**
  - Contains API endpoints.
  - This can easily be changed without affecting the core of our application.
  - Sends requests to the application layer via MediatR.


## Specification Pattern

### What is the Specification Pattern?

The **Specification** pattern is a way to encapsulate business rules and query logic in reusable, composable objects. In our solution, the `Specification<T>` class allows us to define criteria for filtering entities (like `ShoppingItem`) in a clean, testable, and maintainable way.

### How We Use the Specification Class

- **Purpose:**  
  The `Specification<T>` class is an abstract base for creating query specifications. Each specification defines a rule or set of rules for selecting entities from a data source.

- **Key Method: `ToExpression`**  
  The `ToExpression` method is an abstract method that must be implemented by each concrete specification. It returns an `Expression<Func<T, bool>>`, which is a LINQ expression used to filter entities.

- **Composability:**  
  Specifications can be combined using methods like `And`, `OrderBy`, `Include`, etc., allowing for complex queries to be built from simple, reusable components.

- **Important Method: `Specification.WithQuery`**   
The `WithQuery` method in the `Specification<T>` class allows you to further customize or extend the query logic for your specification. It accepts a function that takes an `IQueryable<T>` and returns an `IQueryable<T>`, letting you apply additional LINQ operations (such as filtering, sorting, grouping, or projections) beyond the basic criteria defined in Specifications base class like (`And`, `OrderBy`, `Include`).


## Repository Pattern

The **Repository Pattern** is a design approach that provides a clean abstraction over data access logic. It acts as a mediator between the domain and data mapping layers, allowing you to work with domain entities without worrying about the underlying database or data source.

### Why We Use the Repository Pattern

- **Separation of Concerns:** Keeps business logic separate from data access logic.
- **Testability:** Makes it easier to mock data access in unit tests.
- **Maintainability:** Centralizes data access code, making it easier to update or refactor.
- **Flexibility:** Allows you to change the data source (e.g., switch from SQL to NoSQL) with minimal impact on the rest of the application.
- **Consistency:** Provides a consistent API for data operations across the solution.

### How We Use the Repository in This Solution

- **Abstraction:**  
  We define repository interfaces (e.g., `IApplicationRepository`) that specify methods for common data operations: `GetAsync`, `GetSingleAsync`, `Insert`, `Delete`, `SaveAsync`, etc.

- **Implementation:**  
  Concrete classes (e.g., `ApplicationRepository`) implement these interfaces using Entity Framework Core to interact with the database.

- **Specification Integration:**  
  Our repositories work with the Specification pattern. You create a specification to define your query criteria, and pass it to the repository. The repository uses the specification to build and execute the query.

#### Example Usage
```
 var spec = new FindShoppingItemById(42);
 var item = await _repository.GetSingleAsync(spec);
```

#### Common Methods

- `GetAsync(specification)`: Returns a list of entities matching the specification.
- `GetSingleAsync(specification)`: Returns a single entity matching the specification, or null if not found.
- `Insert(entity)`: Adds a new entity to the database.
- `Delete(entity)`: Removes an entity from the database.
- `SaveAsync()`: Commits changes to the database.



# Flexible Authentication 

- please see the following file 'ReadMeFlexibleAuthentication.md'