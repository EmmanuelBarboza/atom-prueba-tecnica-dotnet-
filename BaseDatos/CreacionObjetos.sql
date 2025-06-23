-- Script para crear la tabla CineAtom_Articulo y procedimientos almacenados

USE CineAtomDB;
GO

CREATE TABLE CineAtom_Articulo (
    ArticuloId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(255) NULL,
    Cantidad INT NOT NULL DEFAULT 0,
    Precio DECIMAL(10,2) NOT NULL,
    Categoria NVARCHAR(50) NULL
);
GO

-- Procedimiento almacenado para actualizar un artículo
CREATE PROCEDURE usp_CineAtom_Articulo_Update
    @ArticuloId INT,
    @Nombre NVARCHAR(100),
    @Descripcion NVARCHAR(255),
    @Cantidad INT,
    @Precio DECIMAL(10,2),
    @Categoria NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE CineAtom_Articulo
    SET Nombre = @Nombre,
        Descripcion = @Descripcion,
        Cantidad = @Cantidad,
        Precio = @Precio,
        Categoria = @Categoria
    WHERE ArticuloId = @ArticuloId;
END;
GO

-- Procedimiento almacenado para eliminar un artículo
CREATE PROCEDURE usp_CineAtom_Articulo_Delete
    @ArticuloId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validar que la cantidad sea mayor a 0 para permitir eliminar
    DECLARE @CantidadActual INT;
    SELECT @CantidadActual = Cantidad FROM CineAtom_Articulo WHERE ArticuloId = @ArticuloId;

    IF (@CantidadActual > 0)
    BEGIN
        -- No se permite eliminar si cantidad > 0 (puedes cambiar según regla de negocio)
        RAISERROR ('No se puede eliminar un artículo con cantidad mayor a cero.', 16, 1);
        RETURN;
    END

    DELETE FROM CineAtom_Articulo WHERE ArticuloId = @ArticuloId;
END;
GO
