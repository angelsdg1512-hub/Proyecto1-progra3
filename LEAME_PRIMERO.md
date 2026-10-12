# Código de acceso a datos — Programación III

Este paquete implementa las carpetas solicitadas **Interfaces, ADONET y EntityFramework**, con la conexión del profesor. Incluye entidades y SQL porque las interfaces y los repositorios necesitan tipos y tablas reales. No es la entrega completa del curso: no contiene las ventanas, servicios de negocio, pedidos, historial para deshacer, pruebas unitarias de negocio, login completo ni manuales finales. No debe presentarse como si ya cumpliera los 100 puntos.

## 1. Lo que dice el PDF y lo que elegimos

El PDF propone diez tablas y enumera sus campos; se respetan todos. No proporciona código de las entidades, tipos/longitudes SQL, versión exacta de .NET, nombres de repositorios ni tasas de impuesto. Los nombres de métodos de Producto y el comportamiento de DatabaseConfig son los del profesor compartidos en la conversación. El resto es implementación nuestra, identificada aquí; no código atribuido al profesor.

| Tema | Implementación elegida |
|---|---|
| Plataforma | .NET 8 / C# 12, nullable habilitado, EF Core 8. No es una versión fijada por el PDF. |
| BD | tienda_db, para coincidir con DatabaseConfig. |
| IDs | int / IDENTITY; claves foráneas sin borrado en cascada. |
| Montos | decimal(18,2); cantidades de inventario enteras. |
| Texto | nvarchar con longitudes explícitas en SQL y parámetros. |
| Eliminación | Delete desactiva registros con Activo. Categoría incluye un DELETE físico para una categoría inactiva sin productos, para demostrar el DELETE ADO.NET exigido. |
| Cliente y empleado | Relación 1 a 1 mediante PersonaId UNIQUE en cada tabla. Una misma persona puede ser cliente y empleado. |
| Histórico | HistóricoMovimientosInventario se escribe sin tilde en SQL: HistoricoMovimientosInventario. La clase C# se llama MovimientoInventario. |
| Fecha | DateTime.Now en objetos y SYSDATETIME() en movimientos. No se define otra política horaria. |
| Stock bajo | StockActual <= StockMinimo, sólo activos. El PDF no precisa si debe incluir igualdad. |
| Ventas | Pendiente, Procesada y Anulada. Confirmar una procesada y anular una anulada devuelven false. |
| Impuesto | Monto absoluto que envía Negocio. No se inventa una tasa como 13 %. |
| Descuentos | Montos absolutos, no porcentajes. Cada detalle conserva SubTotal bruto = Cantidad × PrecioUnitario. Venta.Descuento almacena descuentos de líneas + descuento general. Total = suma de subtotales + Impuesto − descuento combinado. |

**Ambigüedad del PDF:** pide campos Impuesto/Descuento y también dice que el total debe corresponder a la suma de subtotales. La fórmula elegida arriba incorpora esos campos; con impuesto y descuentos cero, Total es exactamente la suma de subtotales. Debe acordarse esa interpretación si el profesor exige literalmente otra. No se aplica ninguna tasa fiscal.

Las entidades aquí son modelos de datos con propiedades públicas. La demostración de encapsulamiento y herencia exigida por el PDF aún debe implementarse con sentido en Dominio/Negocio; no se afirma que estos DTO por sí solos satisfagan ese apartado. El modelo relacional separa Persona de Cliente/Empleado; no se usa herencia EF automáticamente, porque eso cambiaría el mapeo.

## 2. Cómo usarlo en Visual Studio

1. Descomprima y abra **ManagementSystem.sln**. Son dos bibliotecas de clases, no una aplicación ejecutable. Visual Studio restaurará paquetes NuGet; instale el SDK de .NET 8 si falta.
2. Si desea integrarlo en su solución existente, agregue ManagementSystem.Domain y copie las carpetas de DataAccess. Añada en DataAccess la referencia de proyecto a Domain. No mantenga dos copias de DatabaseConfig.
3. DataAccess.csproj referencia Microsoft.EntityFrameworkCore.SqlServer 8.0.* y System.Configuration.ConfigurationManager 8.0.*. El proveedor EF trae Microsoft.Data.SqlClient como dependencia transitiva. El comodín permite restaurar un parche de la familia 8; no se afirma un parche concreto probado. Si su solución usa otro framework, adapte ambos proyectos y EF a una combinación compatible; este paquete no está dirigido a .NET Framework clásico.
4. Ejecute **Sql/01_CrearBase.sql** una sola vez en SQL Server Management Studio contra una BD nueva. No borra tablas ni datos existentes. Si ya existen tablas, compare el esquema antes de ejecutarlo. GO es separador de SSMS, no sentencia de SqlCommand.
5. Configure el servidor real. `Server=.` apunta a la instancia predeterminada local. Express puede requerir `.\SQLEXPRESS` y LocalDB `(localdb)\MSSQLLocalDB`.
6. Pruebe DatabaseConfig.TestConnection() desde su proyecto ejecutable/ventana. Luego compile la solución.

