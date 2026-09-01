using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace SQLSomee_1.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DatosController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public DatosController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("tabla/{tabla}")]
    public async Task<IActionResult> GetTabla(string tabla)
    {
        var connectionString = _configuration.GetConnectionString("SomeeConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return BadRequest("No existe la cadena de conexión 'SomeeConnection'.");
        }

        if (string.IsNullOrWhiteSpace(tabla))
        {
            return BadRequest("Debes indicar el nombre de la tabla.");
        }

        var query = $"SELECT * FROM [{tabla}]";

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var columns = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                columns.Add(reader.GetName(i));
            }

            var rows = new List<object>();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                foreach (var column in columns)
                {
                    row[column] = reader[column] is DBNull ? null : reader[column];
                }
                rows.Add(row);
            }

            return Ok(new
            {
                tabla,
                totalRegistros = rows.Count,
                datos = rows
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "Error al consultar la base de datos.",
                detalle = ex.Message,
                tabla
            });
        }
    }
}
