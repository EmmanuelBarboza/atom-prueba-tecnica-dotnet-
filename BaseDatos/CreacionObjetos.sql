USE CineAtomDB;
GO

-- Tabla de categorías
CREATE TABLE CineAtom_Categoria (
    CategoriaId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL
);
GO

-- Tabla de artículos
CREATE TABLE CineAtom_Articulo (
    ArticuloId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(255) NULL,
    Cantidad INT NOT NULL DEFAULT 0,
    Precio DECIMAL(10,2) NOT NULL,
    CategoriaId INT NOT NULL,
    FOREIGN KEY (CategoriaId) REFERENCES CineAtom_Categoria(CategoriaId)
);
GO

-- Procedimiento para actualizar artículo
CREATE PROCEDURE usp_CineAtom_Articulo_Update
    @ArticuloId INT,
    @Nombre NVARCHAR(100),
    @Descripcion NVARCHAR(255),
    @Cantidad INT,
    @Precio DECIMAL(10,2),
    @CategoriaId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE CineAtom_Articulo
    SET Nombre = @Nombre,
        Descripcion = @Descripcion,
        Cantidad = @Cantidad,
        Precio = @Precio,
        CategoriaId = @CategoriaId
    WHERE ArticuloId = @ArticuloId;
END;
GO

-- Procedimiento para eliminar artículo
CREATE PROCEDURE usp_CineAtom_Articulo_Delete
    @ArticuloId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CantidadActual INT;
    SELECT @CantidadActual = Cantidad FROM CineAtom_Articulo WHERE ArticuloId = @ArticuloId;

    IF (@CantidadActual > 0)
    BEGIN
        RAISERROR ('No se puede eliminar un artículo con cantidad mayor a cero.', 16, 1);
        RETURN;
    END

    DELETE FROM CineAtom_Articulo WHERE ArticuloId = @ArticuloId;
END;
GO
