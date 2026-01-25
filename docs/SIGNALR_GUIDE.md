# SignalR - Documentación de Implementación

## 📋 Estructura Implementada

### 1. **Capa de Aplicación** (Vertex.Application)
- **INotificationService.cs**: Interfaz de abstracción para enviar notificaciones en tiempo real
  - `NotifyOnboardingProgressAsync()`: Notifica progreso del onboarding
  - `NotifyOnboardingCompletedAsync()`: Notifica finalización del onboarding
  - `NotifyAllAsync()`: Broadcast a todos los usuarios
  - `NotifyGroupAsync()`: Notifica a un grupo específico

### 2. **Capa de Infraestructura** (Vertex.Infrastructure)
- **SignalRNotificationService.cs**: Implementación de INotificationService usando SignalR
  - Utiliza `IHubContext<NotificationHub>` para enviar mensajes
  - Mantiene separación de capas (Application no depende de SignalR directamente)

### 3. **Capa de Presentación** (Vertex.API)
- **NotificationHub.cs**: Hub de SignalR con autenticación JWT
  - Requiere `[Authorize]` - solo usuarios autenticados
  - Al conectarse, agrega al usuario a un grupo con su UserId
  - Métodos invocables: `JoinGroup()`, `LeaveGroup()`, `Ping()`

### 4. **Configuración** (Program.cs)
```csharp
// Registro de servicios
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();

// JWT con soporte para SignalR (query string token)
options.Events = new JwtBearerEvents {
    OnMessageReceived = context => {
        var accessToken = context.Request.Query["access_token"];
        if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
            context.Token = accessToken;
    }
};

// CORS con WebSockets
.SetIsOriginAllowed(_ => true)

// Mapeo del Hub
app.MapHub<NotificationHub>("/hubs/notifications");
```

## 🔌 Uso desde Angular

### Instalación
```bash
npm install @microsoft/signalr
```

### Conexión desde el Cliente
```typescript
import * as signalR from '@microsoft/signalr';

export class SignalRService {
  private hubConnection: signalR.HubConnection;

  constructor() {
    const token = localStorage.getItem('jwt_token');
    
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5131/hubs/notifications', {
        accessTokenFactory: () => token || '',
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();

    this.startConnection();
  }

  private startConnection() {
    this.hubConnection
      .start()
      .then(() => console.log('✅ Conectado a SignalR'))
      .catch(err => console.error('❌ Error de conexión:', err));
  }

  // Escuchar eventos del servidor
  onOnboardingProgress(callback: (data: any) => void) {
    this.hubConnection.on('OnboardingProgress', callback);
  }

  onOnboardingCompleted(callback: (data: any) => void) {
    this.hubConnection.on('OnboardingCompleted', callback);
  }

  onNotification(callback: (data: any) => void) {
    this.hubConnection.on('Notification', callback);
  }

  // Invocar métodos del servidor
  ping() {
    return this.hubConnection.invoke('Ping');
  }

  joinGroup(groupName: string) {
    return this.hubConnection.invoke('JoinGroup', groupName);
  }

  leaveGroup(groupName: string) {
    return this.hubConnection.invoke('LeaveGroup', groupName);
  }

  disconnect() {
    this.hubConnection.stop();
  }
}
```

### Uso en Componentes
```typescript
export class OnboardingComponent implements OnInit, OnDestroy {
  constructor(private signalR: SignalRService) {}

  ngOnInit() {
    // Escuchar progreso del onboarding
    this.signalR.onOnboardingProgress((data) => {
      console.log(`Paso ${data.currentStep}: ${data.message}`);
      this.showNotification(data.message);
    });

    // Escuchar completación
    this.signalR.onOnboardingCompleted((data) => {
      console.log('¡Onboarding completado!', data);
      this.router.navigate(['/profile', data.profileId]);
    });
  }

  ngOnDestroy() {
    this.signalR.disconnect();
  }
}
```

## 🔧 Uso desde el Backend

### En OnboardingService (o cualquier servicio)
```csharp
public class OnboardingService : IOnboardingService
{
    private readonly INotificationService _notificationService;

    public OnboardingService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<OnboardingProgressDto>> SaveProgressAsync(string userId, OnboardingDataDto data)
    {
        // ... lógica de negocio ...

        // Notificar al usuario sobre el progreso
        await _notificationService.NotifyOnboardingProgressAsync(
            userId, 
            "Progreso guardado exitosamente", 
            data.CurrentStep
        );

        return ApiResponse<OnboardingProgressDto>.SuccessResponse(result);
    }

    public async Task<ApiResponse<ProfessionalProfileDto>> CompleteOnboardingAsync(string userId, OnboardingDataDto data)
    {
        // ... crear perfil profesional ...

        // Notificar completación
        await _notificationService.NotifyOnboardingCompletedAsync(userId, profileId);

        return ApiResponse<ProfessionalProfileDto>.SuccessResponse(profile);
    }

    // Notificar a todos (ejemplo: mantenimiento del sistema)
    public async Task NotifyMaintenanceAsync()
    {
        await _notificationService.NotifyAllAsync(
            "El sistema estará en mantenimiento en 10 minutos"
        );
    }
}
```

