using EquipmentAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var equipmentList = new List<Equipment>();

app.MapPost("/equipments", (CreateEquipmentDto dto) => 
{
    var equipment = new Equipment(dto.Name, dto.Category, dto.Status, dto.Location);

    equipmentList.Add(equipment);

    return Results.Created($"/equipments/{equipment.Id}", new EquipmentResponseDto
    {
        Id = equipment.Id,
        Name = equipment.Name,
        Status = equipment.Status,
        Location = equipment.Location,
    });
})
   .WithName("CreateEquipment")
   .WithOpenApi();


app.MapGet("/equipments", () => 
{
    var result = equipmentList.Select(eq => new EquipmentResponseDto
    {
        Id = eq.Id,
        Name = eq.Name,
        Status = eq.Status,
        Location = eq.Location,
    });
    return Results.Ok(result);
})
   .WithName("GetEquipments")
   .WithOpenApi();

app.MapGet("/equipments/{id:int:min(1)}", (int id) => 
{
    var equipment = equipmentList.FirstOrDefault(e => e.Id == id);
    if (equipment == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(new EquipmentResponseDto
    {
        Id = equipment.Id,
        Name = equipment.Name,
        Status = equipment.Status,
        Location = equipment.Location,
    });
})
   .WithName("GetEquipmentById")
   .WithOpenApi();


app.Run();


