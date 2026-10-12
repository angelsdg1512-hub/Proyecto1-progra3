using System;
using System.Collections.Generic;

namespace ManagementSystem.Domain.Entities;

// Entidad: representa los campos del modelo del PDF. No ejecuta SQL.
public class Venta
{
    public int VentaId { get; set; }
    public int ClienteId { get; set; }
    public int EmpleadoId { get; set; }
    public DateTime FechaVenta { get; set; } = DateTime.Now;
    public decimal SubTotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string Observaciones { get; set; } = string.Empty;
    public List<DetalleVenta> Detalles { get; set; } = new();
}
