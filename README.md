# AtivoPlus

Projeto desenvolvido no âmbito da disciplina de **Engenharia de Software 2**.

## Descrição

AtivoPlus é uma aplicação de gestão de ativos financeiros que permite aos utilizadores gerir as suas carteiras de investimento de forma organizada e segura. O sistema oferece funcionalidades para criação e gestão de carteiras, adição de diversos tipos de ativos financeiros e controlo de acesso baseado em permissões.

## Componentes do Projeto

- **Backend**: API REST desenvolvida em ASP.NET Core com Entity Framework (este repositório)
- **Frontend**: Aplicação complementar disponível em [AtivoPlusFrontend](https://github.com/JotaBarbosaDev/AtivoPlusFrontend)

## Funcionalidades Principais

- **Gestão de Utilizadores**: Sistema de autenticação e autorização
- **Carteiras**: Criação e gestão de carteiras de investimento
- **Ativos Financeiros**: Suporte para diferentes tipos de ativos:
  - Fundos de Investimento
  - Imóveis Arrendados
  - Depósitos a Prazo
- **Controlo de Acesso**: Sistema de permissões com níveis de administrador
- **Integração Externa**: Dados de mercado através da API Twelve Data
- **Cálculo de Lucros**: Análise de rentabilidade dos investimentos

## Tecnologias Utilizadas

- **Backend**: ASP.NET Core 9.0, Entity Framework Core
- **Base de Dados**: PostgreSQL
- **Testes**: xUnit para testes unitários
- **Documentação**: Swagger/OpenAPI
- **APIs Externas**: Twelve Data API para dados de mercado

## Estrutura do Projeto

```
AtivoPlus/
├── controllers/     # Controladores da API REST
├── data/           # Camada de acesso a dados (Entity Framework)
├── logic/          # Lógica de negócio
├── models/         # Modelos de dados e entidades
├── AtivoPlus.Tests/ # Testes unitários
└── TwelveJson/     # Cache de dados de mercado
```

## Como Executar

### Pré-requisitos
- .NET 9.0 SDK
- PostgreSQL
- Chave API da Twelve Data

### Configuração

1. **Configurar a base de dados PostgreSQL**
   ```bash
   # Criar base de dados PostgreSQL
   createdb ativoplus
   ```

2. **Configurar variáveis de ambiente**
   ```bash
   # Criar ficheiro .env na raiz do projeto
   echo "TWELVE_API=your_api_key_here" > .env
   ```

3. **Configurar string de conexão**
   - Editar `appsettings.json` com os dados da base de dados

4. **Executar migrações**
   ```bash
   dotnet ef database update
   ```

5. **Iniciar a aplicação**
   ```bash
   dotnet run
   ```

A API estará disponível em `https://localhost:5001` com documentação Swagger.

### Adicionar Administrador

```bash
dotnet run -- --addadmin nome_do_utilizador
```

## Testes

O projeto inclui uma suite completa de testes unitários que cobrem:
- Lógica de negócio dos ativos financeiros
- Controlo de acesso e permissões
- Operações CRUD das carteiras
- Validações de dados

Execute os testes com:
```bash
dotnet test
```

## API Endpoints Principais

- `/api/carteira` - Gestão de carteiras
- `/api/ativo-financeiro` - Gestão de ativos financeiros
- `/api/fundo-investimento` - Fundos de investimento
- `/api/deposito-prazo` - Depósitos a prazo
- `/api/imovel-arrendado` - Imóveis arrendados
- `/api/user` - Gestão de utilizadores
- `/api/banco` - Gestão de bancos

## Funcionalidades de Segurança

- Autenticação baseada em utilizador
- Sistema de permissões (admin/user)
- Validação de propriedade de recursos
- Proteção contra acesso não autorizado

## Integração com Dados de Mercado

O sistema integra com a API Twelve Data para obter:
- Cotações em tempo real
- Dados históricos de preços
- Informações sobre stocks, ETFs, criptomoedas

## Contribuidores

Projeto desenvolvido como parte da cadeira de Engenharia de Software 2.
