USE CineAtomDB;
GO


-- Insertamos 5 categorias distintas
INSERT INTO CineAtom_Categoria (Nombre) VALUES
('Snacks'),
('Combos'),
('Bebidas'),
('Promocional'),
('Dulcer�a');
GO

-- Insertamos 30 articulos repartidos entre las 5 categor�as (usando CategoriaId del 1 al 5)
INSERT INTO CineAtom_Articulo (Nombre, Descripcion, Cantidad, Precio, CategoriaId) VALUES
('Palomitas cl�sicas', 'Palomitas de ma�z con mantequilla', 50, 2.50, 1),
('Hot dog simple', 'Pan con salchicha y mostaza', 30, 3.00, 1),
('Combo especial', 'Palomitas grandes + bebida', 20, 5.50, 2),
('Refresco cola', 'Bebida gaseosa de 500ml', 100, 1.80, 3),
('Edici�n limitada Spider-Man', 'Palomitas saborizadas edici�n limitada', 10, 3.50, 4),

('Nachos con queso', 'Nachos con salsa de queso', 0, 3.20, 1),
('Combo familiar', '2 combos + 2 refrescos', 15, 12.00, 2),
('Agua mineral', 'Botella de agua de 500ml', 80, 1.00, 3),
('Galletas dulces', 'Paquete de galletas variadas', 25, 2.00, 5),
('Combo 3D', 'Combo para pel�cula 3D', 12, 6.50, 2),

('Palomitas caramelo', 'Palomitas con sabor a caramelo', 45, 2.75, 1),
('Refresco lim�n', 'Bebida gaseosa sabor lim�n', 70, 1.85, 3),
('Chocolates surtidos', 'Caja de chocolates variados', 30, 4.00, 5),
('Combo parejas', '2 combos + 1 bebida grande', 10, 11.00, 2),
('Palomitas queso', 'Palomitas sabor queso cheddar', 35, 3.00, 1),

('Bebida energ�tica', 'Lata de 250ml', 60, 2.50, 3),
('Dulces mixtos', 'Bolsa de dulces surtidos', 50, 3.50, 5),
('Combo ni�o', 'Combo peque�o para ni�os', 18, 4.00, 2),
('Refresco naranja', 'Bebida gaseosa sabor naranja', 55, 1.90, 3),
('Palomitas picantes', 'Palomitas con especias picantes', 40, 3.20, 1),

('Hot dog deluxe', 'Pan con salchicha, queso y ketchup', 25, 3.75, 1),
('Combo premium', 'Palomitas grandes + bebida + dulce', 8, 7.50, 2),
('Chicles', 'Paquete de chicles menta', 45, 1.25, 5),
('Refresco diet�tico', 'Bebida gaseosa sin az�car', 50, 1.95, 3),
('Palomitas mantequilla extra', 'Palomitas con m�s mantequilla', 38, 3.10, 1),

('Dulces de fruta', 'Caja de dulces naturales', 30, 3.80, 5),
('Combo grupo', '4 combos + 4 bebidas', 6, 22.00, 2),
('Refresco cola light', 'Bebida cola sin az�car', 60, 1.85, 3),
('Palomitas naturales', 'Palomitas sin saborizantes', 50, 2.20, 1),
('Dulces �cidos', 'Bolsa de dulces �cidos variados', 35, 3.00, 5);
GO
