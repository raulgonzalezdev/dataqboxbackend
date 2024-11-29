USE [DatQbox]
GO

-- Vista para obtener información de usuarios
IF OBJECT_ID('vw_Usuarios', 'V') IS NOT NULL
    DROP VIEW vw_Usuarios;
GO

CREATE VIEW vw_Usuarios AS
SELECT Cod_Usuario, Nombre, Tipo, Credito
FROM Usuarios;
GO
