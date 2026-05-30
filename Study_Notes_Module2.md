# 📘 Study Notes — Módulo 2: Banco de Dados e Injeção de Dependência

**Projeto:** CashFlow API · **Stack:** .NET 10 · C# · MySQL · Entity Framework Core  
**Repositório:** [martoxm/cashflow-api](https://github.com/martoxm/cashflow-api)

---

## 🗺️ Mapa do Módulo (ordem dos commits)

| #   | Commit                                             | Conceito                     |
| --- | -------------------------------------------------- | ---------------------------- |
| 1   | `Add Expense entity and PaymentType enum`          | Mapeamento de entidades      |
| 2   | `Add CashFlowDbContext with MySQL configuration`   | DbContext + MySQL            |
| 3   | `Introduce DI and repository pattern`              | Injeção de Dependência       |
| 4   | `Refactor ExpensesRepository for DI`               | DI no repositório            |
| 5   | `Refactor DB config to use connection string`      | Configuração via appsettings |
| 6   | `Introduce Unit of Work pattern`                   | Unit of Work                 |
| 7   | `Make expense registration fully async`            | async/await                  |
| 8   | `Add AutoMapper integration`                       | AutoMapper                   |
| 9   | `Add GetAllExpenses endpoint`                      | Consulta + AutoMapper        |
| 10  | `Use AsNoTracking in GetAll`                       | Performance com EF Core      |
| 11  | `Add GetExpenseById`                               | Consulta por ID              |
| 12  | `Handle not found errors`                          | NotFoundException            |
| 13  | `Refactor repository into read/write interfaces`   | ISP (SOLID)                  |
| 14  | `Refactor exception handling to CashFlowException` | Hierarquia de exceções       |
| 15  | `Add expense deletion`                             | DELETE endpoint              |
| 16  | `Refactor DTOs and add update endpoint`            | PUT endpoint + DTO unificado |

---

## 1. Mapeamento de Entidades com Entity Framework Core

### O que é?

O EF Core usa **classes C# como entidades** para representar tabelas do banco de dados. O mapeamento é feito por convenção (nomes de propriedades) ou configuração explícita via `Fluent API`.

### Commit de referência

[`352eabf` — Add Expense entity and PaymentType enum](https://github.com/martoxm/cashflow-api/commit/352eabf31be5a69ae193157b476d9fd8d04fc61a)

### Entidade criada

```csharp
// CashFlow.Domain/Entities/Expense.cs
public class Expense
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
}
```

```csharp
// CashFlow.Domain/Enums/PaymentType.cs
public enum PaymentType
{
    Cash = 1,
    CreditCard,
    DebitCard,
    EletronicTransfer
}
```

### Conceitos-chave

- `long Id` → convenção do EF Core para **chave primária** (gerada automaticamente)
- Propriedades `string?` = nullable → coluna aceita NULL no banco
- `enum` é mapeado como `int` por padrão na coluna

---

## 2. Configuração do DbContext

### O que é o DbContext?

O `DbContext` é a **ponte entre sua aplicação e o banco de dados**. Ele gerencia conexões, rastreia entidades e executa queries SQL através do LINQ.

### Commit de referência

[`bbbe349` — Add CashFlowDbContext with MySQL configuration](https://github.com/martoxm/cashflow-api/commit/bbbe349bd5f2c841f20cc0e582c9481183ed201a)

### Implementação inicial

```csharp
// CashFlow.Infrastructure/DataAccess/CashFlowDbContext.cs
internal class CashFlowDbContext : DbContext
{
    public DbSet<Expense> Expenses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(
            "Server=localhost;Database=cashflow;Uid=root;Pwd=root;",
            ServerVersion.AutoDetect("Server=localhost;...")
        );
    }
}
```

### ⚠️ Problema: connection string hardcoded

Isso foi refatorado no próximo commit — nunca exponha credenciais no código!

### Refatoração: connection string via appsettings

[`ce7d344` — Refactor DB config to use connection string from settings](https://github.com/martoxm/cashflow-api/commit/ce7d344102147c6ab87c273ed9046ba276e4cc20)

```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=cashflow;Uid=root;Pwd=root;"
  }
}
```

```csharp
// CashFlowDbContext.cs — agora via construtor (DI)
internal class CashFlowDbContext : DbContext
{
    public CashFlowDbContext(DbContextOptions<CashFlowDbContext> options) : base(options) { }

    public DbSet<Expense> Expenses { get; set; }
}
```

```csharp
// DependencyInjectionExtension.cs
public static void AddInfrastructure(this IServiceCollection services, IConfiguration config)
{
    var connectionString = config.GetConnectionString("DefaultConnection");
    var serverVersion = ServerVersion.AutoDetect(connectionString);

    services.AddDbContext<CashFlowDbContext>(opts =>
        opts.UseMySql(connectionString, serverVersion));
}
```

### Conceitos-chave

- `DbSet<T>` representa a tabela no banco
- `DbContextOptions` permite injetar configurações de fora (DI-friendly)
- `UseMySql` + `Pomelo.EntityFrameworkCore.MySql` = integração com MySQL
- `internal` → encapsula o DbContext na camada de Infrastructure

---

## 3. Injeção de Dependências (Dependency Injection)

### O que é?

DI é um padrão onde as **dependências de uma classe são fornecidas de fora**, não instanciadas dentro dela. O .NET tem um container de DI nativo em `IServiceCollection`.

### Commit de referência

[`6c2e3bb` — Introduce Dependency Injection and repository pattern](https://github.com/martoxm/cashflow-api/commit/6c2e3bb743b42a19f56ad1bca694ff9ea25f1f2d)

### Antes (sem DI — acoplamento alto)

```csharp
public class ExpensesController : ControllerBase
{
    [HttpPost]
    public IActionResult Register(RequestRegisterExpenseJson request)
    {
        var useCase = new RegisterExpenseUseCase(); // ❌ dependência hardcoded
        useCase.Execute(request);
        ...
    }
}
```

### Depois (com DI — baixo acoplamento)

```csharp
public class ExpensesController : ControllerBase
{
    private readonly IRegisterExpenseUseCase _useCase;

    public ExpensesController(IRegisterExpenseUseCase useCase) // ✅ injetado
    {
        _useCase = useCase;
    }
}
```

### Registrando no container

```csharp
// DependencyInjectionExtension.cs
public static void AddApplication(this IServiceCollection services)
{
    services.AddScoped<IRegisterExpenseUseCase, RegisterExpenseUseCase>();
}

public static void AddInfrastructure(this IServiceCollection services, IConfiguration config)
{
    services.AddScoped<IExpensesRepository, ExpensesRepository>();
    // ... DbContext config
}
```

```csharp
// Program.cs
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
```

### Ciclos de vida dos serviços

| Lifetime    | Descrição                      | Quando usar                        |
| ----------- | ------------------------------ | ---------------------------------- |
| `Transient` | Nova instância a cada injeção  | Serviços leves, sem estado         |
| `Scoped`    | Uma instância por request HTTP | Use Cases, Repositories, DbContext |
| `Singleton` | Uma instância para toda a app  | Configurações, cache global        |

> 💡 **Regra geral:** Use `Scoped` para DbContext e tudo que depende dele.

---

## 4. Unit of Work

### O que é?

O **Unit of Work** centraliza o `SaveChanges()` — separa a responsabilidade de _persistir mudanças_ dos repositórios, que só adicionam/removem entidades no contexto.

### Commit de referência

[`712bfa5` — Introduce Unit of Work pattern](https://github.com/martoxm/cashflow-api/commit/712bfa59d9853e001e5f9f1fc0b02cd53235a094)

```csharp
// IUnitOfWork.cs
public interface IUnitOfWork
{
    Task Commit();
}

// UnitOfWork.cs
internal class UnitOfWork : IUnitOfWork
{
    private readonly CashFlowDbContext _dbContext;

    public UnitOfWork(CashFlowDbContext dbContext) => _dbContext = dbContext;

    public async Task Commit() => await _dbContext.SaveChangesAsync();
}
```

```csharp
// RegisterExpenseUseCase.cs
public class RegisterExpenseUseCase : IRegisterExpenseUseCase
{
    private readonly IExpensesRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<ResponseRegisterExpenseJson> Execute(RequestExpenseJson request)
    {
        // validações...
        var expense = _mapper.Map<Expense>(request);
        await _repository.Add(expense);
        await _unitOfWork.Commit(); // ✅ SaveChanges centralizado aqui
        return _mapper.Map<ResponseRegisterExpenseJson>(expense);
    }
}
```

### Por que separar o SaveChanges?

- Repositório só conhece **"o quê"** salvar
- Unit of Work decide **"quando"** salvar (fim da operação)
- Facilita **transações** (vários repositórios, um único commit)

---

## 5. Programação Assíncrona — async/await

### O que é?

`async/await` libera a thread enquanto espera operações de I/O (banco de dados, rede), melhorando a **escalabilidade** da API.

### Commit de referência

[`e787a19` — Make expense registration fully async](https://github.com/martoxm/cashflow-api/commit/e787a1915bed47a0b690dacbc2c85571a4eaee1e)

### Antes (síncrono)

```csharp
public ResponseRegisterExpenseJson Execute(RequestExpenseJson request)
{
    _repository.Add(expense);
    _unitOfWork.Commit();
    return response;
}
```

### Depois (assíncrono)

```csharp
// Interface
public interface IRegisterExpenseUseCase
{
    Task<ResponseRegisterExpenseJson> Execute(RequestExpenseJson request);
}

// Use Case
public async Task<ResponseRegisterExpenseJson> Execute(RequestExpenseJson request)
{
    await _repository.Add(expense);
    await _unitOfWork.Commit();
    return response;
}

// Repository
public async Task Add(Expense expense)
{
    await _dbContext.Expenses.AddAsync(expense);
}

// Unit of Work
public async Task Commit()
{
    await _dbContext.SaveChangesAsync();
}

// Controller
[HttpPost]
[ProducesResponseType(typeof(ResponseRegisterExpenseJson), StatusCodes.Status201Created)]
public async Task<IActionResult> Register(RequestExpenseJson request)
{
    var response = await _useCase.Execute(request);
    return Created(string.Empty, response);
}
```

### Regras do async/await

- Método `async` **deve** ter `await` dentro, ou compilador avisa
- `Task` = void assíncrono; `Task<T>` = retorno assíncrono
- Sempre use métodos `*Async` do EF Core: `AddAsync`, `SaveChangesAsync`, `ToListAsync`
- **Não** misture `.Result` ou `.Wait()` com `await` → deadlock!

---

## 6. AutoMapper

### O que é?

O **AutoMapper** elimina código repetitivo de mapeamento entre objetos (ex: `Expense` → `ResponseExpenseJson`).

### Commits de referência

- [`ea94460` — Add AutoMapper integration and update DI](https://github.com/martoxm/cashflow-api/commit/ea944600b0bfd907a2286630b2af8357a0d9343f)
- [`4526154` — Add AutoMapper integration and improve API responses](https://github.com/martoxm/cashflow-api/commit/452615fa8c60e872cd9fb1387c8a479355a303d0)

### Instalação

```xml
<!-- CashFlow.Application.csproj -->
<PackageReference Include="AutoMapper" Version="13.0.1" />
```

### Criando um Profile

```csharp
// AutoMapper/ExpenseProfile.cs
public class ExpenseProfile : Profile
{
    public ExpenseProfile()
    {
        // Request → Entity
        CreateMap<RequestExpenseJson, Expense>();

        // Entity → Response
        CreateMap<Expense, ResponseRegisterExpenseJson>();
        CreateMap<Expense, ResponseExpenseJson>();
        CreateMap<Expense, ResponseShortExpenseJson>();
    }
}
```

### Registrando no DI

```csharp
// DependencyInjectionExtension.cs
public static void AddApplication(this IServiceCollection services)
{
    services.AddAutoMapper(typeof(ExpenseProfile).Assembly); // ✅ sem pacote extra
}
```

### Usando no Use Case

```csharp
public class RegisterExpenseUseCase : IRegisterExpenseUseCase
{
    private readonly IMapper _mapper;

    public async Task<ResponseRegisterExpenseJson> Execute(RequestExpenseJson request)
    {
        var expense = _mapper.Map<Expense>(request);       // DTO → Entity
        await _repository.Add(expense);
        await _unitOfWork.Commit();
        return _mapper.Map<ResponseRegisterExpenseJson>(expense); // Entity → DTO
    }
}
```

### Conceitos-chave

- `Profile` = classe que define os mapeamentos
- `CreateMap<Source, Destination>()` = configura mapeamento bidirecional ou unidirecional
- Por padrão, mapeia **propriedades de mesmo nome**
- Para nomes diferentes: `.ForMember(dest => dest.X, opt => opt.MapFrom(src => src.Y))`

---

## 7. Consultas Performáticas com AsNoTracking

### O que é o tracking?

Por padrão, o EF Core **rastreia todas as entidades** retornadas de queries — armazenando o estado original para detectar mudanças no `SaveChanges`. Isso tem custo de memória e CPU.

### Commit de referência

[`7b1288a` — Use AsNoTracking in GetAll for better performance](https://github.com/martoxm/cashflow-api/commit/7b1288a0e4ea965bf0a059d6d9b99d0263e5ebe6)

```csharp
// ExpensesRepository.cs — SEM AsNoTracking (padrão)
public async Task<List<Expense>> GetAll()
{
    return await _dbContext.Expenses.ToListAsync(); // EF rastreia cada entidade
}

// COM AsNoTracking ✅
public async Task<List<Expense>> GetAll()
{
    return await _dbContext.Expenses
        .AsNoTracking()
        .ToListAsync();
}
```

### Quando usar AsNoTracking?

| Situação                     | AsNoTracking? |
| ---------------------------- | ------------- |
| Leitura pura (GET, listagem) | ✅ Sempre     |
| Vai atualizar/deletar depois | ❌ Não usar   |
| Operações de escrita         | ❌ Não usar   |

> 💡 **Regra:** Se vai apenas **ler e retornar** dados, use `AsNoTracking()` — melhora performance em 20–40% em consultas grandes.

---

## 8. SOLID na Prática

### Commit principal

[`96fd0c2` — Refactor expense repository into read/write interfaces](https://github.com/martoxm/cashflow-api/commit/96fd0c288e4ee7afc74eaaccc33c5a863c4e2697)

### Interface Segregation Principle (ISP) — "I" do SOLID

> _"Uma classe não deve ser forçada a depender de interfaces que não usa."_

#### Antes: uma interface gigante

```csharp
public interface IExpensesRepository
{
    Task Add(Expense expense);
    Task<List<Expense>> GetAll();
    Task<Expense?> GetById(long id);
    Task Delete(long id);
    Task Update(Expense expense);
}
```

#### Depois: interfaces segregadas ✅

```csharp
// IExpensesReadOnlyRepository.cs
public interface IExpensesReadOnlyRepository
{
    Task<List<Expense>> GetAll();
    Task<Expense?> GetById(long id);
}

// IExpensesWriteOnlyRepository.cs
public interface IExpensesWriteOnlyRepository
{
    Task Add(Expense expense);
    Task Delete(long id);
}

// IExpensesUpdateOnlyRepository.cs
public interface IExpensesUpdateOnlyRepository
{
    Task<Expense?> GetById(long id); // tracking habilitado para update
    Task Update(Expense expense);
}
```

```csharp
// ExpensesRepository.cs — implementa as três
internal class ExpensesRepository :
    IExpensesReadOnlyRepository,
    IExpensesWriteOnlyRepository,
    IExpensesUpdateOnlyRepository
{
    // implementações...
}
```

#### Use Cases usando apenas o que precisam

```csharp
// GetAllExpenseUseCase — só lê
public class GetAllExpenseUseCase
{
    private readonly IExpensesReadOnlyRepository _repository; // ✅ sem acesso a escrita
}

// DeleteExpenseUseCase — só escreve
public class DeleteExpenseUseCase
{
    private readonly IExpensesWriteOnlyRepository _repository; // ✅ sem acesso a leitura
}
```

### Outros princípios SOLID no projeto

| Princípio                     | Aplicação no CashFlow                                                                       |
| ----------------------------- | ------------------------------------------------------------------------------------------- |
| **S** — Single Responsibility | Cada Use Case tem **uma responsabilidade** (RegisterExpense, GetAllExpenses, DeleteExpense) |
| **O** — Open/Closed           | Novos endpoints = novos Use Cases, sem modificar os existentes                              |
| **L** — Liskov Substitution   | `ExpensesRepository` substitui qualquer interface que implementa                            |
| **I** — Interface Segregation | `IExpensesReadOnlyRepository` / `IExpensesWriteOnlyRepository`                              |
| **D** — Dependency Inversion  | Controllers dependem de interfaces (`IRegisterExpenseUseCase`), não implementações          |

---

## 9. Hierarquia de Exceções Customizadas

### Commit de referência

[`7731a30` — Refactor exception handling to use CashFlowException base](https://github.com/martoxm/cashflow-api/commit/7731a30d8b35d7b52f19180141f1215f47861c9b)

```csharp
// CashFlowException.cs — classe base abstrata
public abstract class CashFlowException : SystemException
{
    protected CashFlowException(string message) : base(message) { }

    public abstract int StatusCode { get; }
    public abstract IList<string> GetErrors();
}

// ErrorOnValidationException.cs
public class ErrorOnValidationException : CashFlowException
{
    private readonly IList<string> _errors;

    public ErrorOnValidationException(IList<string> errors) : base(string.Empty)
        => _errors = errors;

    public override int StatusCode => StatusCodes.Status400BadRequest;
    public override IList<string> GetErrors() => _errors;
}

// NotFoundException.cs
public class NotFoundException : CashFlowException
{
    public NotFoundException(string message) : base(message) { }

    public override int StatusCode => StatusCodes.Status404NotFound;
    public override IList<string> GetErrors() => [Message];
}
```

```csharp
// ExceptionFilter.cs — centralizado
public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is CashFlowException ex)
        {
            context.HttpContext.Response.StatusCode = ex.StatusCode;
            context.Result = new ObjectResult(new ResponseErrorJson(ex.GetErrors()));
        }
        // fallback para erro 500...
    }
}
```

### Por que essa hierarquia é boa?

- `ExceptionFilter` trata **qualquer** `CashFlowException` com um único `if`
- Adicionar novo tipo de erro = criar nova classe filha, sem mexer no Filter (**OCP**)
- Cada exceção define seu próprio `StatusCode` e `GetErrors()`

---

## 10. Endpoints CRUD Completos

### Fluxo completo de um endpoint (exemplo: GET /expenses/{id})

```
Request HTTP
↓
ExpensesController.GetById(long id)
↓
IGetExpenseByIdUseCase.Execute(id)
↓
IExpensesReadOnlyRepository.GetById(id) ← AsNoTracking
↓
CashFlowDbContext → MySQL Query
↓
Expense entity → AutoMapper → ResponseExpenseJson
↓
200 OK com DTO (ou 404 via NotFoundException)
```

### Resumo dos endpoints implementados

| Método   | Rota             | Use Case                 | Repository Interface         |
| -------- | ---------------- | ------------------------ | ---------------------------- |
| `POST`   | `/expenses`      | `RegisterExpenseUseCase` | `IWriteOnly` + `IUnitOfWork` |
| `GET`    | `/expenses`      | `GetAllExpenseUseCase`   | `IReadOnly` + `AsNoTracking` |
| `GET`    | `/expenses/{id}` | `GetExpenseByIdUseCase`  | `IReadOnly` + `AsNoTracking` |
| `PUT`    | `/expenses/{id}` | `UpdateExpenseUseCase`   | `IUpdateOnly` (com tracking) |
| `DELETE` | `/expenses/{id}` | `DeleteExpenseUseCase`   | `IWriteOnly` + `IUnitOfWork` |

---

## 📦 Pacotes NuGet do Módulo 2

```xml
<!-- CashFlow.Infrastructure.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="10.0.0" />

<!-- CashFlow.Application.csproj -->
<PackageReference Include="AutoMapper" Version="13.0.1" />
```

---

## ❓ Questões para Revisão

1. Qual a diferença entre `Scoped`, `Transient` e `Singleton` no DI?
2. Por que o `DbContext` deve ser `Scoped` e não `Singleton`?
3. Quando **não** usar `AsNoTracking`?
4. O que acontece se você chamar `SaveChanges` dentro do repositório em vez do Unit of Work?
5. Como o `AutoMapper` sabe como mapear propriedades com nomes diferentes?
6. Qual princípio SOLID justifica a separação de `IExpensesReadOnlyRepository` e `IExpensesWriteOnlyRepository`?
7. Por que a classe `CashFlowException` é `abstract`?
8. O que é o `DbSet<T>` e para que serve?

---

_Gerado com base nos commits do repositório [martoxm/cashflow-api](https://github.com/martoxm/cashflow-api) · Módulo 2 · Mai 2026_
