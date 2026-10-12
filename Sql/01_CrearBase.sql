-- Decisión: tienda_db coincide con DatabaseConfig del profesor.
-- Script NO destructivo. Ejecutar UNA vez en una BD nueva desde SSMS (GO es separador de SSMS).
IF DB_ID(N'tienda_db') IS NULL CREATE DATABASE tienda_db;
GO
USE tienda_db;
GO
CREATE TABLE dbo.[Categorias] (
    [CategoriaId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [Nombre] NVARCHAR(150) NOT NULL DEFAULT N'',
    [Descripcion] NVARCHAR(500) NOT NULL DEFAULT N'',
    [FechaCreacion] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [Activo] BIT NOT NULL DEFAULT 1,
    CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0)
);
GO
CREATE TABLE dbo.[Productos] (
    [ProductoId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [Nombre] NVARCHAR(150) NOT NULL DEFAULT N'',
    [Descripcion] NVARCHAR(500) NOT NULL DEFAULT N'',
    [PrecioCompra] DECIMAL(18,2) NOT NULL,
    [PrecioVenta] DECIMAL(18,2) NOT NULL,
    [StockActual] INT NOT NULL,
    [StockMinimo] INT NOT NULL,
    [CategoriaId] INT NOT NULL REFERENCES dbo.[Categorias]([CategoriaId]),
    [FechaRegistro] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [Activo] BIT NOT NULL DEFAULT 1,
    CHECK (PrecioCompra > 0),
    CHECK (PrecioVenta > 0),
    CHECK (StockActual >= 0),
    CHECK (StockMinimo >= 0)
);
GO
CREATE TABLE dbo.[Persona] (
    [PersonaId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [NombreCompleto] NVARCHAR(200) NOT NULL DEFAULT N'',
    [Identificacion] NVARCHAR(50) NOT NULL DEFAULT N'',
    [Email] NVARCHAR(200) NOT NULL DEFAULT N'',
    [Telefono] NVARCHAR(30) NOT NULL DEFAULT N'',
    [Direccion] NVARCHAR(500) NOT NULL DEFAULT N'',
    [FechaRegistro] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [Activo] BIT NOT NULL DEFAULT 1,
    UNIQUE (Identificacion)
);
GO
CREATE TABLE dbo.[Cliente] (
    [ClienteId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [PersonaId] INT NOT NULL REFERENCES dbo.[Persona]([PersonaId]),
    [LimiteCredito] DECIMAL(18,2) NOT NULL,
    [FechaAfiliacion] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [Activo] BIT NOT NULL DEFAULT 1,
    CHECK (LimiteCredito >= 0),
    UNIQUE (PersonaId)
);
GO
CREATE TABLE dbo.[Empleado] (
    [EmpleadoId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [PersonaId] INT NOT NULL REFERENCES dbo.[Persona]([PersonaId]),
    [Puesto] NVARCHAR(100) NOT NULL DEFAULT N'',
    [Salario] DECIMAL(18,2) NOT NULL,
    [FechaIngreso] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [Activo] BIT NOT NULL DEFAULT 1,
    CHECK (Salario >= 0),
    UNIQUE (PersonaId)
);
GO
CREATE TABLE dbo.[Roles] (
    [RolId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [NombreRol] NVARCHAR(50) NOT NULL DEFAULT N'',
    [Descripcion] NVARCHAR(500) NOT NULL DEFAULT N'',
    [Activo] BIT NOT NULL DEFAULT 1,
    UNIQUE (NombreRol)
);
GO
CREATE TABLE dbo.[Usuarios] (
    [UsuarioId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [EmpleadoId] INT NOT NULL REFERENCES dbo.[Empleado]([EmpleadoId]),
    [NombreUsuario] NVARCHAR(100) NOT NULL DEFAULT N'',
    [ContrasenaHash] NVARCHAR(500) NOT NULL DEFAULT N'',
    [RolId] INT NOT NULL REFERENCES dbo.[Roles]([RolId]),
    [Activo] BIT NOT NULL DEFAULT 1,
    UNIQUE (NombreUsuario)
);
GO
CREATE TABLE dbo.[Venta] (
    [VentaId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [ClienteId] INT NOT NULL REFERENCES dbo.[Cliente]([ClienteId]),
    [EmpleadoId] INT NOT NULL REFERENCES dbo.[Empleado]([EmpleadoId]),
    [FechaVenta] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [SubTotal] DECIMAL(18,2) NOT NULL,
    [Impuesto] DECIMAL(18,2) NOT NULL,
    [Descuento] DECIMAL(18,2) NOT NULL,
    [Total] DECIMAL(18,2) NOT NULL,
    [Estado] NVARCHAR(20) NOT NULL DEFAULT N'Pendiente',
    [Observaciones] NVARCHAR(500) NOT NULL DEFAULT N'',
    CHECK (Estado IN (N'Pendiente',N'Procesada',N'Anulada')),
    CHECK (SubTotal >= 0),
    CHECK (Impuesto >= 0),
    CHECK (Descuento >= 0 AND Descuento <= SubTotal),
    CHECK (Total = SubTotal + Impuesto - Descuento)
);
GO
CREATE TABLE dbo.[DetalleVenta] (
    [DetalleVentaId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [VentaId] INT NOT NULL REFERENCES dbo.[Venta]([VentaId]),
    [ProductoId] INT NOT NULL REFERENCES dbo.[Productos]([ProductoId]),
    [Cantidad] INT NOT NULL,
    [PrecioUnitario] DECIMAL(18,2) NOT NULL,
    [Descuento] DECIMAL(18,2) NOT NULL,
    [SubTotal] DECIMAL(18,2) NOT NULL,
    CHECK (Cantidad > 0),
    CHECK (PrecioUnitario > 0),
    CHECK (Descuento >= 0 AND Descuento <= SubTotal),
    CHECK (SubTotal = Cantidad * PrecioUnitario)
);
GO
CREATE TABLE dbo.[HistoricoMovimientosInventario] (
    [MovimientoId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [ProductoId] INT NOT NULL REFERENCES dbo.[Productos]([ProductoId]),
    [EmpleadoId] INT NOT NULL REFERENCES dbo.[Empleado]([EmpleadoId]),
    [FechaMovimiento] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [TipoMovimiento] NVARCHAR(20) NOT NULL DEFAULT N'',
    [Cantidad] INT NOT NULL,
    [StockAnterior] INT NOT NULL,
    [StockNuevo] INT NOT NULL,
    [Observacion] NVARCHAR(500) NOT NULL DEFAULT N'',
    CHECK (TipoMovimiento IN (N'Entrada',N'Salida',N'Ajuste',N'Devolución',N'Venta')),
    CHECK (Cantidad > 0),
    CHECK (StockAnterior >= 0),
    CHECK (StockNuevo >= 0)
);
GO
CREATE INDEX IX_Productos_CategoriaId ON dbo.Productos(CategoriaId);
CREATE INDEX IX_DetalleVenta_VentaId ON dbo.DetalleVenta(VentaId);
CREATE INDEX IX_Movimientos_ProductoId ON dbo.HistoricoMovimientosInventario(ProductoId);
GO
-- Datos iniciales ficticios, elegidos para probar; no vienen en el PDF.
INSERT dbo.Roles(NombreRol,Descripcion) VALUES
(N'Administrador',N'Administración'),(N'Vendedor',N'Ventas'),(N'Bodeguero',N'Inventario'),(N'Consulta',N'Reportes');
INSERT dbo.Categorias(Nombre,Descripcion) VALUES (N'General',N'Categoría de prueba');
INSERT dbo.Productos(Nombre,Descripcion,PrecioCompra,PrecioVenta,StockActual,StockMinimo,CategoriaId)
VALUES(N'Producto de prueba',N'Dato ficticio',1000,1500,20,5,1);
INSERT dbo.Persona(NombreCompleto,Identificacion,Email,Telefono,Direccion) VALUES
(N'Cliente de prueba',N'DEMO-C1',N'cliente@example.com',N'00000000',N'Dato ficticio'),
(N'Empleado de prueba',N'DEMO-E1',N'empleado@example.com',N'00000000',N'Dato ficticio');
INSERT dbo.Cliente(PersonaId,LimiteCredito) VALUES(1,0);
INSERT dbo.Empleado(PersonaId,Puesto,Salario) VALUES(2,N'Vendedor',0);
-- No se inventa una contraseña/hash para login. Crearlo en la capa de negocio.
GO
