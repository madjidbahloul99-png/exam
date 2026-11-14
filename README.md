# Model Management Page - Factory Models (dbo.t_base_model)

## Technology stack
- **Frontend:** Windows Forms (.NET 10, C#), single-window desktop application
- **Backend:** same WinForms process; data access with ADO.NET (`Microsoft.Data.SqlClient`)
- **Database:** SQL Server, table `dbo.t_base_model`
- **Configuration:** `appsettings.json` (connection string + audit username)

## Files
- `exam/exam.csproj` — project file (net10.0-windows, WinForms, Microsoft.Data.SqlClient 7.1.1)
- `exam/Program.cs` — application entry point
- `exam/Form1.cs` — main page: search/list, add, edit, delete
- `exam/Models/TBaseModel.cs` — record model
- `exam/Data/TBaseModelRepository.cs` — data access with parameterized SQL + validation
- `exam/Data/ModelExceptions.cs` — validation/duplicate exceptions
- `exam/Forms/AddEditModelForm.cs` — add/edit dialog
- `exam/appsettings.json` — connection settings
- `Database/Create_t_base_model.sql` — SQL Server table creation script

## Database setup
1. Install SQL Server (the default instance `.` or `.\SQLEXPRESS` works).
2. Create the database (example): `CREATE DATABASE ExamDb;`
3. Run the creation script against that database:

```sql
sqlcmd -S . -E -d ExamDb -i Database\Create_t_base_model.sql
```

The script creates `dbo.t_base_model` with the composite primary key `(factoryCode, model)`, which enforces that the same model cannot exist twice within one factory (different factories may reuse the same model). `id` defaults to `NEWID()`, `imeiCount` defaults to 2, `boxNumber` defaults to 10.

## Connection settings
Edit `exam\appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ExamDb": "Server=.;Database=ExamDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "AuditUser": "exam_user"
}
```

- `Server=.` — default instance on the local machine. For a named instance use `Server=.\SQLEXPRESS` or `Server=myhost,1433`.
- `Database=ExamDb` — target database.
- `Trusted_Connection=True` — Windows integrated authentication (no password in config).
- `TrustServerCertificate=True` — for local/dev SQL Server certificates.
- `AuditUser` — fixed username stamped on create/modify (`exam_user`); no login feature is required by the exam.

## Startup
1. Open a terminal in `D:\CondoreKanban\Newekanban\API\exam`.
2. `dotnet build exam.slnx`
3. `dotnet run --project exam\exam.csproj` (or run `bin\Debug\net10.0-windows\exam.exe`).

The window opens and loads all model records from the database automatically.

## Using the page
The single page shows all records in a grid (factory code, model, description, color, IMEI count, units per box, notes, create/modify date and user). `id` is hidden.

**Search:** enter a factory code (exact match) and/or a model fragment (contains match), then click **Search** or press Enter. Both filters apply together when both are filled. Click **Reset** to show all records. A "No records match." message appears when nothing matches.

**Add:** click **Add**. Factory code and model are required; IMEI count and units per box are prefilled with 2 and 10 (clearing them saves the default value). Other fields are optional. Click **Save**. If the factory+model combination already exists, the message "This model already exists for this factory." is shown and nothing is duplicated.

**Edit:** select a row and click **Edit**. Factory code and model are read-only; description, color, IMEI count, units per box, and notes can be changed. Clearing an optional numeric field saves NULL. Click **Save** to persist; creation date/user are preserved, modification date/user are updated.

**Delete:** select a row and click **Delete**. A confirmation shows the factory code and model. **Cancel** keeps the record. **Yes** deletes only the selected record.

**Validation:** all input is validated on the backend with parameterized SQL (no string concatenation). Each failure shows a specific message (required factory code/model, 200-character text limits, positive integer within int range for IMEI count/units per box). Operational failures show a clear message.

## Verifying persistence
After add/edit/delete, either reload the page or query the database directly, e.g.:

```sql
SELECT factoryCode, model, modelDesc, color, imeiCount, boxNumber, note,
       createDate, createUser, modifyDate, modifyUser
FROM dbo.t_base_model
ORDER BY factoryCode, model;
```

Changes survive a restart because every write hits `dbo.t_base_model` in SQL Server.
