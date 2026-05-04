using EquipmentAPI.Models;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{

//}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


app.MapPost("/equipments", async (CreateEquipmentDto dto) =>
{
using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

using var command = new SqlCommand(
    @"INSERT INTO Equipments (Name, Category, Status, Location)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Category, @Status, @Location)", connection);

command.Parameters.AddWithValue("@Name", dto.Name);
command.Parameters.AddWithValue("@Category", dto.Category);
command.Parameters.AddWithValue("@Status", dto.Status);
command.Parameters.AddWithValue("@Location", dto.Location);

var newId = (int)(await command.ExecuteScalarAsync())!;

    return Results.Created($"/equipments/{newId}", new EquipmentResponseDto
    {
        Id = newId,
        Name = dto.Name,
        Category = dto.Category,
        Status = dto.Status,
        Location = dto.Location
    });
}).WithName("CreateEquipment").WithOpenApi();


app.MapGet("/equipments", async () => 
{
    var equipment = new List <EquipmentResponseDto> ();

    using var connection = new SqlConnection (connectionString);
    await connection.OpenAsync();

    using var command = new SqlCommand("SELECT Id, Name, Category, Status, Location FROM Equipments", connection);
    using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        equipment.Add(new EquipmentResponseDto
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Category = reader.GetString(reader.GetOrdinal("Category")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            Location = reader.GetString(reader.GetOrdinal("Location"))
        });
    }
    return Results.Ok(equipment);

}).WithName("GetEquipments").WithOpenApi();

app.MapGet("/equipments/{id:int:min(1)}", async (int id) => 
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    using var command = new SqlCommand(
        "SELECT Id, Name, Category, Status, Location FROM Equipments WHERE Id = @Id", connection);

    command.Parameters.AddWithValue("@Id", id);

    using var reader = await command.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
        return Results.NotFound();

    return Results.Ok(new EquipmentResponseDto
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        Category = reader.GetString(reader.GetOrdinal("Category")),
        Status = reader.GetString(reader.GetOrdinal("Status")),
        Location = reader.GetString(reader.GetOrdinal("Location")),
    });
}).WithName("GetEquipmentById").WithOpenApi();


app.Run();