## 🎯 Eventos Disponibles

### Del Servidor → Cliente
| Evento | Datos | Descripción |
|--------|-------|-------------|
| `OnboardingProgress` | `{ Id, UserId, Title, Message, Type, CurrentStep, Read, Timestamp }` | Progreso del onboarding |
| `OnboardingCompleted` | `{ Id, UserId, Title, Message, Type, ProfileId, Read, Timestamp }` | Onboarding completado |
| `Notification` | `{ Id, UserId, Title, Message, Type, Read, Timestamp, Data? }` | Notificación general personalizada |
| `GroupNotification` | `{ Id, Title, Message, Type, GroupName, Read, Timestamp }` | Notificación a grupo |
| `NotificationRead` | `notificationId: string` | Confirmación de notificación marcada como leída |
| `AllNotificationsRead` | - | Confirmación de todas las notificaciones marcadas como leídas |
| `Pong` | `DateTime` | Respuesta a Ping |

### Del Cliente → Servidor
| Método | Parámetros | Descripción |
|--------|-----------|-------------|
| `Ping` | - | Test de conexión |
| `MarkAsRead` | `notificationId: string` | Marcar una notificación como leída |
| `MarkAllAsRead` | - | Marcar todas las notificaciones del usuario como leídas |
| `JoinGroup` | `groupName: string` | Unirse a un grupo |
| `LeaveGroup` | `groupName: string` | Salir de un grupo |

## 🔒 Seguridad

✅ **Autenticación JWT Requerida**
- El Hub requiere `[Authorize]`
- Token JWT se envía en query string: `?access_token=YOUR_JWT_TOKEN`
- Solo usuarios autenticados pueden conectarse

✅ **Grupos por Usuario**
- Cada usuario se agrega automáticamente a un grupo con su UserId
- Permite enviar mensajes personalizados sin exponer ConnectionId

✅ **CORS Configurado**
- WebSockets habilitados con `SetIsOriginAllowed(_ => true)`
- Solo orígenes permitidos: localhost:4200, 3000, 5173

## 🧪 Testing

### Test de Conexión con PowerShell (No recomendado - mejor usar Angular)
```powershell
# SignalR requiere cliente WebSocket - difícil de probar con PowerShell
# Mejor usar Angular o cliente .NET
```

### Test con Postman (Limitado)
Postman no soporta WebSockets de SignalR completamente. Usa Angular.

### Test Recomendado
1. Ejecutar API: `dotnet run`
2. Abrir navegador: `http://localhost:5131/hubs/notifications`
3. Usar consola de desarrollo para probar conexión

## 📊 Diagrama de Flujo

```
┌─────────────────┐         WebSocket         ┌──────────────────┐
│  Angular Client │ ←────────────────────────→ │  NotificationHub │
│   (Frontend)    │  JWT Token in QueryString  │   (Vertex.API)   │
└─────────────────┘                            └──────────────────┘
        ↓ Eventos                                        ↑
        ↓ OnboardingProgress                             │ IHubContext
        ↓ OnboardingCompleted                            │
        ↓ Notification                                   │
                                              ┌──────────────────────────┐
                                              │ SignalRNotificationService│
                                              │   (Infrastructure)        │
                                              └──────────────────────────┘
                                                        ↑
                                                        │ INotificationService
                                                        │
                                              ┌──────────────────┐
                                              │ OnboardingService │
                                              │  (Application)    │
                                              └──────────────────┘
```

## ✅ Ventajas de esta Implementación

1. **Separación de Capas**: Application no depende de SignalR directamente
2. **Testeable**: INotificationService puede mockearse fácilmente
3. **Seguro**: Requiere autenticación JWT
4. **Escalable**: Usa grupos para mensajes personalizados
5. **Reconexión Automática**: `withAutomaticReconnect()` en cliente
6. **Clean Architecture**: Respeta SOLID y DDD

## 🚀 Próximos Pasos

1. Integrar notificaciones en OnboardingService
2. Crear componente de notificaciones en Angular
3. Agregar persistencia de notificaciones (opcional)
4. Implementar grupos por roles (ej: "Recruiters", "Candidates")
5. Agregar telemetría y logging de conexiones
