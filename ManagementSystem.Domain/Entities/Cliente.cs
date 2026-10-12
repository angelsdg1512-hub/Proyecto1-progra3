using System;
using System.Collections.Generic;

namespace ManagementSystem.Domain.Entities;

// Entidad: representa los campos del modelo del PDF. No ejecuta SQL.
public class Cliente
{
    public int ClienteId { get; set; }
    public int PersonaId { get; set; }
    public decimal LimiteCredito { get; set; }
    public DateTime FechaAfiliacion { get; set; } = DateTime.Now;
    public bool Activo { get; set; } = true;
}
