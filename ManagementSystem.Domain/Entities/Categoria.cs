using System;
using System.Collections.Generic;

namespace ManagementSystem.Domain.Entities;

// Entidad: representa los campos del modelo del PDF. No ejecuta SQL.
public class Categoria
{
    public int CategoriaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public bool Activo { get; set; } = true;
}
