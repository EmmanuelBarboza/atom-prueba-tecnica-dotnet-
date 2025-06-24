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

CREATE OR ALTER PROCEDURE usp_CineAtom_Articulo_Update
    @ArticuloId INT,
    @Nombre NVARCHAR(100),
    @Descripcion NVARCHAR(255),
    @Cantidad INT,
    @Precio DECIMAL(10,2),
    @CategoriaId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM CineAtom_Articulo WHERE ArticuloId = @ArticuloId)
    BEGIN
        RETURN -2; -- Esta validacion es por si no existe el producto entonces que devuelva -2
    END

    UPDATE CineAtom_Articulo
    SET
        Nombre = @Nombre,
        Descripcion = @Descripcion,
        Cantidad = @Cantidad,
        Precio = @Precio,
        CategoriaId = @CategoriaId
    WHERE ArticuloId = @ArticuloId;

    RETURN 0; -- Quiere decir que lo actualizo correctamente
END;
GO




-- Procedimiento para eliminar artículo
CREATE OR ALTER PROCEDURE usp_CineAtom_Articulo_Delete
    @ArticuloId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CantidadActual INT;

    SELECT @CantidadActual = Cantidad FROM CineAtom_Articulo WHERE ArticuloId = @ArticuloId;

    IF (@CantidadActual IS NULL)
    BEGIN
        -- Esto es si no existe el articulo
        RETURN -2;
    END

    IF (@CantidadActual > 0)
    BEGIN
	--Esto para que solo puedan eliminar los que no tengan existencias
        RAISERROR ('No se puede eliminar un artículo con cantidad mayor a cero.', 16, 1);
        RETURN -1;
    END

    DELETE FROM CineAtom_Articulo WHERE ArticuloId = @ArticuloId;

    IF @@ROWCOUNT = 0
    BEGIN
        -- No se eliminó ningún registro
        RETURN -2;
    END

    RETURN 0;
END;
GO

