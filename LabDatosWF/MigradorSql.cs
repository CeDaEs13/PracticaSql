using Microsoft.Data.SqlClient;

namespace LabDatosWF;

public class MigradorSql
{
    // Cambia la IP por la del servidor real (usa ipconfig en la PC servidor)
    private const string ConnectionString =
        "Server=localhost,1433;" +
        "Database=LabDatos;"                      +
        "User Id=sa;"                             +
        "Password=1234;"                          +
        "TrustServerCertificate=True;";

    public async Task<(bool ok, string mensaje)> VerificarConexion()
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();
            return (true, $"Conectado a: {conn.DataSource} | DB: {conn.Database}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(int insertados, int omitidos, List<string> log)> MigrarDesdeArchivo(string archivoPath)
    {
        var log = new List<string>();
        int insertados = 0, omitidos = 0;

        if (!File.Exists(archivoPath))
        {
            log.Add("ERROR: No se encontró el archivo .dat");
            return (0, 0, log);
        }

        long totalBytes = new FileInfo(archivoPath).Length;
        int totalReg = (int)(totalBytes / Ciudadano.Size);
        var gestor = new GestorArchivos();

        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        log.Add($"Conexión establecida. Migrando {totalReg} registro(s)...\n");

        const string query = "INSERT INTO Ciudadanos (Id, Nombre, Edad) VALUES (@Id, @Nombre, @Edad)";

        for (int i = 0; i < totalReg; i++)
        {
            var c = gestor.LeerCiudadano(i);
            if (c is null) continue;
            try
            {
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id",     c.Value.Id);
                cmd.Parameters.AddWithValue("@Nombre", c.Value.Nombre);
                cmd.Parameters.AddWithValue("@Edad",   c.Value.Edad);
                await cmd.ExecuteNonQueryAsync();
                log.Add($"[OK]      Id:{c.Value.Id} | {c.Value.Nombre} | {c.Value.Edad} años");
                insertados++;
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                log.Add($"[OMITIDO] Id:{c.Value.Id} ya existe en la BD.");
                omitidos++;
            }
        }

        log.Add($"\nFinalizado: {insertados} insertado(s), {omitidos} omitido(s).");
        return (insertados, omitidos, log);
    }

    public async Task<List<Ciudadano>> ConsultarTodos()
    {
        var lista = new List<Ciudadano>();
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        using var cmd = new SqlCommand("SELECT Id, Nombre, Edad FROM Ciudadanos ORDER BY Id", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            lista.Add(new Ciudadano(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2)));
        return lista;
    }
}
