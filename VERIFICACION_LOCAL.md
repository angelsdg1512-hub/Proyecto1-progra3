# Comprobaciones de integración en su PC

Ejecute el SQL en una base nueva; los ejemplos suponen los datos ficticios iniciales. Abra la solución, restaure NuGet y compile. Puede llamar los métodos desde un proyecto consola temporal net8.0 que referencie DataAccess. No ejecute los bloques sin revisar IDs de su base real.

1. **Conexión:** TestConnection debe devolver Success=true. Un nombre de servidor inválido debe devolver Success=false y un mensaje de error.
2. **CRUD de categoría:** cree una categoría, búsquela por ID, cambie su nombre con Update y compruebe el resultado. Delete debe dejar Activo=false y GetById debe seguir encontrándola. EliminarFisicamenteSiNoTieneProductos sólo debe borrarla si está inactiva y no tiene ningún producto, incluso inactivo. Una categoría relacionada no debe borrarse.
3. **EF:** GetByCategoria(1) debe incluir Producto de prueba. Después de desactivarlo, la consulta por defecto debe excluirlo; soloActivos:false debe incluirlo. Reactívelo antes de vender.
4. **Pendiente:** stock inicial 20. Registrar dos unidades con confirmar:false debe dejar stock 20, Total=3000 y dos unidades en detalle, sin movimiento de Venta.
5. **Confirmación:** Confirmar debe bajar stock a 18 y agregar histórico anterior 20/nuevo 18/cantidad 2. Una segunda Confirmar devuelve false y no cambia stock/histórico.
6. **Anulación:** Anular de la anterior debe restaurar 20 y registrar Devolución. La segunda Anular devuelve false y no devuelve unidades nuevamente. Anular una pendiente no cambia stock.
7. **Stock insuficiente:** registrar confirmado 21 unidades cuando hay 20 debe lanzar excepción; no debe quedar un nuevo encabezado, detalle ni movimiento. No compruebe IDs consecutivos: SQL Server puede consumir IDs en un rollback.
8. **Rollback de varios productos:** cree un segundo producto con stock 1; una venta que incluye 2 del producto 1 y 2 del producto 2 debe fallar y conservar ambos stocks y el número de ventas/movimientos.
9. **Líneas repetidas:** con stock 20, dos líneas del mismo producto de 12 cada una deben fallar. Dos líneas de 2 y 3 deben descontar 5 en un movimiento agrupado.
10. **Precios/cantidades:** precio de venta o compra cero/negativo y stock negativo deben ser rechazados por restricciones de SQL. Registrar detalle de cantidad 0/precio 0 debe rechazarse antes de guardar. Los servicios deberán probar estas reglas sin SQL también.
11. **Descuentos:** dos unidades a 1500, descuento de línea 100, descuento general 50 e impuesto monto 200 deben guardar SubTotal=3000, Descuento=150 y Total=3050. Un descuento combinado superior al subtotal o un impuesto negativo se rechaza. Montos con más de dos decimales se rechazan al registrar venta.
12. **Movimiento manual:** RegistrarCambio(1,1,"Entrada",5,"Prueba") aumenta stock y registra anterior/nuevo. Salida con cambio negativo mayor al stock se rechaza sin movimiento. Ajuste admite cambio positivo o negativo. Venta no es tipo permitido para este método público.
13. **Concurrencia:** desde dos procesos, con stock 20, intente confirmar dos ventas pendientes de 15 unidades. Como máximo una debe quedar procesada; la otra falla por stock o por conflicto transaccional. El stock final nunca debe ser negativo. Si hay interbloqueo, reintente la transacción completa tras recargar datos.
14. **Usuarios:** Create almacena el hash que usted le pase; GetByNombreUsuario devuelve sólo un usuario activo. Eso no constituye autenticación: faltan hash/verificación y comprobación de empleado/persona/rol activos en Negocio.
15. **LINQ:** copie ReportesLINQ.cs a Negocio y pruebe estadísticas con lista vacía, productos activos/inactivos y ventas procesadas/anuladas. Sólo las procesadas deben contribuir a totales por cliente.

Para observar resultados:

```sql
USE tienda_db;
SELECT ProductoId,Nombre,StockActual,Activo FROM dbo.Productos;
SELECT * FROM dbo.Venta ORDER BY VentaId;
SELECT * FROM dbo.DetalleVenta ORDER BY DetalleVentaId;
SELECT * FROM dbo.HistoricoMovimientosInventario ORDER BY MovimientoId;
```

Estos pasos no han sido ejecutados en este entorno; son la verificación pendiente con Visual Studio y SQL Server locales.
