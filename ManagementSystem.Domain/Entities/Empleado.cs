using System;
using System.Collections.Generic;

namespace ManagementSystem.Domain.Entities;

// Entidad: representa los campos del modelo del PDF. No ejecuta SQL.
public class Empleado
{
    public int EmpleadoId { get; set; }
    public int PersonaId { get; set; }
    public string Puesto { get; set; } = string.Empty;
    public decimal Salario { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.Now;
    public bool Activo { get; set; } = true;
}
