using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Helpers;
using PlanningAPI.Models;
using WebApi.Controllers;
using WebApi.Services;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDefinedLabelsController : ControllerBase
    {

        private readonly MydatabaseContext _context;
        public UserDefinedLabelsController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet("GetUdefValuesBYEntityId")]
        public async Task<IActionResult> GetUdefValuesBYEntityId(string tableId, string entityId, string companyId)
        {
            var fields = await _context.UdefFields
                .Include(x => x.Options)
                .Where(x => x.TableId == tableId && x.CompanyId == companyId)
                .OrderBy(x => x.SeqNo)
                .ToListAsync();

            var values = await _context.UdefValues
                .Where(x => x.EntityId == entityId && x.CompanyId == companyId)
                .ToListAsync();

            var valueLookup = values
                .GroupBy(v => v.FieldId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = fields
                .Select(f =>
                {
                    // ✅ Try get values if exist
                    valueLookup.TryGetValue(f.Id, out var fieldValues);

                    return new
                    {
                        f.Id,

                        GenId = fieldValues?.Select(v => v.GenId).FirstOrDefault(),

                        f.FieldName,
                        f.DataType,
                        f.IsMultiSelect,
                        f.IsRequired,

                        Options = f.Options
                            .Select(o => new { o.Value, o.Label }),

                        Values = fieldValues != null
                            ? fieldValues.Select(v => v.Value).ToList()
                            : new List<string>() // ✅ empty if no data
                    };
                })
                .ToList();

            return Ok(result);
        }

        //[HttpGet("GetUdefValuesBYEntityId")]
        //public async Task<IActionResult> GetUdefValuesBYEntityId(string tableId, string entityId, string companyId)
        //{
        //    var fields = await _context.UdefFields
        //        .Include(x => x.Options)
        //        .Where(x => x.TableId == tableId && x.CompanyId == companyId)
        //        .OrderBy(x => x.SeqNo)
        //        .ToListAsync();

        //    var values = await _context.UdefValues
        //        .Where(x => x.EntityId == entityId && x.CompanyId == companyId)
        //        .ToListAsync();

        //    var valueLookup = values
        //        .GroupBy(v => v.FieldId)
        //        .ToDictionary(g => g.Key, g => g.ToList());

        //    var result = fields
        //        .Where(f => valueLookup.ContainsKey(f.Id)) // ✅ only matching fields
        //        .Select(f =>
        //        {
        //            var fieldValues = valueLookup[f.Id];

        //            return new
        //            {
        //                f.Id,
        //                GenId = fieldValues.Select(v => v.GenId).FirstOrDefault(),
        //                f.FieldName,
        //                f.DataType,
        //                f.IsMultiSelect,
        //                f.IsRequired,

        //                Options = f.Options.Select(o => new { o.Value, o.Label }),

        //                Values = fieldValues.Select(v => v.Value).ToList()
        //            };
        //        })
        //        .ToList();

        //    return Ok(result);
        //}

        [HttpGet("udef")]
        public async Task<IActionResult> GetUdef(string tableId, string entityId, string companyId)
        {
            var fields = await _context.UdefFields
                .Include(x => x.Options)
                .Where(x => x.TableId == tableId && x.CompanyId == companyId)
                .OrderBy(x => x.SeqNo)
                .ToListAsync();

            var values = await _context.UdefValues
                .Where(x => x.EntityId == entityId && x.CompanyId == companyId)
                .ToListAsync();

            var result = fields.Select(f => new
            {
                f.Id,
                GenId = values
                    .Where(v => v.FieldId == f.Id)
                    .Select(v => v.GenId).FirstOrDefault(),
                f.FieldName,
                f.DataType, 
                f.IsMultiSelect,
                f.IsRequired,
                Options = f.Options.Select(o => new { o.Value, o.Label }),

                Values = values
                    .Where(v => v.FieldId == f.Id)
                    .Select(v => v.Value)
                    .ToList()
            });

            return Ok(result);
        }

        [HttpGet("udefGetByTableName")]
        public async Task<IActionResult> GetUdefByTableName(string tableId,string companyId)
        {
            var fields = await _context.UdefFields
                .Include(x => x.Options)
                .Where(x => x.TableId == tableId && x.CompanyId == companyId)
                .OrderBy(x => x.SeqNo)
                .ToListAsync();

            return Ok(fields);
        }


        [HttpPost("udef")]
        public async Task<IActionResult> SaveUdef(
            string entityId,
            string tableId,
            string companyId,
            [FromBody] List<UdefSaveDto> inputs)
        {
            if (string.IsNullOrWhiteSpace(entityId) ||
                string.IsNullOrWhiteSpace(tableId) ||
                string.IsNullOrWhiteSpace(companyId))
            {
                return BadRequest(new { message = "Invalid request parameters" });
            }

            if (inputs == null || !inputs.Any())
                return BadRequest(new { message = "No data provided" });

            var createdFields = new List<object>();

            // ✅ STEP 1: Load existing fields
            var existingFieldIds = inputs
                .Where(x => x.FieldId.HasValue)
                .Select(x => x.FieldId!.Value)
                .ToList();

            var fields = await _context.UdefFields
                .Where(x => existingFieldIds.Contains(x.Id)
                         && x.CompanyId == companyId
                         && x.TableId == tableId)
                .ToListAsync();

            // ❌ Invalid FieldIds
            var dbFieldIds = fields.Select(x => x.Id).ToHashSet();

            var invalidFieldIds = inputs
                .Where(x => x.FieldId.HasValue && !dbFieldIds.Contains(x.FieldId.Value))
                .Select(x => x.FieldId)
                .ToList();

            if (invalidFieldIds.Any())
            {
                await LogAudit("INVALID_FIELD", $"Invalid FieldIds: {string.Join(",", invalidFieldIds)}");
                return BadRequest(new
                {
                    message = "Invalid FieldIds",
                    invalidFieldIds
                });
            }

            // ✅ STEP 2: Create new fields
            foreach (var input in inputs.Where(x => !x.FieldId.HasValue))
            {
                if (string.IsNullOrWhiteSpace(input.FieldName) ||
                    string.IsNullOrWhiteSpace(input.DataType))
                {
                    return BadRequest(new
                    {
                        message = "FieldName and DataType required for new fields"
                    });
                }

                // ❌ Prevent duplicate field
                var exists = await _context.UdefFields.AnyAsync(x =>
                    x.TableId == tableId &&
                    x.CompanyId == companyId &&
                    x.FieldName.ToLower() == input.FieldName.ToLower());

                if (exists)
                {
                    return BadRequest($"Field '{input.FieldName}' already exists");
                }

                var newField = new UdefField
                {
                    TableId = tableId,
                    FieldName = input.FieldName,
                    DataType = input.DataType,
                    SeqNo = fields.Count + 1,
                    IsMultiSelect = input.IsMultiSelect,
                    CompanyId = companyId,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.UdefFields.AddAsync(newField);
                await _context.SaveChangesAsync();

                input.FieldId = newField.Id;
                fields.Add(newField);

                createdFields.Add(new
                {
                    fieldName = newField.FieldName,
                    fieldId = newField.Id
                });

                // 🔥 Auto-create dropdown options
                if (input.DataType == "L" && input.Values.Any())
                {
                    var udoptions = input.Values.Distinct().Select(v => new UdefOption
                    {
                        FieldId = newField.Id,
                        Value = v,
                        Label = v
                    });

                    await _context.UdefOptions.AddRangeAsync(udoptions);
                    await _context.SaveChangesAsync();
                }
            }

            // ✅ STEP 3: Load dropdown options
            var fieldIds = inputs.Select(x => x.FieldId!.Value).ToList();

            var options = await _context.UdefOptions
                .Where(o => fieldIds.Contains(o.FieldId))
                .ToListAsync();

            var errors = new List<string>();

            // ✅ STEP 4: VALIDATION
            foreach (var input in inputs)
            {
                var field = fields.First(x => x.Id == input.FieldId);
                var values = input.Values ?? new List<string>();

                // Required
                if (field.IsRequired && !values.Any())
                {
                    errors.Add($"{field.FieldName} is required");
                    continue;
                }

                // Datatype
                foreach (var val in values)
                {
                    if (!ValidateDataType(field.DataType, val))
                        errors.Add($"Invalid value '{val}' for {field.FieldName}");
                }

                // Dropdown
                if (field.DataType == "L")
                {
                    var validOptions = options
                        .Where(o => o.FieldId == field.Id)
                        .Select(o => o.Value)
                        .ToHashSet();

                    var invalidValues = values.Where(v => !validOptions.Contains(v)).ToList();

                    if (invalidValues.Any())
                        errors.Add($"Invalid option(s) for {field.FieldName}: {string.Join(",", invalidValues)}");
                }
            }

            if (errors.Any())
            {
                //await LogAudit("VALIDATION_FAILED", string.Join(" | ", errors));

                return BadRequest(new
                {
                    message = "Validation failed",
                    errors
                });
            }

            // ✅ STEP 5: SAVE DATA
            foreach (var input in inputs)
            {
                var field = fields.First(x => x.Id == input.FieldId);
                var values = input.Values ?? new List<string>();

                if (field.IsMultiSelect)
                {
                    var existing = _context.UdefValues.Where(x =>
                        x.EntityId == entityId &&
                        x.FieldId == field.Id &&
                        x.CompanyId == companyId);

                    _context.UdefValues.RemoveRange(existing);

                    if (values.Any())
                    {
                        var newValues = values.Select(v => new UdefValue
                        {
                            EntityId = entityId,
                            FieldId = field.Id,
                            Value = v,
                            CompanyId = companyId,
                            CreatedAt = DateTime.UtcNow
                        });

                        await _context.UdefValues.AddRangeAsync(newValues);
                    }
                }
                else
                {
                    var value = values.FirstOrDefault();

                    var existing = await _context.UdefValues.FirstOrDefaultAsync(x =>
                        x.EntityId == entityId &&
                        x.FieldId == field.Id &&
                        x.CompanyId == companyId);

                    if (string.IsNullOrEmpty(value))
                    {
                        if (existing != null)
                            _context.UdefValues.Remove(existing);
                    }
                    else if (existing != null)
                    {
                        existing.Value = value;
                        existing.CreatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        await _context.UdefValues.AddAsync(new UdefValue
                        {
                            EntityId = entityId,
                            FieldId = field.Id,
                            Value = value,
                            CompanyId = companyId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            // ✅ SUCCESS AUDIT
            //await LogAudit("SAVE_SUCCESS", $"Saved UDEF for Entity: {entityId}");

            return Ok(new
            {
                message = "Saved successfully",
                createdFields
            });
        }

        [HttpPost("udefV1")]
        public async Task<IActionResult> SaveUdefV1(
            string entityId,
            string tableId,
            string companyId,
            [FromBody] List<UdefSaveDto> inputs)
        {
            if (string.IsNullOrWhiteSpace(entityId) ||
                string.IsNullOrWhiteSpace(tableId) ||
                string.IsNullOrWhiteSpace(companyId))
            {
                return BadRequest(new { message = "Invalid request parameters" });
            }

            if (inputs == null || !inputs.Any())
                return BadRequest(new { message = "No data provided" });

            var createdFields = new List<object>();

            // ✅ STEP 1: Load existing fields by FieldId
            var existingFieldIds = inputs
                .Where(x => x.FieldId.HasValue)
                .Select(x => x.FieldId!.Value)
                .ToList();

            var fields = await _context.UdefFields
                .Where(x => existingFieldIds.Contains(x.Id)
                         && x.CompanyId == companyId
                         && x.TableId == tableId)
                .ToListAsync();

            var dbFieldIds = fields.Select(x => x.Id).ToHashSet();

            var invalidFieldIds = inputs
                .Where(x => x.FieldId.HasValue && !dbFieldIds.Contains(x.FieldId.Value))
                .Select(x => x.FieldId)
                .ToList();

            if (invalidFieldIds.Any())
            {
                return BadRequest(new
                {
                    message = "Invalid FieldIds",
                    invalidFieldIds
                });
            }

            // ✅ STEP 2: UPSERT FIELDS (no duplicate error)
            foreach (var input in inputs.Where(x => !x.FieldId.HasValue))
            {
                if (string.IsNullOrWhiteSpace(input.FieldName) ||
                    string.IsNullOrWhiteSpace(input.DataType))
                {
                    return BadRequest(new
                    {
                        message = "FieldName and DataType required for new fields"
                    });
                }

                var fieldEntity = await _context.UdefFields.FirstOrDefaultAsync(x =>
                    x.TableId == tableId &&
                    x.CompanyId == companyId &&
                    x.FieldName.ToLower() == input.FieldName.ToLower());

                if (fieldEntity == null)
                {
                    fieldEntity = new UdefField
                    {
                        TableId = tableId,
                        FieldName = input.FieldName,
                        DataType = input.DataType,
                        SeqNo = fields.Count + 1,
                        IsMultiSelect = input.IsMultiSelect,
                        CompanyId = companyId,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.UdefFields.AddAsync(fieldEntity);
                    await _context.SaveChangesAsync();

                    createdFields.Add(new
                    {
                        fieldName = fieldEntity.FieldName,
                        fieldId = fieldEntity.Id
                    });
                }

                input.FieldId = fieldEntity.Id;

                if (!fields.Any(f => f.Id == fieldEntity.Id))
                    fields.Add(fieldEntity);

                // 🔥 Dropdown option merge
                if (fieldEntity.DataType == "L" && input.Values.Any())
                {
                    var existingOptions = await _context.UdefOptions
                        .Where(o => o.FieldId == fieldEntity.Id)
                        .Select(o => o.Value)
                        .ToListAsync();

                    var newOptions = input.Values
                        .Where(v => !existingOptions.Contains(v))
                        .Distinct()
                        .Select(v => new UdefOption
                        {
                            FieldId = fieldEntity.Id,
                            Value = v,
                            Label = v
                        });

                    if (newOptions.Any())
                    {
                        await _context.UdefOptions.AddRangeAsync(newOptions);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            var fieldIds = inputs.Select(x => x.FieldId!.Value).ToList();

            // ✅ STEP 3: Load dropdown options
            var options = await _context.UdefOptions
                .Where(o => fieldIds.Contains(o.FieldId))
                .ToListAsync();

            var errors = new List<string>();

            // ✅ STEP 4: VALIDATION
            foreach (var input in inputs)
            {
                var field = fields.First(x => x.Id == input.FieldId);
                var values = input.Values ?? new List<string>();

                if (field.IsRequired && !values.Any())
                {
                    errors.Add($"{field.FieldName} is required");
                    continue;
                }

                foreach (var val in values)
                {
                    if (!ValidateDataType(field.DataType, val))
                        errors.Add($"Invalid value '{val}' for {field.FieldName}");
                }

                if (field.DataType == "L")
                {
                    var validOptions = options
                        .Where(o => o.FieldId == field.Id)
                        .Select(o => o.Value)
                        .ToHashSet();

                    var invalidValues = values.Where(v => !validOptions.Contains(v)).ToList();

                    if (invalidValues.Any())
                        errors.Add($"Invalid option(s) for {field.FieldName}: {string.Join(",", invalidValues)}");
                }
            }

            if (errors.Any())
            {
                return BadRequest(new
                {
                    message = "Validation failed",
                    errors
                });
            }

            // ✅ STEP 5: BULK FETCH EXISTING VALUES
            var allExisting = await _context.UdefValues
                .Where(x => x.EntityId == entityId &&
                            fieldIds.Contains(x.FieldId) &&
                            x.CompanyId == companyId)
                .ToListAsync();

            // ✅ STEP 6: SMART SAVE
            foreach (var input in inputs)
            {
                var field = fields.First(x => x.Id == input.FieldId);
                var values = input.Values ?? new List<string>();

                var existingValues = allExisting
                    .Where(x => x.FieldId == field.Id)
                    .ToList();

                if (field.IsMultiSelect)
                {
                    var existingSet = existingValues.Select(x => x.Value).ToHashSet();
                    var newSet = values.ToHashSet();

                    // DELETE removed
                    var toDelete = existingValues.Where(x => !newSet.Contains(x.Value)).ToList();
                    if (toDelete.Any())
                        _context.UdefValues.RemoveRange(toDelete);

                    // INSERT new
                    var toInsert = newSet
                        .Where(v => !existingSet.Contains(v))
                        .Select(v => new UdefValue
                        {
                            GenId = input.GenId,
                            EntityId = entityId,
                            FieldId = field.Id,
                            Value = v,
                            CompanyId = companyId,
                            CreatedAt = DateTime.UtcNow
                        });

                    if (toInsert.Any())
                        await _context.UdefValues.AddRangeAsync(toInsert);

                    // UPDATE timestamp
                    foreach (var item in existingValues.Where(x => newSet.Contains(x.Value)))
                    {
                        item.CreatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    var value = values.FirstOrDefault();
                    var existing = existingValues.FirstOrDefault();

                    if (string.IsNullOrEmpty(value))
                    {
                        if (existing != null)
                            _context.UdefValues.Remove(existing);
                    }
                    else if (existing != null)
                    {
                        if (existing.Value != value)
                        {
                            existing.Value = value;
                            existing.CreatedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        await _context.UdefValues.AddAsync(new UdefValue
                        {
                            GenId = input.GenId,
                            EntityId = entityId,
                            FieldId = field.Id,
                            Value = value,
                            CompanyId = companyId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Saved successfully",
                createdFields
            });
        }


        [HttpPost("AddField")]
        public async Task<IActionResult> AddField(
        string tableId,
        string companyId,
        [FromBody] List<UdefSaveDto> inputs)
        {
            if (string.IsNullOrWhiteSpace(tableId) ||
                string.IsNullOrWhiteSpace(companyId))
            {
                return BadRequest(new { message = "Invalid request parameters" });
            }

            if (inputs == null || !inputs.Any())
                return BadRequest(new { message = "No data provided" });

            var createdFields = new List<object>();

            // ✅ STEP 1: Load existing fields by FieldId
            var existingFieldIds = inputs
                .Where(x => x.FieldId.HasValue)
                .Select(x => x.FieldId!.Value)
                .ToList();

            var fields = await _context.UdefFields
                .Where(x => existingFieldIds.Contains(x.Id)
                         && x.CompanyId == companyId
                         && x.TableId == tableId)
                .ToListAsync();

            var dbFieldIds = fields.Select(x => x.Id).ToHashSet();

            var invalidFieldIds = inputs
                .Where(x => x.FieldId.HasValue && !dbFieldIds.Contains(x.FieldId.Value))
                .Select(x => x.FieldId)
                .ToList();

            if (invalidFieldIds.Any())
            {
                return BadRequest(new
                {
                    message = "Invalid FieldIds",
                    invalidFieldIds
                });
            }

            // ✅ STEP 2: UPSERT FIELDS (no duplicate error)
            foreach (var input in inputs.Where(x => !x.FieldId.HasValue))
            {
                if (string.IsNullOrWhiteSpace(input.FieldName) ||
                    string.IsNullOrWhiteSpace(input.DataType))
                {
                    return BadRequest(new
                    {
                        message = "FieldName and DataType required for new fields"
                    });
                }

                var fieldEntity = await _context.UdefFields.FirstOrDefaultAsync(x =>
                    x.TableId == tableId &&
                    x.CompanyId == companyId &&
                    x.FieldName.ToLower() == input.FieldName.ToLower());

                if (fieldEntity == null)
                {
                    fieldEntity = new UdefField
                    {
                        TableId = tableId,
                        FieldName = input.FieldName,
                        DataType = input.DataType,
                        SeqNo = fields.Count + 1,
                        IsMultiSelect = input.IsMultiSelect,
                        CompanyId = companyId,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.UdefFields.AddAsync(fieldEntity);
                    await _context.SaveChangesAsync();

                    createdFields.Add(new
                    {
                        fieldName = fieldEntity.FieldName,
                        fieldId = fieldEntity.Id
                    });
                }

                input.FieldId = fieldEntity.Id;

                if (!fields.Any(f => f.Id == fieldEntity.Id))
                    fields.Add(fieldEntity);

                // 🔥 Dropdown option merge
                if (fieldEntity.DataType == "L" && input.Values.Any())
                {
                    var existingOptions = await _context.UdefOptions
                        .Where(o => o.FieldId == fieldEntity.Id)
                        .Select(o => o.Value)
                        .ToListAsync();

                    var newOptions = input.Values
                        .Where(v => !existingOptions.Contains(v))
                        .Distinct()
                        .Select(v => new UdefOption
                        {
                            FieldId = fieldEntity.Id,
                            Value = v,
                            Label = v
                        });

                    if (newOptions.Any())
                    {
                        await _context.UdefOptions.AddRangeAsync(newOptions);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            var fieldIds = inputs.Select(x => x.FieldId!.Value).ToList();

            // ✅ STEP 3: Load dropdown options
            var options = await _context.UdefOptions
                .Where(o => fieldIds.Contains(o.FieldId))
                .ToListAsync();

            var errors = new List<string>();

            // ✅ STEP 4: VALIDATION
            foreach (var input in inputs)
            {
                var field = fields.First(x => x.Id == input.FieldId);
                var values = input.Values ?? new List<string>();

                if (field.IsRequired && !values.Any())
                {
                    errors.Add($"{field.FieldName} is required");
                    continue;
                }

                foreach (var val in values)
                {
                    if (!ValidateDataType(field.DataType, val))
                        errors.Add($"Invalid value '{val}' for {field.FieldName}");
                }

                if (field.DataType == "L")
                {
                    var validOptions = options
                        .Where(o => o.FieldId == field.Id)
                        .Select(o => o.Value)
                        .ToHashSet();

                    var invalidValues = values.Where(v => !validOptions.Contains(v)).ToList();

                    if (invalidValues.Any())
                        errors.Add($"Invalid option(s) for {field.FieldName}: {string.Join(",", invalidValues)}");
                }
            }

            if (errors.Any())
            {
                return BadRequest(new
                {
                    message = "Validation failed",
                    errors
                });
            }

            //// ✅ STEP 5: BULK FETCH EXISTING VALUES
            //var allExisting = await _context.UdefValues
            //    .Where(x => x.EntityId == entityId &&
            //                fieldIds.Contains(x.FieldId) &&
            //                x.CompanyId == companyId)
            //    .ToListAsync();

            //// ✅ STEP 6: SMART SAVE
            //foreach (var input in inputs)
            //{
            //    var field = fields.First(x => x.Id == input.FieldId);
            //    var values = input.Values ?? new List<string>();

            //    var existingValues = allExisting
            //        .Where(x => x.FieldId == field.Id)
            //        .ToList();

            //    if (field.IsMultiSelect)
            //    {
            //        var existingSet = existingValues.Select(x => x.Value).ToHashSet();
            //        var newSet = values.ToHashSet();

            //        // DELETE removed
            //        var toDelete = existingValues.Where(x => !newSet.Contains(x.Value)).ToList();
            //        if (toDelete.Any())
            //            _context.UdefValues.RemoveRange(toDelete);

            //        // INSERT new
            //        var toInsert = newSet
            //            .Where(v => !existingSet.Contains(v))
            //            .Select(v => new UdefValue
            //            {
            //                GenId = input.GenId,
            //                EntityId = entityId,
            //                FieldId = field.Id,
            //                Value = v,
            //                CompanyId = companyId,
            //                CreatedAt = DateTime.UtcNow
            //            });

            //        if (toInsert.Any())
            //            await _context.UdefValues.AddRangeAsync(toInsert);

            //        // UPDATE timestamp
            //        foreach (var item in existingValues.Where(x => newSet.Contains(x.Value)))
            //        {
            //            item.CreatedAt = DateTime.UtcNow;
            //        }
            //    }
            //    else
            //    {
            //        var value = values.FirstOrDefault();
            //        var existing = existingValues.FirstOrDefault();

            //        if (string.IsNullOrEmpty(value))
            //        {
            //            if (existing != null)
            //                _context.UdefValues.Remove(existing);
            //        }
            //        else if (existing != null)
            //        {
            //            if (existing.Value != value)
            //            {
            //                existing.Value = value;
            //                existing.CreatedAt = DateTime.UtcNow;
            //            }
            //        }
            //        else
            //        {
            //            await _context.UdefValues.AddAsync(new UdefValue
            //            {
            //                GenId = input.GenId,
            //                EntityId = entityId,
            //                FieldId = field.Id,
            //                Value = value,
            //                CompanyId = companyId,
            //                CreatedAt = DateTime.UtcNow
            //            });
            //        }
            //    }
            //}

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Saved successfully",
                createdFields
            });
        }

        [NonAction]
        private bool ValidateDataType(string type, string value)
        {
            return type switch
            {
                "T" => true, // text
                "N" => decimal.TryParse(value, out _),
                "D" => DateTime.TryParse(value, out _),
                "L" => true, // handled separately
                _ => false
            };
        }
        [NonAction]
        private async Task LogAudit(string action, string message)
        {
            await _context.AuditLogs.AddAsync(new AuditLog
            {
                TableName = "udef_value",
                Action = action,
                NewValues = message,
                ModifiedBy = User?.Identity?.Name ?? "system",
                CompanyId = "SYSTEM",
                TimeStamp = DateTime.UtcNow,
                RequestPath = HttpContext.Request.Path,
                HttpMethod = HttpContext.Request.Method
            });

            //await _context.SaveChangesAsync();
        }
    }
}
