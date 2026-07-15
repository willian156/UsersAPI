# UsersAPI

Microsserviço responsável pelo cadastro, autenticação com geração de JWT e autorização de usuários. Após um cadastro bem-sucedido, publica `UserCreatedEvent`.

## Executar

```bash
dotnet run --project UsersAPI.sln
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

```bash
kubectl apply -f k8s/
```
