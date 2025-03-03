# PruebaTecnicaMicroservicio
## 📌 Descripción del Proyecto

Este proyecto implementa un backend en **.NET** con **SQL Server**, proporcionando varios endpoints para la gestión de usuarios y tareas.  

### 📂 Funcionalidades principales:
- 📌 **Gestión de Usuarios (Personas)**  
  - Registro de usuarios con clave y contraseña.  
  - Generación de **JWT Token** tras autenticación exitosa.  
  - Uso del token para autenticación en otros endpoints.  

- 📝 **Gestión de Tareas**  
  - Acceso restringido a las tareas creadas por el usuario autenticado.  
  - Operaciones **CRUD** (Crear, Leer, Actualizar y Eliminar) sobre tareas.  
  - Seguridad mediante **JWT**, asegurando que cada usuario solo pueda gestionar sus propias tareas.  

la mayoria de endpoints requieren autenticación mediante el **JWT Token**, asegurando un acceso seguro y controlado a la información del usuario.

## Instrucciones para la configuración y ejecución del sistema.

 Requisitos previos y dependencias del proyecto
Para ejecutar correctamente este proyecto, es necesario contar con los siguientes requisitos previos y herramientas:
 ### Framework y herramientas: 
- .NET Core en la versión especificada en el repositorio del proyecto.
- SQL Server Management Studio (SSMS) para gestionar las bases de datos.
- Docker instalado y configurado correctamente para ejecutar contenedores.
- Otras dependencias y paquetes con su versión recomendada también se encuentran detallados en los archivos de configuración del proyecto (como csproj y appsettings.json). .

## Clonación del Repositorio
Clona el repositorio desde la rama principal utilizando el siguiente comando:



## Preparación de los Contenedores en Docker
Ejecuta las siguientes imágenes en Docker:

- **SQL Server**: Asegúrate de asignar el puerto según las configuraciones en `appsettings.json`.

## Configuración de las Bases de Datos
Cada microservicio utiliza su propia base de datos independiente. Configura las cadenas de conexión en el archivo `appsettings.json` del microservicio. Asegúrate de que las cadenas de conexión apunten a la base de datos del contenedor SQL Server.



## Migración y Ejecución de los Microservicios
1. Navega a la carpeta de cada microservicio utilizando PowerShell o cualquier terminal de tu preferencia.
2. Ejecuta las migraciones necesarias para preparar las bases de datos con el siguiente comando:
   ```bash
   dotnet ef migrations add NombreDeLaMigracion
   ```
3. Una vez completada la migración, ejecuta el microservicio con el comando:
   ```bash
   dotnet run
   ```

## Pruebas y Endpoints
Para probar los endpoints de cada microservicio, puedes utilizar Postman o cualquier otra herramienta de cliente HTTP. Consulta la documentación del proyecto para obtener detalles sobre los endpoints disponibles y su funcionalidad.
## Personas
### post
- http://localhost:3000/api/Personas/registrar
```bash
  {
  "nombre": "Laura",
  "apellido": "Perez",
  "identificacion": "123456",
  "email": "Laura.perez@gmail.com",
  "userName": "LauPer",
  "password": "LauraPerez123$"
}
```
- http://localhost:3000/api/Personas/login
```bash
{
  "email": "Laura.perez@gmail.com",
  "password": "LauraPerez123$"
}
```
### get
- http://localhost:3000/api/Personas
- http://localhost:3000/api/Personas/all
### Delete
- http://localhost:3000/api/Personas/123456
### Put
- http://localhost:3000/api/Personas/654321
  
## Tareas
## Post
- http://localhost:3000/api/Tarea
## Get
- http://localhost:3000/api/Tarea
- http://localhost:3000/api/Tarea/1
  ## Delete
  - http://localhost:3000/api/Tarea/2
