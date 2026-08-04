# Trabajo Práctico Integrador
## Desarrollo de Software 2026

## INTEGRANTES 
| Nombre y Apellido | Legajo |
|-------------------|--------|
|Fernández, Guillermo Emanuel     |   57799     |
| Giuliante, Ian     |    58268    |
| Velloso Colombres, Nazarena     |    58340    |

## INSTRUCCIONES DE CONFIGURACIÓN Y EJECUCIÓN DEL PROYECTO

### Requisitos previos

- **Visual Studio 2022** con la carga de trabajo **"Desarrollo de ASP.NET y web"** instalada. 
- **.NET 10 SDK**.
- **SQL Server**.

### **Instrucciones**
1. Clonar los archivos del repositorio:
```
	git clone <URL-del-repo>

	cd <carpeta-del-repo>
```
2. Abrir la solución (archivo .sln)
3. Actualizar la base de datos: desde la consola del administrador de paquetes:
 ```
 	Update-Database   -Context Dsw2026TpiDbContext -Project Dsw2026Tpi.Data -StartupProject Dsw2026Tpi.Api
   
	Update-Database   -Context AuthenticationDbContext -Project Dsw2026Tpi.Data -StartupProject Dsw2026Tpi.Api
 ```
4. Correr el proyecto con CTRL+F5.
## DESCRIPCIÓN DE ENDPOINTS
### **Módulo de Autenticación**
* POST /api/auth/admin/login:
	* Se utiliza para iniciar la sesión de un usuario con rol de Administrador. Se debe enviar el email y password en el cuerpo de la petición.  Retorna un token JWT.  
* POST /api/auth/patient/login
	* Sirve para autenticar a los pacientes. Recibe un email y un dni. Si es la primera vez que el paciente ingresa, el sistema lo registra automáticamente. Retorna un token JWT.  

### **Módulo de Especialidades**
* GET /api/specialties
	* Obtiene una lista paginada de especialidades activas. Soporta parámetros de consulta (pageSize, pageIndex, name) para filtrar y paginar los resultados.  Si no se proporciona name retorna todos los registros. No incluye los registros eliminados.
* POST /api/specialties
	* Crea una nueva especialidad médica recibiendo un nombre (name) y descripción (description).  Retorna la nueva especialidad creada.
* PUT /api/specialties/{id}
	* Actualiza la información de una especialidad existente identificada por su ID.  Retorna la entidad actualizada.
* DELETE /api/specialties/{id}
	* Realiza un borrado lógico (soft delete) cambiando el estado de la especialidad para que ya no aparezca en las listas.  

### **Módulo de Médicos**
* GET /api/doctors
	* Obtiene la lista paginada de los médicos activos. También permite filtrar por nombre usando los parámetros pageSize, pageIndex y name.  Si no se proporciona name retorna todos los registros. No incluye los registros eliminados.
* GET /api/doctors/{id}/availabilities
	* Devuelve un arreglo con la disponibilidad horaria (rango de horas de inicio y fin para los días de la semana) de un médico específico.  Si no se cargo ninguna disponibilidad, retorna vacío.
* POST /api/doctors
	* Registra un nuevo médico enviando su nombre, número de licencia médica y el ID de la especialidad a la que pertenece.  Retorna la nueva especialidad creada.
 * PUT /api/doctors/{id}
	 * Actualiza los datos de un médico existente.  Retorna la entidad actualizada.
* DELETE /api/doctors/{id}
	* Ejecuta un borrado lógico del médico indicado.  
  
### **Módulo de Disponibilidades**
* POST /api/availabilities
	* Genera la agenda mensual de un médico. Se le envía el ID del doctor y un arreglo con los días de la semana, hora de inicio y hora de fin. El sistema divide este horario creando intervalos automáticos de 30 minutos para el mes en curso (ignorando feriados).  Retorna la nueva especialidad creada.
* PUT /api/availabilities
	* Actualiza o sobrescribe la disponibilidad horaria mensual del médico.  Retorna la entidad actualizada.

### **Módulo de Citas (Turnos)**
* POST /api/appointments
	* Sirve para reservar un turno médico. Requiere el envío del ID del médico, el ID del intervalo de disponibilidad (availabilitySlotId), y los datos del paciente (DNI y motivo de la consulta).  Retorna la nueva especialidad creada.
* GET /api/appointments/patient?dni=numero
	* Muestra únicamente los turnos activos reservados por un paciente (filtrando por DNI), sin incluir los cancelados o completados.  
* DELETE /api/appointments/{id}
	* Permite a un paciente cancelar su turno. Solo es válido si el estado del turno es "BOOKED" (Reservado).  
* GET /api/appointments?date=YYYY-MM-DD:
	* Endpoint administrativo para obtener todos los turnos agendados en una fecha específica. 
* GET /api/appointments/search
	* Permite a los administradores realizar una búsqueda avanzada de turnos combinando filtros por especialidad, médico, DNI del paciente, fecha, y soporte de paginación.  

## DOCUMENTACIÓN
* Acceso al [documento](https://frtutneduar-my.sharepoint.com/:b:/g/personal/franciscovicente_doc_frt_utn_edu_ar/IQD-5kaAARqnT5eL7EnPMCPgAX2LFXXX6e3p-u1C43z5rsQ?e=lbbpnz)
