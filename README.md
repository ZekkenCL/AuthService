# Auth Service

El Auth Service es un microservicio que maneja la autenticación, registro, actualización de contraseñas y validación de tokens. Además, interactúa con RabbitMQ para sincronizar los datos de los usuarios.

---

## Requisitos

### Herramientas Necesarias
- [.NET SDK](https://dotnet.microsoft.com/download) (versión compatible con el proyecto).
- [Docker](https://www.docker.com/) para la base de datos SQL Server.
- [Postman](https://www.postman.com/) o similar para pruebas.

### Archivo `.env`
Crea un archivo `.env` en la raíz del proyecto con el siguiente contenido:

```env
SQL_CONNECTION_STRING=Server=localhost,1433;Database=AuthDB;User Id=sa;Password=YourPassword
JWT_SECRET=SuperSecretJWTKeyWithAtLeast32Characters123

RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
RABBITMQ_QUEUE=auth_queue
RABBITMQ_EXCHANGE=auth_exchange
RABBITMQ_ROUTING_KEY=user.auth
```

---

## Configuración

### Base de Datos (Docker Compose)

1. Asegúrate de tener un archivo `docker-compose.yml` con el siguiente contenido:

```yaml
version: '3.8'
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2019-latest
    container_name: sqlserver-container
    ports:
      - "1433:1433"
    environment:
      SA_PASSWORD: "YourPassword"
      ACCEPT_EULA: "Y"
      TRUST_SERVER_CERTIFICATE: "true"


```

2. Levanta el servicio de la base de datos:

```bash
docker-compose up -d
```

3. Verifica que el contenedor esté corriendo:

```bash
docker ps
```

### Aplicar Migraciones

1. Aplica las migraciones de la base de datos:

```bash
dotnet ef database update
```

2. Si no tienes el CLI de Entity Framework, instálalo con:

```bash
dotnet tool install --global dotnet-ef
```

---

## Levantar el Servicio

1. Carga las dependencias:

```bash
dotnet restore
```


2. Ejecuta el proyecto:

```bash
dotnet run
```

3. Accede a la documentación de Swagger para probar los endpoints:

```
http://localhost:5092/swagger/index.html
```

---

## Endpoints Disponibles

### Autenticación

- **Registro de Usuario**: `POST /api/Auth/register`
  ```json
  {
    "firstName": "John",
    "lastName": "Doe",
    "secondLastName": "Smith",
    "rut": "12345678-9",
    "email": "user@example.com",
    "careerId": 1,
    "password": "password123"
  }
  ```

- **Inicio de Sesión**: `POST /api/Auth/login`
  ```json
  {
    "email": "user@example.com",
    "password": "password123"
  }
  ```

### Tokens

- **Validar Token**: `POST /api/Token/validate-token`
  ```json
  {
    "token": "your_jwt_token_here"
  }
  ```

- **Revocar Token**: `POST /api/Token/revoke-token`
  ```json
  {
    "token": "your_jwt_token_here"
  }
  ```

### Actualización de Contraseña

- **Actualizar Contraseña**: `PUT /api/Auth/update-password`
  ```json
  {
    "currentPassword": "old_password",
    "newPassword": "new_password",
    "confirmPassword": "new_password"
  }
  ```





---

## Notas Finales

- Asegúrate de que las configuraciones de conexión coincidan con tu entorno.
- Este microservicio debe integrarse con los demás servicios del sistema para un funcionamiento completo.
- Utiliza Postman o Swagger para probar los endpoints y verificar las respuestas.

---

© 2024 - Arquitectura de Sistemas