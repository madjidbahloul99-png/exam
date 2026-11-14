using Microsoft.Data.SqlClient;
using System.Text.Json;
using exam.Models;

namespace exam.Data;

public sealed class TBaseModelRepository : IDisposable
{
    private readonly string _connectionString;
    private readonly string _auditUser;

    public TBaseModelRepository(string connectionString, string auditUser = "exam_user")
    {
        _connectionString = connectionString;
        _auditUser = auditUser;
    }

    public List<TBaseModel> Get(string factoryCode, string model)
    {
        factoryCode = string.IsNullOrWhiteSpace(factoryCode) ? string.Empty : factoryCode.Trim();
        model = string.IsNullOrWhiteSpace(model) ? string.Empty : model.Trim();

        const string sql = @"
            SELECT id, factoryCode, model, modelDesc, color, imeiCount, boxNumber, note,
                   createDate, createUser, modifyDate, modifyUser
            FROM dbo.t_base_model
            WHERE (@factoryCode = N'' OR factoryCode = @factoryCode)
              AND (@model = N'' OR model LIKE N'%' + @model + N'%')
            ORDER BY factoryCode, model";

        var list = new List<TBaseModel>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@factoryCode", factoryCode);
        command.Parameters.AddWithValue("@model", model);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReader(reader));
        }
        return list;
    }

    public TBaseModel GetByKey(string factoryCode, string model)
    {
        const string sql = @"
            SELECT id, factoryCode, model, modelDesc, color, imeiCount, boxNumber, note,
                   createDate, createUser, modifyDate, modifyUser
            FROM dbo.t_base_model
            WHERE factoryCode = @factoryCode AND model = @model";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@factoryCode", factoryCode);
        command.Parameters.AddWithValue("@model", model);

        connection.Open();
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapReader(reader);
        }
        throw new Exception("The record was not found. It may have been deleted by another user.");
    }

    public List<string> GetDistinctColors()
    {
        const string sql = "SELECT DISTINCT color FROM dbo.t_base_model WHERE color IS NOT NULL AND color <> N'' ORDER BY color";

        var colors = new List<string>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            colors.Add(reader.GetString(0));
        }
        return colors;
    }

    public void Insert(TBaseModel model)
    {
        Validate(model, isEdit: false);
        ApplyAddDefaults(model);

        const string sql = @"
            INSERT INTO dbo.t_base_model (factoryCode, model, modelDesc, color, imeiCount, boxNumber, note, createDate, createUser)
            OUTPUT INSERTED.id
            VALUES (@factoryCode, @model, @modelDesc, @color, @imeiCount, @boxNumber, @note, @createDate, @createUser)";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            AddCommonParameters(command, model);
            command.Parameters.AddWithValue("@createDate", DateTime.Now);
            command.Parameters.AddWithValue("@createUser", _auditUser);

            connection.Open();
            var id = command.ExecuteScalar();
            model.Id = id?.ToString();
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            throw new DuplicateModelException("This model already exists for this factory.");
        }
        catch (SqlException ex)
        {
            throw new Exception($"Database operation failed ({ex.Number}): {ex.Message}");
        }
    }

    public void Update(TBaseModel model)
    {
        Validate(model, isEdit: true);

        const string sql = @"
            UPDATE dbo.t_base_model
            SET modelDesc = @modelDesc,
                color = @color,
                imeiCount = @imeiCount,
                boxNumber = @boxNumber,
                note = @note,
                modifyDate = @modifyDate,
                modifyUser = @modifyUser
            WHERE factoryCode = @factoryCode AND model = @model";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            AddCommonParameters(command, model);
            command.Parameters.AddWithValue("@modifyDate", DateTime.Now);
            command.Parameters.AddWithValue("@modifyUser", _auditUser);

            connection.Open();
            var affected = command.ExecuteNonQuery();
            if (affected == 0)
            {
                throw new Exception("The record was not found. It may have been deleted by another user.");
            }
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            throw new DuplicateModelException("This model already exists for this factory.");
        }
        catch (SqlException ex)
        {
            throw new Exception($"Database operation failed ({ex.Number}): {ex.Message}");
        }
    }

    public void Delete(string factoryCode, string model)
    {
        const string sql = "DELETE FROM dbo.t_base_model WHERE factoryCode = @factoryCode AND model = @model";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@factoryCode", factoryCode);
            command.Parameters.AddWithValue("@model", model);

            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw new Exception($"Database operation failed ({ex.Number}): {ex.Message}");
        }
    }

    public void Dispose()
    {
    }

    private static void AddCommonParameters(SqlCommand command, TBaseModel model)
    {
        command.Parameters.AddWithValue("@factoryCode", model.FactoryCode);
        command.Parameters.AddWithValue("@model", model.Model);
        command.Parameters.AddWithValue("@modelDesc", (object?)model.ModelDesc ?? DBNull.Value);
        command.Parameters.AddWithValue("@color", (object?)model.Color ?? DBNull.Value);
        command.Parameters.AddWithValue("@imeiCount", (object?)model.ImeiCount ?? DBNull.Value);
        command.Parameters.AddWithValue("@boxNumber", (object?)model.BoxNumber ?? DBNull.Value);
        command.Parameters.AddWithValue("@note", (object?)model.Note ?? DBNull.Value);
    }

    private static TBaseModel MapReader(SqlDataReader reader)
    {
        return new TBaseModel
        {
            Id = reader["id"] is DBNull ? null : reader.GetString(reader.GetOrdinal("id")),
            FactoryCode = reader.GetString(reader.GetOrdinal("factoryCode")),
            Model = reader.GetString(reader.GetOrdinal("model")),
            ModelDesc = reader["modelDesc"] is DBNull ? null : reader.GetString(reader.GetOrdinal("modelDesc")),
            Color = reader["color"] is DBNull ? null : reader.GetString(reader.GetOrdinal("color")),
            ImeiCount = reader["imeiCount"] is DBNull ? null : reader.GetInt32(reader.GetOrdinal("imeiCount")),
            BoxNumber = reader["boxNumber"] is DBNull ? null : reader.GetInt32(reader.GetOrdinal("boxNumber")),
            Note = reader["note"] is DBNull ? null : reader.GetString(reader.GetOrdinal("note")),
            CreateDate = reader["createDate"] is DBNull ? null : reader.GetDateTime(reader.GetOrdinal("createDate")),
            CreateUser = reader["createUser"] is DBNull ? null : reader.GetString(reader.GetOrdinal("createUser")),
            ModifyDate = reader["modifyDate"] is DBNull ? null : reader.GetDateTime(reader.GetOrdinal("modifyDate")),
            ModifyUser = reader["modifyUser"] is DBNull ? null : reader.GetString(reader.GetOrdinal("modifyUser")),
        };
    }

    private static void Validate(TBaseModel model, bool isEdit)
    {
        model.FactoryCode = model.FactoryCode?.Trim() ?? string.Empty;
        model.Model = model.Model?.Trim() ?? string.Empty;
        model.ModelDesc = string.IsNullOrWhiteSpace(model.ModelDesc) ? null : model.ModelDesc.Trim();
        model.Color = string.IsNullOrWhiteSpace(model.Color) ? null : model.Color.Trim();
        model.Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim();

        if (string.IsNullOrEmpty(model.FactoryCode))
            throw new ModelValidationException("Factory code is required.");
        if (model.FactoryCode.Length > 200)
            throw new ModelValidationException("Factory code must not exceed 200 characters.");

        if (string.IsNullOrEmpty(model.Model))
            throw new ModelValidationException("Model is required.");
        if (model.Model.Length > 200)
            throw new ModelValidationException("Model must not exceed 200 characters.");

        if (model.ModelDesc is not null && model.ModelDesc.Length > 200)
            throw new ModelValidationException("Model description must not exceed 200 characters.");
        if (model.Color is not null && model.Color.Length > 200)
            throw new ModelValidationException("Color must not exceed 200 characters.");
        if (model.Note is not null && model.Note.Length > 200)
            throw new ModelValidationException("Notes must not exceed 200 characters.");

        if (model.ImeiCount.HasValue && model.ImeiCount.Value <= 0)
            throw new ModelValidationException("IMEI count must be a positive integer.");

        if (model.BoxNumber.HasValue && model.BoxNumber.Value <= 0)
            throw new ModelValidationException("Units per box must be a positive integer.");
    }

    private static void ApplyAddDefaults(TBaseModel model)
    {
        if (!model.ImeiCount.HasValue)
            model.ImeiCount = 2;
        if (!model.BoxNumber.HasValue)
            model.BoxNumber = 10;
    }
}
