using System;
using System.Collections.Generic;

namespace ManagementSystem.Domain.Entities;

// Entidad: representa los campos del modelo del PDF. No ejecuta SQL.
public class MovimientoInventario
{
    public int MovimientoId { get; set; }
    public int ProductoId { get; set; }
    public int EmpleadoId { get; set; }
    public DateTime FechaMovimiento { get; set; } = DateTime.Now;
    public string TipoMovimiento { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public int StockAnterior { get; set; }
    public int StockNuevo { get; set; }
    public string Observacion { get; set; } = string.Empty;
}
