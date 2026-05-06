# 📚 Documentação de Estudos — CashFlow API (Módulo 1)

> **Repositório:** [cashflow-api](https://github.com/martoxm/cashflow-api)  
> **Tecnologias:** .NET 8 · C# · ASP.NET Core · Entity Framework Core · MySQL · FluentValidation · xUnit · DDD

Este documento registra minha jornada de aprendizado no Módulo 1 do curso, seguindo a ordem cronológica dos commits. Cada seção representa uma etapa do que aprendi e implementei.

---

## 📌 Índice

1. [Criação do Projeto e Estrutura Inicial](#1-criação-do-projeto-e-estrutura-inicial)
2. [Arquitetura em Camadas (DDD)](#2-arquitetura-em-camadas-ddd)
3. [Validações com Estruturas Condicionais e FluentValidation](#3-validações-com-estruturas-condicionais-e-fluentvalidation)
4. [Tratamento de Erros com Exceções Customizadas](#4-tratamento-de-erros-com-exceções-customizadas)
5. [Filtro Global de Exceções (ExceptionFilter)](#5-filtro-global-de-exceções-exceptionfilter)
6. [Centralização de Mensagens com Resource Files](#6-centralização-de-mensagens-com-resource-files)
7. [Suporte a Múltiplos Idiomas (Localização)](#7-suporte-a-múltiplos-idiomas-localização)
8. [Testes Unitários com xUnit](#8-testes-unitários-com-xunit)
9. [Utilitários de Teste com Bogus](#9-utilitários-de-teste-com-bogus)
10. [Migração para Shouldly](#10-migração-para-shouldly)
11. [Testes de Validação Completos](#11-testes-de-validação-completos)

---

## 1. Criação do Projeto e Estrutura Inicial

**Commits:**

- `Add .gitattributes, .gitignore, README.md, and LICENSE.txt.` — 04/05/2026
- `Add project files.` — 04/05/2026

### O que aprendi

O projeto foi criado pelo Visual Studio com configurações padrão de um projeto .NET. O `.gitignore` garante que arquivos de build, segredos e binários não sejam versionados.

### Estrutura gerada

```text
cashflow-api/
├── src/
│   ├── CashFlow.Api/ # Camada de apresentação (Controllers, Program.cs)
│   ├── CashFlow.Application/ # Casos de uso (Use Cases)
│   ├── CashFlow.Communication/ # DTOs de Request/Response
│   ├── CashFlow.Domain/ # Entidades, Enums, Interfaces
│   ├── CashFlow.Exception/ # Exceções customizadas
│   └── CashFlow.Infrastructure/ # Repositórios, EF Core, DbContext
└── tests/
    └── Validators.Tests/ # Testes unitários
```

### Conceito: .gitignore

O `.gitignore` define quais arquivos o Git deve **ignorar**. Para .NET, isso inclui:

- `bin/` e `obj/` — pastas de compilação
- `*.user` — configurações locais do usuário
- `appsettings.Development.json` — configurações com dados sensíveis

---

## 2. Arquitetura em Camadas (DDD)

**Conceito central aprendido nessa etapa do projeto.**

### O que é DDD (Domain-Driven Design)?

DDD é uma abordagem de arquitetura que organiza o software em torno do **domínio do negócio**. Em vez de misturar tudo num único projeto, separamos responsabilidades em camadas.

### As Camadas do Projeto

| Camada             | Projeto                   | Responsabilidade                                   |
| ------------------ | ------------------------- | -------------------------------------------------- |
| **API**            | `CashFlow.Api`            | Recebe requisições HTTP, retorna respostas         |
| **Application**    | `CashFlow.Application`    | Contém os Use Cases (regras de negócio)            |
| **Communication**  | `CashFlow.Communication`  | DTOs — contratos de Request e Response             |
| **Domain**         | `CashFlow.Domain`         | Entidades, Enums, Interfaces dos repositórios      |
| **Exception**      | `CashFlow.Exception`      | Exceções personalizadas do domínio                 |
| **Infrastructure** | `CashFlow.Infrastructure` | EF Core, DbContext, implementação dos repositórios |

### Fluxo de uma requisição

HTTP Request
↓
Controller (Api)
↓
Use Case (Application)
↓
Repository Interface (Domain) ← implementado por → Infrastructure
↓
Banco de Dados (MySQL via EF Core)
↓
Response DTO (Communication)
↓
HTTP Response

text

### Conceito: DTO (Data Transfer Object)

DTOs são classes simples usadas para transportar dados entre camadas. **Nunca exponha sua entidade de domínio diretamente na API.**

```csharp
// Communication/Requests/RegisterExpenseRequest.cs
public class RegisterExpenseRequest
{
    public string Title { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
    public string? Description { get; set; }
}
```

### Conceito: Injeção de Dependência (DI)

A DI é registrada no `Program.cs` e permite que as classes recebam suas dependências pelo construtor, sem instanciá-las manualmente.

```csharp
// Program.cs
builder.Services.AddScoped<IRegisterExpenseUseCase, RegisterExpenseUseCase>();
builder.Services.AddScoped<IExpensesRepository, ExpensesRepository>();
```

---

## 3. Validações com Estruturas Condicionais e FluentValidation

**Commit:** `Applying validations with conditional structures, Handling exceptions with try/catch, Managing packages with NuGet, Elegant validations with Fluent Validation` — 04/05/2026

### Validação Manual (antes do FluentValidation)

Antes de usar uma biblioteca, valida-se manualmente com `if`:

```csharp
if (string.IsNullOrWhiteSpace(request.Title))
    throw new Exception("O título não pode ser vazio.");

if (request.Amount <= 0)
    throw new Exception("O valor deve ser positivo.");
```

**Problema:** código verboso, difícil de manter e sem padronização.

### FluentValidation

O [FluentValidation](https://docs.fluentvalidation.net/) usa uma API fluente para escrever validações de forma elegante e reutilizável.

**Instalação via NuGet:**

```bash
dotnet add package FluentValidation
```

**Implementação:**

```csharp
// Application/UseCases/Expenses/Register/RegisterExpenseValidator.cs
public class RegisterExpenseValidator : AbstractValidator<RegisterExpenseRequest>
{
    public RegisterExpenseValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("O título não pode ser vazio.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("O valor deve ser maior que zero.");

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("A data não pode ser futura.");

        RuleFor(x => x.PaymentType)
            .IsInEnum()
            .WithMessage("Tipo de pagamento inválido.");
    }
}
```

**Uso no Use Case:**

```csharp
public class RegisterExpenseUseCase : IRegisterExpenseUseCase
{
    public async Task Execute(RegisterExpenseRequest request)
    {
        Validate(request);
        // ... salvar no banco
    }

    private void Validate(RegisterExpenseRequest request)
    {
        var validator = new RegisterExpenseValidator();
        var result = validator.Validate(request);

        if (!result.IsValid)
        {
            var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
            throw new ErrorOnValidationException(errors);
        }
    }
}
```

### Conceito: NuGet

NuGet é o gerenciador de pacotes do .NET. Para adicionar pacotes:

```bash
dotnet add package NomeDoPacote
```

Ou pelo Visual Studio: clique com botão direito no projeto → _Manage NuGet Packages_.

---

## 4. Tratamento de Erros com Exceções Customizadas

**Commit:** `Refactor expense validation error handling` — 04/05/2026

### Por que criar exceções customizadas?

Usar `throw new Exception("mensagem")` diretamente é problemático porque:

- Não carrega contexto de negócio
- É difícil de capturar de forma específica no controller
- Não suporta múltiplos erros de validação

### Hierarquia de Exceções

CashFlowException (base)
└── ErrorOnValidationException

text

```csharp
// Exception/CashFlowException.cs
public class CashFlowException : SystemException
{
    public CashFlowException(string message) : base(message) { }
}

// Exception/ErrorOnValidationException.cs
public class ErrorOnValidationException : CashFlowException
{
    public List<string> Errors { get; }

    public ErrorOnValidationException(List<string> errors)
        : base(string.Join(", ", errors))
    {
        Errors = errors;
    }
}
```

### ResponseErrorJson

Para retornar erros padronizados na API, usa-se um DTO de resposta de erro:

```csharp
// Communication/Responses/ResponseErrorJson.cs
public class ResponseErrorJson
{
    public List<string> Errors { get; set; }

    public ResponseErrorJson(string error)
    {
        Errors = new List<string> { error };
    }

    public ResponseErrorJson(List<string> errors)
    {
        Errors = errors;
    }
}
```

### Tratamento no Controller (antes do ExceptionFilter)

```csharp
[HttpPost]
public async Task<IActionResult> Register([FromBody] RegisterExpenseRequest request)
{
    try
    {
        await _useCase.Execute(request);
        return Created();
    }
    catch (ErrorOnValidationException ex)
    {
        return BadRequest(new ResponseErrorJson(ex.Errors));
    }
    catch (Exception)
    {
        return StatusCode(500, new ResponseErrorJson("Erro interno do servidor."));
    }
}
```

---

## 5. Filtro Global de Exceções (ExceptionFilter)

**Commit:** `Refactor: add global exception filter for error handling` — 05/05/2026

### O problema do try/catch em cada controller

Repetir `try/catch` em todos os controllers viola o princípio **DRY** (Don't Repeat Yourself). A solução é centralizar o tratamento de erros.

### ExceptionFilter

Um `IExceptionFilter` do ASP.NET Core intercepta todas as exceções lançadas durante o processamento de requisições.

```csharp
// Api/Filters/ExceptionFilter.cs
public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ErrorOnValidationException validationEx)
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Result = new ObjectResult(new ResponseErrorJson(validationEx.Errors));
        }
        else
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Result = new ObjectResult(new ResponseErrorJson("Erro inesperado."));
        }

        context.ExceptionHandled = true;
    }
}
```

### Registro no Program.cs

```csharp
builder.Services.AddMvc(options =>
{
    options.Filters.Add<ExceptionFilter>();
});
```

### Resultado

O controller fica limpo — apenas recebe a requisição e chama o use case:

```csharp
[HttpPost]
public async Task<IActionResult> Register([FromBody] RegisterExpenseRequest request)
{
    await _useCase.Execute(request);
    return Created();
}
```

---

## 6. Centralização de Mensagens com Resource Files

**Commit:** `Centralize error messages using resource files` — 05/05/2026

### Por que usar Resource Files?

Hardcodar strings de erro espalhadas pelo código é ruim para:

- **Manutenção** — trocar uma mensagem requer busca em todo o projeto
- **Localização** — não é possível traduzir sem Resource Files

### Como criar um Resource File (.resx)

No projeto `CashFlow.Exception`:

1. Clicar com botão direito → _Add_ → _New Item_ → _Resource File_
2. Nomear como `ResourceErrorMessages.resx`
3. Adicionar chave/valor para cada mensagem
   Chave: TITLE_REQUIRED | Valor: O título é obrigatório.
   Chave: AMOUNT_MUST_BE_GREATER_THAN_ZERO | Valor: O valor deve ser maior que zero.
   Chave: DATE_CANNOT_BE_FOR_THE_FUTURE | Valor: A data não pode ser futura.
   Chave: PAYMENT_TYPE_INVALID | Valor: Tipo de pagamento inválido.

text

O Visual Studio gera automaticamente uma classe `ResourceErrorMessages.Designer.cs`.

### Uso no Validator

```csharp
RuleFor(x => x.Title)
    .NotEmpty()
    .WithMessage(ResourceErrorMessages.TITLE_REQUIRED);

RuleFor(x => x.Amount)
    .GreaterThan(0)
    .WithMessage(ResourceErrorMessages.AMOUNT_MUST_BE_GREATER_THAN_ZERO);
```

---

## 7. Suporte a Múltiplos Idiomas (Localização)

**Commit:** `Add culture middleware and improve localization support` — 05/05/2026

### Como funciona a localização no .NET

Para suportar múltiplos idiomas, cria-se um Resource File por idioma com o sufixo da cultura:

| Arquivo                            | Idioma                |
| ---------------------------------- | --------------------- |
| `ResourceErrorMessages.resx`       | Padrão (pt-BR)        |
| `ResourceErrorMessages.en-US.resx` | Inglês americano      |
| `ResourceErrorMessages.pt-PT.resx` | Português de Portugal |
| `ResourceErrorMessages.fr.resx`    | Francês               |

### CultureMiddleware

Um middleware personalizado lê o header `Accept-Language` da requisição e define a cultura atual do thread:

```csharp
// Api/Middleware/CultureMiddleware.cs
public class CultureMiddleware
{
    private readonly RequestDelegate _next;

    public CultureMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        var supportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        var requestedCulture = context.Request.Headers.AcceptLanguage.FirstOrDefault();

        var cultureInfo = supportedCultures
            .FirstOrDefault(c => c.Name.Equals(requestedCulture))
            ?? new CultureInfo("pt-BR");

        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;

        await _next(context);
    }
}
```

### Registro no Program.cs

```csharp
app.UseMiddleware<CultureMiddleware>();
```

### Como testar

No Postman, adicione o header:
Accept-Language: en-US

text
A resposta de erro virá em inglês automaticamente.

---

## 8. Testes Unitários com xUnit

**Commit:** `Add Validators.Tests project with RegisterExpenseValidator test` — 06/05/2026

### O que são testes unitários?

Testes unitários validam **uma unidade isolada de código** (geralmente um método ou classe) sem dependência de banco de dados, rede, etc.

### Estrutura do projeto de testes

```
tests/
└── Validators.Tests/
├── Validators.Tests.csproj
└── Expenses/
└── Register/
└── RegisterExpenserValidatorTests.cs
```

### Instalação do xUnit

```bash
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
```

### Estrutura de um teste (padrão AAA)

O padrão **Arrange, Act, Assert** é a forma mais clara de organizar testes:

```csharp
[Fact]
public void Success()
{
    // Arrange — preparar os dados
    var request = new RegisterExpenseRequest
    {
        Title = "Teste",
        Amount = 100,
        Date = DateOnly.FromDateTime(DateTime.Today),
        PaymentType = PaymentType.Cash
    };
    var validator = new RegisterExpenseValidator();

    // Act — executar a ação
    var result = validator.Validate(request);

    // Assert — verificar o resultado
    Assert.True(result.IsValid);
}
```

---

## 9. Utilitários de Teste com Bogus

**Commit:** `Add CommonTestUtilities and improve test setup` — 06/05/2026

### O problema de criar dados de teste manualmente

Criar objetos de teste manualmente para cada cenário é repetitivo. A biblioteca [Bogus](https://github.com/bchavez/Bogus) gera dados fake de forma fluente.

### Instalação

```bash
dotnet add package Bogus
```

### Criando um Builder com Bogus

O padrão **Builder** centraliza a criação de objetos de teste:

```csharp
// CommonTestUtilities/Requests/RegisterExpenseRequestBuilder.cs
public class RegisterExpenseRequestBuilder
{
    public static RegisterExpenseRequest Build()
    {
        return new Faker<RegisterExpenseRequest>("pt_BR")
            .RuleFor(x => x.Title, f => f.Commerce.ProductName())
            .RuleFor(x => x.Amount, f => f.Finance.Amount(min: 1))
            .RuleFor(x => x.Date, f => f.Date.PastDateOnly())
            .RuleFor(x => x.PaymentType, f => f.PickRandom<PaymentType>())
            .RuleFor(x => x.Description, f => f.Lorem.Sentence())
            .Generate();
    }
}
```

### Uso nos testes

```csharp
[Fact]
public void Success()
{
    var request = RegisterExpenseRequestBuilder.Build();
    var validator = new RegisterExpenseValidator();

    var result = validator.Validate(request);

    Assert.True(result.IsValid);
}
```

---

## 10. Migração para Shouldly

**Commit:** `Switch tests to Shouldly and update xUnit version` — 06/05/2026

### FluentAssertions vs Shouldly

Ambas as bibliotecas melhoram a legibilidade dos asserts. O projeto migrou do FluentAssertions para o [Shouldly](https://docs.shouldly.org/).

| FluentAssertions                   | Shouldly                        |
| ---------------------------------- | ------------------------------- |
| `result.IsValid.Should().BeTrue()` | `result.IsValid.ShouldBeTrue()` |
| `errors.Should().HaveCount(1)`     | `errors.Count.ShouldBe(1)`      |
| `errors.Should().Contain("msg")`   | `errors.ShouldContain("msg")`   |

### Instalação

```bash
dotnet add package Shouldly
```

### Exemplo com Shouldly

```csharp
[Fact]
public void Error_EmptyTitle()
{
    var request = RegisterExpenseRequestBuilder.Build();
    request.Title = string.Empty;

    var validator = new RegisterExpenseValidator();
    var result = validator.Validate(request);

    result.IsValid.ShouldBeFalse();
    result.Errors.Count.ShouldBe(1);
    result.Errors.Single().ErrorMessage
        .ShouldBe(ResourceErrorMessages.TITLE_REQUIRED);
}
```

---

## 11. Testes de Validação Completos

**Commit:** `Add validation tests for RegisterExpenseValidator` — 06/05/2026

### Cobertura dos cenários de erro

Para cada regra de validação, criamos um teste de erro:

```csharp
public class RegisterExpenserValidatorTests
{
    [Fact]
    public void Success() { /* ... */ }

    [Fact]
    public void Error_EmptyTitle()
    {
        var request = RegisterExpenseRequestBuilder.Build();
        request.Title = string.Empty;
        // assert: IsValid == false, 1 erro, mensagem correta
    }

    [Fact]
    public void Error_FutureDate()
    {
        var request = RegisterExpenseRequestBuilder.Build();
        request.Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        // assert: IsValid == false
    }

    [Fact]
    public void Error_InvalidPaymentType()
    {
        var request = RegisterExpenseRequestBuilder.Build();
        request.PaymentType = (PaymentType)99; // valor inválido
        // assert: IsValid == false
    }

    [Fact]
    public void Error_AmountZeroOrNegative()
    {
        var request = RegisterExpenseRequestBuilder.Build();
        request.Amount = 0;
        // assert: IsValid == false
    }
}
```

---

## 🗺️ Resumo da Jornada — Módulo 1
```
Projeto criado + .gitignore
↓
Arquitetura DDD (6 camadas) + Injeção de Dependência
↓
FluentValidation + NuGet
↓
Exceções Customizadas + ResponseErrorJson
↓
ExceptionFilter Global (sem try/catch no controller)
↓
Resource Files (mensagens centralizadas)
↓
Localização (middleware Accept-Language)
↓
Testes Unitários com xUnit (padrão AAA)
↓
Bogus para dados fake (Builder pattern)
↓
Shouldly para asserts legíveis
↓
Cobertura completa dos casos de erro

```

---

## 🔗 Referências

- [Repositório: cashflow-api](https://github.com/martoxm/cashflow-api)
- [FluentValidation Docs](https://docs.fluentvalidation.net/)
- [Shouldly Docs](https://docs.shouldly.org/)
- [Bogus GitHub](https://github.com/bchavez/Bogus)
- [xUnit Docs](https://xunit.net/)
- [ASP.NET Core Filters](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters)
- [.NET Localization](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization)
