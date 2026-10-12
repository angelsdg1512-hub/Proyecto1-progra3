using System;
using System.Collections.Generic;

namespace ManagementSystem.Domain.Entities;

// Entidad: representa los campos del modelo del PDF. No ejecuta SQL.
public class Persona
{
    public int PersonaId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public bool Activo { get; set; } = true;
}
