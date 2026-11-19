# UsersAPI

Microsserviço responsável pelo cadastro, autenticação com geração de JWT e autorização de usuários. Após um cadastro bem-sucedido, publica `UserCreatedEvent`.

## Executar

```bash
dotnet run --project UsersAPI.csproj
```

O RabbitMQ deve estar acessível com as variáveis abaixo.

## Variáveis de ambiente

- `ASPNETCORE_URLS`
- `RabbitMq__Host`
- `RabbitMq__Username`
- `RabbitMq__Password`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key`
- `Jwt__ExpiresMinutes`
- `Seed__AdminEmail`
- `Seed__AdminPassword`
- `ConnectionStrings__Database` (PostgreSQL exclusivo do serviço)

## Docker

```bash
docker build -t fcg/users-api:latest .
```

## Kubernetes

Os manifests ativos ficam no repositório de orquestração, em `k8s/` e `deploy/azure/`. Os arquivos locais de `k8s/` deste repositório são históricos da Fase 3; não os aplique junto da configuração nova. Consulte `docs/DEPLOY_AZURE_DEVOPS.md` na orquestração.

## Fase 4: testes e Azure DevOps

`dotnet test tests/UsersAPI.Tests/UsersAPI.Tests.csproj` executa os testes automatizados. `azure-pipelines.yml` valida PRs e publica a branch main no ACR/AKS com rolling update. Configure o variable group `fcg-production` conforme o tutorial central. Credenciais são obrigatórias via configuração externa; os defaults de senha/chave foram removidos.
