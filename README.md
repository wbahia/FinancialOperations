# Financial Operations System

Sistema completo para processamento de operações financeiras com suporte a múltiplas contas, transações reversíveis, processamento assíncrono e controle de concorrência.

##  Arquitetura

O projeto segue os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, dividido em camadas bem definidas:

```
FinancialOperations/
├── src/
│   ├── FinancialOperations.Domain/          # Entidades, Value Objects, Eventos
│   ├── FinancialOperations.Application/     # Use Cases, Interfaces, Validadores
│   ├── FinancialOperations.Infrastructure/  # Repositórios, Event Publisher
│   ├── FinancialOperations.Api/             # Controllers, DTOs, API
├── tests/
│   ├── FinancialOperations.UnitTests/       # Testes unitários


```

##  Tecnologias Utilizadas

- **.NET CORE 8.0** - Framework principal
- **C#** - Linguagem de programação
- **FluentValidation** - Validação de entrada
- **Shouldly** - Biblioteca de asserts para testes
- **xUnit** - Framework de testes
- **Moq** - Framework de mocking
- **Swagger** - Documentação da API

##  Funcionalidades

### Operações Bancárias
- ✅ **Crédito** - Adicionar fundos à conta
- ✅ **Débito** - Remover fundos da conta
- ✅ **Reserva** - Reservar fundos para posterior captura
- ✅ **Captura** - Confirmar fundos reservados
- ✅ **Estorno** - Devolver fundos reservados
- ✅ **Transferência** - Transferir fundos entre contas

### Características Técnicas
-  **Controle de Concorrência** - Lock por conta para operações simultâneas
-  **Processamento Assíncrono** - Todas as operações são async/await
-  **Padrão Observer** - Notificação de eventos de transação
-  **Resiliência** - Retry com backoff exponencial
-  **Performance** - Uso eficiente de memória
-  **Testabilidade** - Cobertura completa de testes

##  Como Executar

### Pré-requisitos
- .NET 8.0 SDK
- Visual Studio 2022 ou VS Code

### 1. Clonar o Repositório
```bash
git clone <url-do-repositorio>
cd FinancialOperations
```

### 2. Restaurar Dependências
```bash
dotnet restore
```

### 3. Executar Testes
```bash
# Testes unitários
dotnet test tests/FinancialOperations.UnitTests/

# Todos os testes com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

### 4. Executar a API
```bash
cd src/FinancialOperations.Api
dotnet run
```
A API estará disponível em: `https://localhost:7000` e `http://localhost:5000`


## Uso da API

### Endpoints Principais

#### Crédito
```http
POST /api/accounts/{accountId}/credit
Content-Type: application/json

{
  "amount": 500.00,
  "description": "Depósito"
}
```

#### Débito
```http
POST /api/accounts/{accountId}/debit
Content-Type: application/json

{
  "amount": 200.00,
  "description": "Saque"
}
```

#### Transferência
```http
POST /api/accounts/transfer
Content-Type: application/json

{
  "fromAccountId": "guid-conta-origem",
  "toAccountId": "guid-conta-destino",
  "amount": 100.00,
  "description": "Transferência"
}
```

#### Listar Clientes
```http
GET /api/customers
```

#### Contas do Cliente
```http
GET /api/customers/{customerId}/accounts
```

### Swagger UI
Acesse `https://localhost:7000/swagger` para documentação interativa da API.

## Estratégia de Testes

### Testes Unitários (90+ casos)
- **Domain**: Entidades e regras de negócio
- **Use Cases**: Lógica de aplicação
- **Validators**: Validação de entrada
- **Mocking**: Isolamento de dependências

### Cobertura de Testes
```bash
# Gerar relatório de cobertura
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Visualizar cobertura (necessário instalar reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:Html


## Observabilidade

### Logs Estruturados
- Todas as transações são logadas com timestamps
- Eventos de domínio são capturados via Observer
- Falhas e retries são registrados

### Métricas de Performance
- Tempo de resposta das operações
- Throughput de transações
- Taxa de sucesso/falha

## Padrões Implementados

### Arquiteturais
- **Clean Architecture** - Separação de responsabilidades
- **Domain-Driven Design** - Modelagem rica do domínio
- **CQRS** - Separação de comandos e consultas
- **Repository Pattern** - Abstração de acesso a dados

### Comportamentais
- **Observer** - Notificação de eventos
- **Strategy** - Diferentes tipos de transação
- **Command** - Encapsulamento de operações

### Criacionais
- **Factory** - Criação de entidades
- **Dependency Injection** - Inversão de controle

## Regras de Negócio

### Contas
- Cada cliente pode ter múltiplas contas
- Cada conta possui saldo disponível, reservado e limite de crédito
- Status da conta (Ativa, Bloqueada, Fechada)

### Transações
- Todas as operações são idempotentes
- Controle de concorrência por conta
- Histórico completo de auditoria
- Suporte a rollback/estorno

### Validações
- Valores devem ser positivos
- Verificação de saldo suficiente
- Contas devem existir e estar ativas
- Transferências não podem ser para a mesma conta


## Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para detalhes.

## Autor

Desenvolvido como parte do desafio técnico, demonstrando conhecimentos em:
- Arquitetura de software escalável
- Padrões de design
- Programação assíncrona
- Testes automatizados
- Containerização
- Boas práticas de desenvolvimento

⭐ **Se este projeto foi útil, não esqueça de dar uma estrela!**
