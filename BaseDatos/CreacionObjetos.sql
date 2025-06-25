USE CineAtomDB;
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

    IF (@CantidadActual <= 0)
    BEGIN
	--Esto para que solo puedan eliminar los que no tengan existencias
        RAISERROR ('No se puede eliminar un artículo con cantidad igual a cero.', 16, 1);
        RETURN -1;
    END

    DELETE FROM CineAtom_Articulo WHERE ArticuloId = @ArticuloId;

    IF @@ROWCOUNT = 0 --Osea la cantidad de filas alteradas
    BEGIN
        -- No se elimin ningún registro
        RETURN -2;
    END

    RETURN 0;
END;
GO

