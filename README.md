# 💻 Task Management API (ASP.NET Core Web API)

Uma API RESTful robusta, construída em C# com ASP.NET Core, projetada para gerenciar dados de tarefas, projetos e usuários, com segurança baseada em JWT (JSON Web Tokens).

## 🚀 Tecnologias e Ferramentas

* **Linguagem:** C#
* **Framework:** ASP.NET Core Web API
* **Banco de Dados:** [Mencione o DB: Ex: PostgreSQL / SQL Server]
* **ORM:** [Mencione: Ex: Entity Framework Core]
* **Segurança:** JSON Web Tokens (JWT) para autenticação e BCrypt para hashing de senhas.
* **Documentação:** Swagger/OpenAPI (para testes e visualização de endpoints).

## 💡 Arquitetura e Estrutura

A API segue o princípio da **Separação de Preocupações**, com uma arquitetura que inclui:

* **Controllers:** Responsáveis por receber as requisições HTTP e retornar as respostas.
* **Services:** Contêm a lógica de negócio principal.
* **Repositories:** Abstraem a interação com o banco de dados.
* **Autenticação:** Serviços dedicados para registro, login e geração de tokens JWT.

## ✨ Endpoints Chave

| Método | Endpoint | Descrição | Requer Token? |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/Auth/register` | Cria um novo usuário. | Não |
| `POST` | `/api/Auth/login` | Autentica e retorna um Token JWT. | Não |
| `GET` | `/api/Tasks` | Lista todas as tarefas do usuário autenticado. | Sim |
| `POST` | `/api/Tasks` | Cria uma nova tarefa. | Sim |
| `PUT` | `/api/Tasks/{id}` | Atualiza uma tarefa existente. | Sim |

## 🔗 Link para o Frontend

Esta API é consumida pela aplicação cliente Task Management:

* **Repositório do Cliente:** https://github.com/joaovitorsamora/TaskManagement-Client-ReactTS

## 🛠 Como Rodar Localmente

1.  Clone este repositório: `git clone [URL]`
2.  **Configuração do Banco de Dados:** Configure a *Connection String* no arquivo `appsettings.json`.
3.  **Migrações:** Rode as migrações do Entity Framework (se aplicável) para criar o esquema do banco de dados.
4.  **Execução:** Inicie o projeto no Visual Studio ou via linha de comando: `dotnet run`
5.  Acesse o Swagger em `http://localhost:[Porta]/swagger` para testar os endpoints.
