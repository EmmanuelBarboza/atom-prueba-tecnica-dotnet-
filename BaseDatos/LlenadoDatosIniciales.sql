-- Script para insertar datos iniciales en CineAtom_Articulo

USE CineAtomDB;
GO

INSERT INTO CineAtom_Articulo (Nombre, Descripcion, Cantidad, Precio, Categoria) VALUES
('Palomitas cl�sicas', 'Palomitas de ma�z con mantequilla', 50, 2.50, 'Snacks'),
('Hot dog simple', 'Pan con salchicha y mostaza', 30, 3.00, 'Snacks'),
('Combo especial', 'Palomitas grandes + bebida', 20, 5.50, 'Combos'),
('Refresco cola', 'Bebida gaseosa de 500ml', 100, 1.80, 'Bebidas'),
('Edici�n limitada Spider-Man', 'Palomitas saborizadas edici�n limitada', 10, 3.50, 'Promocional');
GO
