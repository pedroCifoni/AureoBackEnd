# Aureo API

API REST de **organização financeira pessoal**: cada usuário cadastra e acompanha suas receitas e despesas. Consumida pelo [AureoFrontEnd](https://github.com/pedroCifoni/AureoFrontEnd) ([ver online](https://aureo-front-end.vercel.app)).

## Stack

- **C# / .NET 10** (ASP.NET Core, Controllers)
- **PostgreSQL** com **Entity Framework Core** (Npgsql)
- **Autenticação JWT** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Scalar / OpenAPI** para documentação interativa
- **Rate limiting** no endpoint de login
- **Docker** (build multi-stage)
- Testes com **xUnit**, **Moq**, **FluentAssertions** e **EF Core InMemory**

## Arquitetura

Solução em camadas, com dependências apontando para o domínio:

```
Aureo.Api             → controllers, autenticação, tratamento global de exceções
Aureo.Application     → serviços, DTOs e interfaces dos casos de uso
Aureo.Domain          → entidades, regras de negócio, exceções e filtros
Aureo.Infrastructure  → EF Core, acesso a dados e repositórios
Aureo.Tests           → testes de controllers, domínio, repositórios e serviços
```

Decisões de projeto:

- **Regras no domínio:** a entidade `Transacao` valida seus próprios dados (descrição e categoria obrigatórias, valor maior que zero, tipo e data válidos) e expõe construtor privado com fábrica `Criar`.
- **Valor sempre positivo:** o sinal vem do tipo da transação (`Receita` ou `Despesa`).
- **Datas em UTC:** normalizadas antes de persistir.
- **Isolamento por usuário:** o `UsuarioId` vem do token JWT, e cada usuário só acessa as próprias transações.
- **Erros centralizados:** exceções de domínio e de autorização são convertidas em respostas HTTP em um único ponto.

## Endpoints

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| POST | `/api/usuarios` | Cadastra um usuário | não |
| POST | `/api/usuarios/login` | Autentica e retorna o token | não (com rate limit) |
| GET | `/api/transacoes` | Lista as transações do usuário, com filtros via query string | JWT |
| GET | `/api/transacoes/{id}` | Busca uma transação | JWT |
| POST | `/api/transacoes` | Cria uma transação | JWT |
| PUT | `/api/transacoes/{id}` | Atualiza uma transação | JWT |
| DELETE | `/api/transacoes/{id}` | Remove uma transação | JWT |

## Como rodar

Pré-requisitos: .NET 10 SDK e um PostgreSQL acessível.

```bash
git clone https://github.com/pedroCifoni/AureoBackEnd.git
cd AureoBackEnd
```

Configure a connection string do PostgreSQL e a chave do JWT (em `appsettings.json` ou, de preferência, com `dotnet user-secrets` no projeto `Aureo.Api`). Depois:

```bash
dotnet run --project Aureo.Api
```

### Testes

```bash
dotnet test
```

### Docker

```bash
docker build -t aureo-api .
docker run -p 8080:8080 aureo-api
```

A imagem expõe a porta `8080` e já aceita *forwarded headers*, para uso atrás de proxy reverso (Render, Railway, Fly etc.).

## Autor

Pedro Cifoni · [LinkedIn](https://www.linkedin.com/in/pedro-cifoni-montezuma/)
