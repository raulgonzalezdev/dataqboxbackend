USE [DatQbox]
GO

-- Procedimiento para obtener todos los usuarios
IF OBJECT_ID('GetAllUsuarios', 'P') IS NOT NULL
    DROP PROCEDURE GetAllUsuarios;
GO

CREATE PROCEDURE GetAllUsuarios
AS
BEGIN
    SELECT * FROM Usuarios;
END
GO

-- Procedimiento para obtener un usuario por ID
IF OBJECT_ID('GetUsuarioById', 'P') IS NOT NULL
    DROP PROCEDURE GetUsuarioById;
GO

CREATE PROCEDURE GetUsuarioById
    @Cod_Usuario NVARCHAR(10)
AS
BEGIN
    SELECT * FROM Usuarios WHERE Cod_Usuario = @Cod_Usuario;
END
GO

-- Procedimiento para agregar un nuevo usuario
IF OBJECT_ID('AddUsuario', 'P') IS NOT NULL
    DROP PROCEDURE AddUsuario;
GO

CREATE PROCEDURE AddUsuario
    @Cod_Usuario NVARCHAR(10),
    @Password NVARCHAR(10),
    @Nombre NVARCHAR(50),
    @Tipo NVARCHAR(10),
    @Updates BIT,
    @Addnews BIT,
    @Deletes BIT,
    @Creador BIT,
    @Cambiar BIT,
    @PrecioMinimo BIT,
    @Credito BIT
AS
BEGIN
    INSERT INTO Usuarios (Cod_Usuario, Password, Nombre, Tipo, Updates, Addnews, Deletes, Creador, Cambiar, PrecioMinimo, Credito)
    VALUES (@Cod_Usuario, @Password, @Nombre, @Tipo, @Updates, @Addnews, @Deletes, @Creador, @Cambiar, @PrecioMinimo, @Credito);
END
GO

-- Procedimiento para actualizar un usuario
IF OBJECT_ID('UpdateUsuario', 'P') IS NOT NULL
    DROP PROCEDURE UpdateUsuario;
GO

CREATE PROCEDURE UpdateUsuario
    @Cod_Usuario NVARCHAR(10),
    @Password NVARCHAR(10),
    @Nombre NVARCHAR(50),
    @Tipo NVARCHAR(10),
    @Updates BIT,
    @Addnews BIT,
    @Deletes BIT,
    @Creador BIT,
    @Cambiar BIT,
    @PrecioMinimo BIT,
    @Credito BIT
AS
BEGIN
    UPDATE Usuarios
    SET Password = @Password,
        Nombre = @Nombre,
        Tipo = @Tipo,
        Updates = @Updates,
        Addnews = @Addnews,
        Deletes = @Deletes,
        Creador = @Creador,
        Cambiar = @Cambiar,
        PrecioMinimo = @PrecioMinimo,
        Credito = @Credito
    WHERE Cod_Usuario = @Cod_Usuario;
END
GO

-- Procedimiento para eliminar un usuario
IF OBJECT_ID('DeleteUsuario', 'P') IS NOT NULL
    DROP PROCEDURE DeleteUsuario;
GO

CREATE PROCEDURE DeleteUsuario
    @Cod_Usuario NVARCHAR(10)
AS
BEGIN
    DELETE FROM Usuarios WHERE Cod_Usuario = @Cod_Usuario;
END
GO
