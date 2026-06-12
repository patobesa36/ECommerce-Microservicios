using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Data;
using Users.API.Models;

namespace Users.API.Data;

public class UserRepository(IConfiguration configuration) : IUserRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db";

    private SqliteConnection CreateConnection() => new(_connectionString);

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<User>(
            """
            SELECT
                id                AS Id,
                nombre            AS Nombre,
                apellido          AS Apellido,
                email             AS Email,
                password_hash     AS PasswordHash,
                fecha_registro    AS FechaRegistro,
                activo            AS Activo,
                intentos_fallidos AS IntentosFallidos
            FROM users
            WHERE lower(email) = lower(@Email)
            LIMIT 1;
            """,
            new { Email = email });
    }

    public async Task<User> CreateAsync(User user)
    {
        using var connection = CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO users (
                id,
                nombre,
                apellido,
                email,
                password_hash,
                fecha_registro,
                activo,
                intentos_fallidos
            )
            VALUES (
                @Id,
                @Nombre,
                @Apellido,
                @Email,
                @PasswordHash,
                @FechaRegistro,
                @Activo,
                @IntentosFallidos
            );
            """,
            new
            {
                Id = user.Id.ToString(),
                user.Nombre,
                user.Apellido,
                user.Email,
                user.PasswordHash,
                FechaRegistro = user.FechaRegistro.ToString("O"),
                Activo = user.Activo ? 1 : 0,
                user.IntentosFallidos
            });

        return user;
    }

    public async Task UpdateLoginStateAsync(Guid id, bool activo, int intentosFallidos)
    {
        using var connection = CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE users
            SET
                activo = @Activo,
                intentos_fallidos = @IntentosFallidos
            WHERE id = @Id;
            """,
            new
            {
                Id = id.ToString(),
                Activo = activo ? 1 : 0,
                IntentosFallidos = intentosFallidos
            });
    }
}