Ejemplo de configuración en el **App.config del proyecto ejecutable**, no sólo en la biblioteca:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <connectionStrings>
    <add name="FacturacionConnection"
         connectionString="Server=.;Database=tienda_db;Integrated Security=True;TrustServerCertificate=True;"
         providerName="Microsoft.Data.SqlClient" />
  </connectionStrings>
</configuration>
```

Alternativa antes de crear repositorios/contextos:

```csharp
DatabaseConfig.ConnectionString = @"Server=.\SQLEXPRESS;Database=tienda_db;Integrated Security=True;TrustServerCertificate=True;";
```

Si FacturacionConnection existe, el constructor estático la prefiere a la cadena predeterminada. TrustServerCertificate se conserva del profesor para desarrollo local.

**IRepository.cs:** reemplace la interfaz anterior con nombres Agregar/Actualizar por la de este paquete. No cree dos interfaces con el mismo nombre/namespace. Este contrato genérico usa GetAll/GetById/Create/Update/Delete. **IProductoRepository permanece independiente**, tal como lo compartió el profesor, sin herencia añadida.

## 3. Qué va en cada carpeta

| Carpeta | Archivos / función |
|---|---|
| Connection | DatabaseConfig.cs: crear conexión y probarla. |
| Interfaces | IRepository y contratos de producto, categoría, persona, cliente, empleado, usuario, rol, venta, detalle, movimiento y consulta EF. |
| ADONET | RepositoryBase, SqlExecutor, RowMapper y los diez repositorios. |
| EntityFramework | ManagementDbContext y ProductoConsultaEF. Sólo consultas de productos/categorías. |
| Domain/Entities | Diez clases con todos los campos del PDF; Venta añade una List de detalles. |
| Sql | Creación de las diez tablas, restricciones y datos de prueba ficticios. |
| Ejemplos | ReportesLINQ.cs, para copiar posteriormente a Negocio; no se compila dentro de estas bibliotecas. |

No hay CRUD suelto de detalles ni cambios arbitrarios a una venta procesada: esas escrituras deben mantener juntas venta, detalles, stock e histórico. DetalleVentaRepository consulta detalles. MovimientoInventarioRepository consulta historial y registra cambios de stock con una transacción; no permite borrar histórico. Usuarios almacena ContrasenaHash; NO implementa verificación de contraseñas ni guarda contraseñas en texto plano. El SQL no crea credenciales ficticias.

## 4. Entender el código sin enredarse

**Entidad:** objeto que transporta datos, por ejemplo Producto. No abre conexiones.

**Interfaz:** contrato. `int Create(Producto producto)` obliga a ofrecer ese método; hemos definido su entero como el ID nuevo. `Producto?` puede ser null si no se encuentra; `bool` indica si se afectó un registro o se pudo cambiar el estado.

**RepositoryBase<T>:** reutiliza el CRUD. T se sustituye por Categoria, Persona, etc. Las clases hijas indican tabla/campos, cómo pasar propiedades a parámetros y cómo convertir una fila SQL en objeto. Es herencia de implementación; no sustituye la herencia de entidades/abstracción de negocio que el proyecto también pide.

**SqlExecutor:** abre una conexión por operación, ejecuta y la cierra en finally. `using` libera conexiones, comandos, lectores y transacciones. Si SQL falla, se propaga la excepción: Negocio/UI debe mostrar un mensaje claro; no se oculta como una lista vacía.

**Parámetros:** `@id`, `@Nombre`, etc. contienen valores. Los nombres de tablas/columnas se arman sólo con constantes internas; nunca se concatena el texto escrito por un usuario como SQL.

**ExecuteReader:** SELECT de varias columnas/filas. Se recorre con while(reader.Read()).

**ExecuteScalar:** una celda; OUTPUT INSERTED devuelve el ID creado.

**ExecuteNonQuery:** INSERT/UPDATE/DELETE; devuelve cantidad de filas afectadas.

**Transacción:** si se queda sin stock cualquier producto, se revierte TODA la venta: encabezado, detalles, movimientos y stock. Serializable y UPDLOCK/HOLDLOCK protegen la lectura/modificación de stock. Se agrupan las líneas repetidas del mismo producto. Interbloqueos aún pueden ocurrir; una excepción revierte la operación y Negocio debe permitir reintentar la operación completa de forma controlada.

**EF:** `using var context` libera recursos; AsNoTracking evita seguimiento porque sólo leemos. Where/OrderBy se traducen a SQL; ToList ejecuta la consulta. EF usa la misma conexión/esquema que ADO.NET; no use EnsureCreated ni migraciones para crear un esquema diferente.

## 5. Ejemplos de uso (en Negocio, no SQL dentro de botones)

```csharp
using ManagementSystem.Domain.Entities;
using ManagementSystem.DataAccess.Interfaces;
using ManagementSystem.DataAccess.ADONET;
using ManagementSystem.DataAccess.EntityFramework;

