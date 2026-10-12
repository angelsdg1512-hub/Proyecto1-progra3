using System;
using System.Collections.Generic;

namespace ManagementSystem.Domain.Entities;

// Entidad: representa los campos del modelo del PDF. No ejecuta SQL.
public class Usuario
{
    public int UsuarioId { get; set; }
    public int EmpleadoId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;
    public int RolId { get; set; }
    public bool Activo { get; set; } = true;
}
