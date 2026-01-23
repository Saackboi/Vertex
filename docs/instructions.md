# 📋 Prompt Maestro: Especificación Técnica Backend (VERTEX)

**Rol:** Actúa como un Desarrollador Backend Senior experto en .NET 9.
**Objetivo:** Construir la infraestructura base del proyecto "VERTEX" siguiendo estrictamente **Clean Architecture**.

**Instrucciones Generales:**
No asumas implementación. Sigue las reglas de dependencia de la arquitectura Onion. Tu tarea es generar el código C# y los comandos de terminal necesarios para cumplir con los siguientes requerimientos paso a paso.

---

## 1. Arquitectura y Estructura de Proyectos
Debes generar los comandos CLI (`dotnet`) para crear una Solución (`.sln`) que contenga exactamente estos 4 proyectos con las siguientes relaciones de dependencia:

1.  **`Vertex.Domain`**: (Núcleo) No debe tener dependencias de otros proyectos.
2.  **`Vertex.Application`**: Debe depender únicamente de `Vertex.Domain`.
3.  **`Vertex.Infrastructure`**: Debe depender de `Vertex.Application` y `Vertex.Domain`. Aquí instalarás las librerías de Entity Framework Core y SQL Server.
4.  **`Vertex.API`**: Debe depender de `Vertex.Application` y `Vertex.Infrastructure`.

---

## 2. Modelado del Dominio (Capa `Vertex.Domain`)
Dentro del proyecto de Dominio, define las siguientes Entidades como clases POCO (Plain Old CLR Objects):

* [cite_start]**Entidad `OnboardingProcess`:** [cite: 49]
    * [cite_start]Debe tener un identificador único (GUID)[cite: 51].
    * [cite_start]Debe almacenar el ID del usuario (string) que viene del sistema de identidad[cite: 52].
    * [cite_start]Debe tener un entero para controlar el paso actual del formulario (`CurrentStep`, default 1)[cite: 53].
    * [cite_start]Debe tener un campo de texto largo para almacenar el JSON crudo del formulario (`SerializedData`)[cite: 54].
    * [cite_start]Debe incluir campos de auditoría (`UpdatedAt`) [cite: 55] [cite_start]y estado (`IsCompleted`)[cite: 56].

* [cite_start]**Entidad `ProfessionalProfile`:** [cite: 57]
    * Esta entidad representa el CV finalizado (Resultado Final).
    * [cite_start]Debe tener campos para el Nombre Completo (`FullName`), Resumen Profesional (`Summary`) y un campo para almacenar las Habilidades (Skills) en formato JSON [cite: 59-62].

---

## 3. Lógica de Aplicación (Capa `Vertex.Application`)
Define los contratos y objetos de transporte necesarios para desacoplar el núcleo de la base de datos:

* **Interfaces:** Crea una interfaz `IOnboardingRepository` que defina los métodos para:
    1.  [cite_start]Obtener un proceso por ID de usuario[cite: 32].
    2.  [cite_start]Guardar o actualizar un proceso (Upsert)[cite: 33].
* **DTOs:** Crea un objeto de transferencia de datos (`SaveProgressDto`) que contenga solo la información necesaria que envía el Frontend (Paso actual y Data serializada), para no exponer la entidad de dominio directamente al controlador.

---

## 4. Infraestructura de Persistencia (Capa `Vertex.Infrastructure`)
Implementa la lógica real de acceso a datos usando **Entity Framework Core**:

* **Contexto de Datos:** Crea una clase que herede de `IdentityDbContext` (preparado para seguridad futura) e incluye los `DbSet` para las entidades definidas arriba.
* **Configuración:** Usa `OnModelCreating` para asegurar que el campo de JSON tenga el tipo de dato correcto para texto largo en SQL Server.
* **Repositorio:** Implementa la interfaz `IOnboardingRepository`.
    * **Regla de Negocio Crítica:** En el método de guardar, debes verificar primero si ya existe un registro para ese usuario. [cite_start]Si existe, actualízalo; si no existe, crea uno nuevo[cite: 66, 67]. **No permitas duplicados para un mismo usuario.**

---

## 5. API REST (Capa `Vertex.API`)
Expone la lógica de negocio mediante Controladores HTTP:

* **Configuración:** Registra el DbContext (usando SQL Server) y la Inyección de Dependencias del Repositorio en el contenedor de servicios (`Program.cs`).
* **Controlador (`OnboardingController`):**
    * Crea un endpoint `POST /save` para guardar el progreso. [cite_start]Debe recibir el DTO, mapearlo a la entidad de dominio y llamar al repositorio[cite: 33].
    * Crea un endpoint `GET /resume` para recuperar el estado. [cite_start]Debe retornar el `CurrentStep` y `SerializedData`[cite: 32, 72].
    * [cite_start]*Nota:* Por ahora usa un ID de usuario harcodeado/temporal, pero deja comentarios indicando dónde iría la extracción del usuario vía Token JWT[cite: 32].

---

**Instrucción Final:**
Analiza estos requerimientos y procede a generar:
1.  Los comandos de creación de estructura.
2.  El código de las clases solicitadas capa por capa, explicando brevemente la responsabilidad de cada archivo generado.# 📋 Prompt Maestro: Especificación Técnica Backend (VERTEX)