IProductoRepository productos = new ProductoRepository();
List<Producto> lista = productos.GetAll();
Producto? producto = productos.GetById(1);
List<Producto> encontrados = productos.Search("prueba");

ICategoriaRepository categorias = new CategoriaRepository();
int idCategoria = categorias.Create(new Categoria
{
    Nombre = "Limpieza", Descripcion = "Artículos de limpieza", Activo = true
});
categorias.CambiarEstado(idCategoria, false);
// Delete(id) es borrado lógico. EliminarFisicamenteSiNoTieneProductos(id)
// es la operación explícita de borrado físico; no usarla en el mantenimiento habitual.

IProductoConsultaEF consulta = new ProductoConsultaEF();
List<Producto> deCategoria = consulta.GetByCategoria(1);
List<Producto> pocoStock = consulta.GetStockBajo();

IVentaRepository ventas = new VentaRepository();
var venta = new Venta
{
    ClienteId = 1, EmpleadoId = 1,
    Impuesto = 0, Descuento = 0,
    Detalles = new List<DetalleVenta>
    {
        new() { ProductoId = 1, Cantidad = 2, PrecioUnitario = 1500, Descuento = 0 }
    }
};
int idVenta = ventas.Registrar(venta, confirmar: false); // Guarda pendiente; stock sigue igual.
bool confirmada = ventas.Confirmar(idVenta); // Descuenta dos y registra movimiento.
Venta? guardada = ventas.GetById(idVenta); // Incluye detalles y sus IDs.
bool anulada = ventas.Anular(idVenta); // Devuelve dos al stock si estaba procesada.
```

`Registrar(venta, true)` guarda y confirma en una única transacción. `Registrar` sólo admite objetos con VentaId=0. No lo llame nuevamente con una venta ya registrada. Create de otros repositorios devuelve el ID y no modifica automáticamente el ID del objeto; asígnele el resultado si lo necesita. GetAll de ventas trae encabezados; GetById trae encabezado + detalles.

**UpdateStock del profesor:** es una primitiva que descuenta stock de forma condicional; no recibe EmpleadoId, así que no puede llenar el histórico completo. Se mantiene la firma y se documenta esa limitación. Para confirmar ventas use SIEMPRE VentaRepository. Para entradas/salidas/ajustes use RegistrarCambio, que coordina stock e histórico. Editar Producto con Update no modifica StockActual ni FechaRegistro; esos campos se ignoran en esa actualización. Create sí permite existencias iniciales.

Las restricciones SQL protegen precios, stock y totales; los servicios futuros deben además validar nombres, identificación, email, longitudes, permisos y reglas de negocio ANTES de llamar al repositorio. No hay permiso/autorización implementado en esta capa. GetAll incluye activos e inactivos para administración.

## 6. Correspondencia con el PDF

| Requisito | Dónde está / estado |
|---|---|
| Todos los campos de las diez tablas | Entities y 01_CrearBase.sql. |
| PK y FK | Script SQL, sin cascadas. |
| INSERT, SELECT, UPDATE ADO.NET | RepositoryBase y repositorios específicos. |
| DELETE ADO.NET | CategoriaRepository.EliminarFisicamenteSiNoTieneProductos. |
| SqlConnection/Command/Reader/parámetros | DatabaseConfig, SqlExecutor y repositorios. |
| using + finally | SqlExecutor (todas las operaciones ADO.NET de repositorios pasan por él). TestConnection conserva using del profesor. |
| Venta + detalles + stock + histórico atómicos | VentaRepository.Registrar/Confirmar. |
| Pendiente sin afectar inventario | Registrar(..., false). |
| Categorías por estado; activar productos | GetByEstado/CambiarEstado. |
| Clientes y búsqueda | ClienteRepository.Search busca en Persona. |
| EF delimitado | Productos por categoría y bajo stock; no escribe ventas. |
| LINQ en List<T> y agregados | Ejemplos/ReportesLINQ.cs. Copiar a Negocio. |
| FIFO/LIFO, deshacer acciones, Forms, login, validaciones de negocio, tests | Pendientes en las otras capas. |
| Diagrama ER/arquitectura, manuales y video | Pendientes de entrega general. |

El módulo EF se separa para demostrar una alternativa de consulta expresada con LINQ; las escrituras de ventas permanecen en ADO.NET para controlar explícitamente la transacción indicada por el PDF. Documentación oficial: https://learn.microsoft.com/en-us/ef/core/providers/sql-server/

## 7. Verificación y límites reales

Se revisaron correspondencia de campos, claves foráneas, parámetros de escritura, firmas de interfaces y rutas del flujo transaccional. **No se compiló ni ejecutó contra SQL Server en este entorno**: no están disponibles el SDK dotnet ni el servidor SQL. No se afirma que hayan pasado pruebas unitarias. El archivo VERIFICACION_LOCAL.md trae pruebas de integración concretas para realizar en su PC; son comprobaciones manuales, no sustituyen las pruebas de Negocio del PDF.
