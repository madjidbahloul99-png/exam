namespace exam.Models;

public sealed class TBaseModel
{
    public string? Id { get; set; }

    public string FactoryCode { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string? ModelDesc { get; set; }

    public string? Color { get; set; }

    public int? ImeiCount { get; set; }

    public int? BoxNumber { get; set; }

    public string? Note { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? CreateUser { get; set; }

    public DateTime? ModifyDate { get; set; }

    public string? ModifyUser { get; set; }
}