**Rol:** Actúa como un Desarrollador Backend Senior experto en .NET 9.
**Objetivo:** Construir la infraestructura base del proyecto "VERTEX" siguiendo estrictamente **Clean Architecture**.

**Instrucciones Generales:**
No asumas implementación. Sigue las reglas de dependencia de la arquitectura Onion. Tu tarea es generar el código C# y los comandos de terminal necesarios para cumplir con los siguientes requerimientos paso a paso.

---

## 1. Arquitectura y Estructura de Proyectos
Debes generar los comandos CLI (`dotnet`) para crear una Solución (`.sln`) que contenga exactamente estos 4 proyectos con las siguientes relaciones de dependencia:

1.  **`Vertex.Domain`**: (Núcleo) No debe tener dependencias de otros proyectos.
2.  **`Vertex.Application`**: Debe depender únicamente de `Vertex.Domain`.
3.  **`Vertex.Infrastructure`**: Debe depender de `Vertex.Application` y `Vertex.Domain`. Aquí instalarás las librerías de Entity Framework Core y SQL Server.
4.  **`Vertex.API`**: Debe depender de `Vertex.Application` y `Vertex.Infrastructure`.

---

## 2. Modelado del Dominio (Capa `Vertex.Domain`)
Dentro del proyecto de Dominio, define las siguientes Entidades como clases POCO (Plain Old CLR Objects):

* [cite_start]**Entidad `OnboardingProcess`:** [cite: 49]
    * [cite_start]Debe tener un identificador único (GUID)[cite: 51].
    * [cite_start]Debe almacenar el ID del usuario (string) que viene del sistema de identidad[cite: 52].
    * [cite_start]Debe tener un entero para controlar el paso actual del formulario (`CurrentStep`, default 1)[cite: 53].
    * [cite_start]Debe tener un campo de texto largo para almacenar el JSON crudo del formulario (`SerializedData`)[cite: 54].
    * [cite_start]Debe incluir campos de auditoría (`UpdatedAt`) [cite: 55] [cite_start]y estado (`IsCompleted`)[cite: 56].

* [cite_start]**Entidad `ProfessionalProfile`:** [cite: 57]
    * Esta entidad representa el CV finalizado (Resultado Final).
    * [cite_start]Debe tener campos para el Nombre Completo (`FullName`), Resumen Profesional (`Summary`) y un campo para almacenar las Habilidades (Skills) en formato JSON [cite: 59-62].

---

## 3. Lógica de Aplicación (Capa `Vertex.Application`)
Define los contratos y objetos de transporte necesarios para desacoplar el núcleo de la base de datos:

* **Interfaces:** Crea una interfaz `IOnboardingRepository` que defina los métodos para:
    1.  [cite_start]Obtener un proceso por ID de usuario[cite: 32].
    2.  [cite_start]Guardar o actualizar un proceso (Upsert)[cite: 33].
* **DTOs:** Crea un objeto de transferencia de datos (`SaveProgressDto`) que contenga solo la información necesaria que envía el Frontend (Paso actual y Data serializada), para no exponer la entidad de dominio directamente al controlador.

---

## 4. Infraestructura de Persistencia (Capa `Vertex.Infrastructure`)
Implementa la lógica real de acceso a datos usando **Entity Framework Core**:

* **Contexto de Datos:** Crea una clase que herede de `IdentityDbContext` (preparado para seguridad futura) e incluye los `DbSet` para las entidades definidas arriba.
* **Configuración:** Usa `OnModelCreating` para asegurar que el campo de JSON tenga el tipo de dato correcto para texto largo en SQL Server.
* **Repositorio:** Implementa la interfaz `IOnboardingRepository`.
    * **Regla de Negocio Crítica:** En el método de guardar, debes verificar primero si ya existe un registro para ese usuario. [cite_start]Si existe, actualízalo; si no existe, crea uno nuevo[cite: 66, 67]. **No permitas duplicados para un mismo usuario.**

---

## 5. API REST (Capa `Vertex.API`)
Expone la lógica de negocio mediante Controladores HTTP:

* **Configuración:** Registra el DbContext (usando SQL Server) y la Inyección de Dependencias del Repositorio en el contenedor de servicios (`Program.cs`).
* **Controlador (`OnboardingController`):**
    * Crea un endpoint `POST /save` para guardar el progreso. [cite_start]Debe recibir el DTO, mapearlo a la entidad de dominio y llamar al repositorio[cite: 33].
    * Crea un endpoint `GET /resume` para recuperar el estado. [cite_start]Debe retornar el `CurrentStep` y `SerializedData`[cite: 32, 72].
    * [cite_start]*Nota:* Por ahora usa un ID de usuario harcodeado/temporal, pero deja comentarios indicando dónde iría la extracción del usuario vía Token JWT[cite: 32].

---

**Instrucción Final:**
Analiza estos requerimientos y procede a generar:
1.  Los comandos de creación de estructura.
2.  El código de las clases solicitadas capa por capa, explicando brevemente la responsabilidad de cada archivo generado.
3. Esa explicación breve ve documentándola paso a paso según lo vas haciendo en un archiv docs/documentacion.md