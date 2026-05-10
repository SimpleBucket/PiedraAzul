using Microsoft.JSInterop;
using PiedraAzul.Client.Services.GraphQLServices;

namespace PiedraAzul.Client.Services;

/// <summary>
/// Sincroniza las citas próximas del usuario con IndexedDB para acceso offline.
/// </summary>
public class OfflineCacheService(GraphQLAppointmentService appointmentService, IJSRuntime js)
{
    /// <summary>
    /// Obtiene todas las citas próximas del usuario autenticado y las guarda en IndexedDB,
    /// reemplazando cualquier dato previo (maneja el caso de re-login automáticamente).
    /// </summary>
    public async Task SyncAsync()
    {
        try
        {
            var result = await appointmentService.GetMyUpcomingAppointmentsAsync();
            if (!result.IsSuccess) return;

            var payload = result.Value.Select(a => new
            {
                id        = a.Id,
                doctorName = a.DoctorName,
                specialty  = a.Specialty,
                date       = a.Start.ToString("o"),   // ISO 8601 — JS new Date() lo entiende
                patientName = a.PatientName,
                status     = "Scheduled"
            }).ToArray();

            await js.InvokeAsync<bool>("saveAppointmentsToIndexedDB", (object)payload);
        }
        catch
        {
            // Silent fail — la sincronización offline no debe bloquear el flujo normal
        }
    }

    /// <summary>
    /// Elimina las citas guardadas en IndexedDB (por ejemplo, al cerrar sesión).
    /// </summary>
    public async Task ClearAsync()
    {
        try
        {
            await js.InvokeAsync<bool>("saveAppointmentsToIndexedDB", Array.Empty<object>());
        }
        catch
        {
            // Silent fail
        }
    }
}
