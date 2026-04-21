# MyHub Catalog API

## Visao Geral

API REST para gerenciamento do catalogo de servicos do MyHub. Permite criar e gerenciar **Planos**, **Servicos** e **Funcoes**, com relacionamentos flexiveis entre essas entidades.

## Arquitetura

Projeto organizado em 4 camadas seguindo Clean Architecture:

- **CatalogAPI** - Controllers REST, configuracao do pipeline
- **CatalogApplication** - Application Services, resultado padronizado (ServiceResult)
- **CatalogDomain** - Entidades ricas, Value Objects, DTOs, Validators
- **CatalogInfrastructure** - Repositorios, DbContext (EF Core + MySQL)

## Tecnologias

- .NET 9.0
- Entity Framework Core 9 (Pomelo MySQL)
- FluentValidation
- RabbitMQ (MessageBus)
- JWT Authentication
- Docker

## Endpoints

### Health
- `GET /api/catalogo/health` - Health check (DB + RabbitMQ)
- `GET /api/catalogo/health/ping` - Ping

### Planos
- `GET    /api/catalogo/planos` - Listar planos
- `GET    /api/catalogo/planos/ativos` - Listar planos ativos
- `GET    /api/catalogo/planos/{codPlano}` - Obter plano
- `POST   /api/catalogo/planos` - Criar plano
- `PUT    /api/catalogo/planos/{codPlano}` - Atualizar plano
- `DELETE /api/catalogo/planos/{codPlano}` - Excluir plano
- `GET    /api/catalogo/planos/{codPlano}/servicos` - Servicos do plano
- `PUT    /api/catalogo/planos/{codPlano}/servicos` - Vincular servicos ao plano

### Servicos
- `GET    /api/catalogo/servicos` - Listar servicos
- `GET    /api/catalogo/servicos/{codServico}` - Obter servico
- `POST   /api/catalogo/servicos` - Criar servico
- `PUT    /api/catalogo/servicos/{codServico}` - Atualizar servico
- `DELETE /api/catalogo/servicos/{codServico}` - Excluir servico
- `GET    /api/catalogo/servicos/{codServico}/planos` - Planos do servico

### Funcoes
- `GET    /api/catalogo/servicos/{codServico}/funcoes` - Listar funcoes
- `POST   /api/catalogo/servicos/{codServico}/funcoes` - Criar funcao
- `PUT    /api/catalogo/servicos/{codServico}/funcoes/{codFuncao}` - Atualizar funcao
- `DELETE /api/catalogo/servicos/{codServico}/funcoes/{codFuncao}` - Remover funcao

## Formato de Resposta

Todas as respostas seguem o envelope padronizado:

**Sucesso com dados:**
```json
{ "success": true, "message": "Plano criado com sucesso.", "data": { ... } }
```

**Sucesso sem dados (DELETE):**
```
HTTP 204 No Content
```

**Erro:**
```json
{ "success": false, "message": "Descricao do erro.", "errors": ["detalhe"] }
```

## Entidades

- **Plano** - Plano de assinatura com valor base, status e cobranca
- **Servico** - Servico que pode pertencer a multiplos planos
- **Funcao** - Funcionalidade especifica de um servico (com ordem de exibicao)
- **PlanoServico** - Relacionamento N:N entre Plano e Servico

## Como Executar

### Pre-requisitos
- .NET 9.0 SDK
- MySQL
- RabbitMQ
- Docker (opcional)

### Execucao Local

```
cd CatalogAPI
dotnet restore
dotnet run
```

### Execucao com Docker

```
docker compose up
```

## Estrutura de Pastas

```
myhub-api-catalog/
|-- CatalogAPI/
|   |-- Controllers/          # PlanoController, ServicoController, FuncaoController, HealthController
|   |-- Program.cs            # Pipeline e DI
|-- CatalogApplication/
|   |-- Common/               # ServiceResult
|   |-- Services/             # Application Services (Plano, Servico, Funcao, PlanoServico)
|   |-- Responders/           # CatalogResponder (RabbitMQ)
|-- CatalogDomain/
|   |-- Entities/             # Plano, Servico, Funcao, PlanoServico, Entity, IAggregateRoot
|   |-- ValueObjects/         # CodigoPlano, Dinheiro
|   |-- Dtos/                 # DTOs e Requests
|   |-- Validators/           # FluentValidation
|-- CatalogInfrastructure/
|   |-- Context/              # CatalogContext (EF Core)
|   |-- Repositories/         # PlanoRepository, ServicoRepository, etc.
```

## Dependencias Externas

Componentes compartilhados em `myhub-api-all-components`:
- **DomainObjects** - IUnitOfWork, ConnectionSettings
- **MicroserviceCore** - JWT, CORS, Swagger, Error handling
- **MessageBus** - RabbitMQ
- **IntegrationHandlers** - Request/Response para comunicacao entre APIs
- **SecurityCore** - Seguranca
