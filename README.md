# Salon Appointment System

## 📋 Descripción
Sistema de gestión de citas para peluquerías desarrollado como proyecto académico para la Universidad de Almería.

## 👥 Autores
- **Johan Eduardo Cala Torra** - [JohanCalaT](https://github.com/JohanCalaT)
- **jct576** - [jct576](https://github.com/jct576)

## 🚀 Tecnologías
- .NET 10.0
- C#
- .NET Aspire (Orquestación de microservicios)
- Blazor (Frontend)
- ASP.NET Core Web API (Backend)

## 📁 Estructura del Proyecto

El proyecto está organizado en tres carpetas principales:

```
📦 ProyectoDesarrolloWeb
├── 📁 1.Backend/
│   └── SalonAppointmentSystem.ApiService      # API REST del sistema
├── 📁 2.Frontend/
│   └── SalonAppointmentSystem.Web             # Aplicación Blazor
└── 📁 3.Orchestrator/
    ├── SalonAppointmentSystem.AppHost         # Orquestador .NET Aspire
    └── SalonAppointmentSystem.ServiceDefaults # Configuración compartida
```

### Descripción de Componentes

- **1.Backend**: Contiene la API REST que maneja la lógica de negocio y acceso a datos
- **2.Frontend**: Aplicación web Blazor Server para la interfaz de usuario
- **3.Orchestrator**:
  - `AppHost`: Orquestador de .NET Aspire que gestiona todos los servicios
  - `ServiceDefaults`: Configuraciones compartidas (telemetría, health checks, service discovery)

## 🏃‍♂️ Cómo Ejecutar

Para ejecutar el proyecto completo:

```bash
cd 3.Orchestrator/SalonAppointmentSystem.AppHost
dotnet run
```

Esto iniciará el dashboard de Aspire y todos los servicios configurados.

## 📅 Fecha de Creación
25 de Noviembre de 2025

## 📝 Contribuir
Este proyecto utiliza **co-autoría en commits** para mantener registro en ambos perfiles de GitHub.

**⚠️ IMPORTANTE:** Todos los commits deben incluir la línea de co-autoría.

Ver [CONTRIBUTING.md](CONTRIBUTING.md) para instrucciones detalladas sobre cómo hacer commits correctamente.

### Ejemplo rápido:
```bash
git commit -m "Tu mensaje" -m "" -m "Co-authored-by: jct576 <jct576@inlumine.ual.es>"
```

## 🏫 Institución
Universidad de Almería (UAL)

---

*Este repositorio está configurado con co-autoría para mantener registro en múltiples perfiles de GitHub*

