# 💸 CashFlow API

> API de controle de despesas pessoais desenvolvida em **.NET 8**, aplicando arquitetura **DDD (Domain Driven Design)**, princípios **SOLID**, **Entity Framework Core** com MySQL e boas práticas modernas de desenvolvimento backend.

---

## 📌 Status do Projeto

> **Projeto em desenvolvimento e aprendizado contínuo.**

Este repositório faz parte da minha jornada de estudos em **.NET 8** e arquitetura de software. O projeto ainda não está 100% finalizado, pois está sendo construído conforme avanço nas aulas e nas implementações práticas.

A proposta deste repositório é registrar minha evolução como desenvolvedor, aplicando aos poucos conceitos importantes como **DDD**, **SOLID**, **injeção de dependência**, **persistência de dados**, **testes** e geração de relatórios.

---

## 🎯 Objetivo do Projeto

O **CashFlow** é uma aplicação backend para controle de despesas pessoais. Mais do que entregar uma aplicação pronta, este projeto tem como objetivo servir como base prática para consolidar conhecimentos em:

- Arquitetura em camadas
- Separação de responsabilidades
- Boas práticas com APIs REST
- Escrita de código limpo e manutenível
- Modelagem de domínio
- Integração com banco de dados

## 🚧 O que estou aprendendo neste projeto

### 📦 Módulo 1 — Primeiros passos criando a nossa API


- [x] Fundamentos do **DDD (Domain Driven Design)**
- [x] Escrita de código limpo, seguro e orientado ao domínio
- [x] Aplicação de validações com **estruturas condicionais**
- [x] Tratamento de exceções com **try/catch**
- [x] Gerenciamento de pacotes com **NuGet**
- [x] Validações elegantes com **Fluent Validation**
-  Criação de **filtros personalizados de exceções**
-  Escrita de **testes de unidade** para garantir qualidade e confiabilidade

---

### 🗄️ Módulo 2 — Banco de dados e Injeção de dependência


-  Integração da aplicação .NET com **MySQL**
-  Mapeamento de entidades com **Entity Framework Core**
-  Configuração do **DbContext**
-  Injeção de dependências com **Dependency Injection**
-  Simplificação de transformações de dados com **AutoMapper**
-  Consultas performáticas com **AsNoTracking**
-  Programação assíncrona com **métodos async/await**
-  Princípios **SOLID** aplicados na prática

---

### 📊 Módulo 3 — Gerando relatórios em Excel e PDF


-  Geração de relatórios profissionais em **PDF**
-  Exportação de dados para **Excel**
-  Versionamento do projeto com **GitHub**
-  Colaboração com outras pessoas desenvolvedoras
-  Manutenção do histórico do código de forma segura e organizada

---

## 🛠️ Tecnologias Utilizadas

| Tecnologia | Finalidade |
|---|---|
| .NET 8 | Framework principal |
| ASP.NET Core | Criação da API REST |
| Entity Framework Core | ORM para acesso ao banco de dados |
| MySQL | Banco de dados relacional |
| Fluent Validation | Validação de dados |
| AutoMapper | Mapeamento entre objetos |
| xUnit | Testes de unidade |
| Swagger / Scalar | Documentação da API |
| GitHub | Controle de versão |

---

## 🏗️ Arquitetura

O projeto segue os princípios do **Domain Driven Design (DDD)**, com separação clara de responsabilidades:

```
src/
├── CashFlow.API/            # Camada de apresentação (Controllers, Filters)
├── CashFlow.Application/    # Casos de uso e regras de aplicação
├── CashFlow.Domain/         # Entidades, interfaces e regras de domínio
└── CashFlow.Infrastructure/ # Repositórios, DbContext e serviços externos
```

---

## 🚀 Como executar o projeto

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [MySQL](https://www.mysql.com/)
- [Visual Studio](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/)

### Passos

```bash
# Clone o repositório
git clone https://github.com/martoxm/cashflow-api.git

# Acesse a pasta do projeto
cd cashflow-api

# Configure a string de conexão no appsettings.json
# Depois execute as migrations
dotnet ef database update

# Execute o projeto
dotnet run --project src/CashFlow.API
```

---

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

<p align="center">Desenvolvido por <strong>Gabriel Martorelli</strong> 🚀</p>
