USE [DatQbox]
GO

-- Función para obtener el total de usuarios
IF OBJECT_ID('GetTotalUsuarios', 'FN') IS NOT NULL
    DROP FUNCTION GetTotalUsuarios;
GO

CREATE FUNCTION GetTotalUsuarios()
RETURNS INT
AS
BEGIN
    DECLARE @Total INT;
    SELECT @Total = COUNT(*) FROM Usuarios;
    RETURN @Total;
END
GO
