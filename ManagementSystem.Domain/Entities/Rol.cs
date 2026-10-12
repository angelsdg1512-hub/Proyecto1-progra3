using System;
using System.Collections.Generic;

namespace ManagementSystem.Domain.Entities;

// Entidad: representa los campos del modelo del PDF. No ejecuta SQL.
public class Rol
{
    public int RolId { get; set; }
    public string NombreRol { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
