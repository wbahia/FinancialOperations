using FinancialOperations.Application.Interfaces;
using FinancialOperations.Application.UseCases;
using FinancialOperations.Infrastructure.Events;
using FinancialOperations.Infrastructure.Repositories;
using FluentValidation;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Financial Operations API", Version = "v1" });
});

// Repositorios
builder.Services.AddSingleton<ICustomerRepository, InMemoryCustomerRepository>();
builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();

// Eventos
builder.Services.AddSingleton<ConsoleEventPublisher>();
builder.Services.AddSingleton<IEventPublisher>(provider => provider.GetRequiredService<ConsoleEventPublisher>());
builder.Services.AddSingleton<TransactionObserver>();

// Use Cases
builder.Services.AddScoped<ProcessCreditUseCase>();
builder.Services.AddScoped<ProcessDebitUseCase>();
builder.Services.AddScoped<ProcessTransferUseCase>();
builder.Services.AddScoped<ProcessReserveUseCase>();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<ProcessCreditValidator>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Observer
var eventPublisher = app.Services.GetRequiredService<ConsoleEventPublisher>();
var transactionObserver = app.Services.GetRequiredService<TransactionObserver>();
eventPublisher.Subscribe(transactionObserver);

// Dados de exemplo
await InitializeSampleDataAsync(app.Services);

app.Run();

static async Task InitializeSampleDataAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var customerRepo = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
    var accountRepo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Dados de exemplo...");

    var customers = new[]
    {
        new FinancialOperations.Domain.Entities.Customer("Walter Bahia", "12345678901", "walter@email.com"),
        new FinancialOperations.Domain.Entities.Customer("Alice Rodrigues", "98765432109", "alice@email.com"),
        new FinancialOperations.Domain.Entities.Customer("Victor Bahia", "45678912345", "victor@email.com")
    };

    foreach (var customer in customers)
    {
        await customerRepo.AddAsync(customer);

        // Cria contas para cada cliente
        var account1 = customer.CreateAccount(1000); // limite de 1000
        var account2 = customer.CreateAccount(500);  // limite 500

        // Saldo inicial
        account1.Credit(500, "Deposito inicial");
        account2.Credit(300, "Deposito inicial");

        await accountRepo.AddAsync(account1);
        await accountRepo.AddAsync(account2);

        logger.LogInformation("Cliente criado nome: {Name}. Com 2 contas.", customer.Name);
    }

    logger.LogInformation("Dados de exemplo concluidos");
}