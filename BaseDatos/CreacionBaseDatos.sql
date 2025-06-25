-- Script para crear la base de datos

CREATE DATABASE CineAtomDB;
GO

USE CineAtomDB;
GO
-- Tabla de categorías para distintos tipos de productos
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
    FOREIGN KEY (CategoriaId) REFERENCES CineAtom_Categoria(CategoriaId) --Aca hacemos una llave foranea 
);
GO