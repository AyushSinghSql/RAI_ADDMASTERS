using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlanningAPI.Models;
using PlanningAPI.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/accounts-payable-vouchers")]
    public sealed class AccountsPayableVouchersController : ControllerBase
    {
        private readonly MydatabaseContext _context;
        private readonly VoucherDatabaseHandler _handler;

        public AccountsPayableVouchersController(MydatabaseContext context, IOptions<VoucherDatabaseOptions> options)
        {
            _context = context;
            _handler = new VoucherDatabaseHandler(context, options);
        }

        [HttpGet]
        public async Task<IActionResult> GetHeaders([FromQuery] int limit, CancellationToken cancellationToken)
        {
            return Ok(await _handler.GetVoucherHeadersAsync(limit <= 0 ? 100 : limit, cancellationToken));
        }

        [HttpGet("vendor-employees")]
        public async Task<IActionResult> GetVendorEmployees(CancellationToken cancellationToken)
        {
            return Ok(await _handler.GetVendorEmployeesAsync(cancellationToken));
        }

        [HttpGet("plc-codes")]
        public async Task<IActionResult> GetPlcCodes(CancellationToken cancellationToken)
        {
            return Ok(await _handler.GetPlcCodesAsync(cancellationToken));
        }

        [HttpGet("glc-codes")]
        public async Task<IActionResult> GetGlcCodes(CancellationToken cancellationToken)
        {
            return Ok(await _handler.GetGlcCodesAsync(cancellationToken));
        }

        [HttpGet("{voucherKey:int}")]
        public async Task<IActionResult> GetByVoucherKey(int voucherKey, CancellationToken cancellationToken)
        {
            var voucher = await _handler.GetVoucherAsync(voucherKey, cancellationToken);
            return voucher is null ? NotFound() : Ok(voucher);
        }

        [HttpPost]
        public async Task<IActionResult> Create(VoucherWriteRequest voucher, CancellationToken cancellationToken)
        {
            try
            {
                var voucherKey = await _handler.CreateVoucherAsync(voucher, cancellationToken);
                return CreatedAtAction(nameof(GetByVoucherKey), new { voucherKey }, new VoucherCreateResponse
                {
                    Success = true,
                    Message = "Voucher created successfully.",
                    VoucherKey = voucherKey
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PostgresException ex)
            {
                return Problem(
                    title: "Database rejected the voucher insert.",
                    detail: GetDatabaseErrorDetail(ex),
                    statusCode: 400);
            }
        }

        [HttpPut("{voucherKey:int}")]
        public async Task<IActionResult> Update(int voucherKey, VoucherWriteRequest voucher, CancellationToken cancellationToken)
        {
            try
            {
                var status = await _handler.UpdateVoucherAsync(voucherKey, voucher, cancellationToken);
                return status switch
                {
                    VoucherUpdateStatus.Updated => Ok(new VoucherCreateResponse
                    {
                        Success = true,
                        Message = "Voucher updated successfully.",
                        VoucherKey = voucherKey
                    }),
                    VoucherUpdateStatus.Posted => Conflict("Posted vouchers cannot be updated."),
                    _ => NotFound()
                };
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PostgresException ex)
            {
                return Problem(
                    title: "Database rejected the voucher update.",
                    detail: GetDatabaseErrorDetail(ex),
                    statusCode: 400);
            }
        }

        [HttpDelete("{voucherKey:int}")]
        public async Task<IActionResult> Delete(int voucherKey, CancellationToken cancellationToken)
        {
            var deleted = await _handler.DeleteVoucherAsync(voucherKey, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }

        private static string GetDatabaseErrorDetail(PostgresException ex)
        {
            return ex.ConstraintName switch
            {
                "voucher_hdr_term" => "Invalid terms_dc. The value must already exist in public.vendor_terms.terms_dc.",
                "voucher_hdr_fypd" => "Invalid company_id, fy_cd, period_no, or sub_period_no. The combination must already exist in public.sub_period.",
                _ => ex.MessageText
            };
        }
    }

    internal sealed class VoucherDatabaseHandler
    {
        private static readonly string[] VoucherTables =
        {
            "voucher_hdr",
            "voucher_ln",
            "voucher_ln_account",
            "voucher_lab_vendor"
        };

        private readonly MydatabaseContext _context;
        private readonly string _schemaName;
        private readonly string _quotedSchemaName;

        public VoucherDatabaseHandler(MydatabaseContext context, IOptions<VoucherDatabaseOptions> options)
        {
            _context = context;
            _schemaName = options.Value.Schema;
            _quotedSchemaName = QuoteIdentifier(options.Value.Schema);
        }

        private async Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken cancellationToken)
        {
            var connection = (NpgsqlConnection)_context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }
            return connection;
        }

        public async Task<IReadOnlyList<Dictionary<string, object?>>> GetVoucherHeadersAsync(
            int limit,
            CancellationToken cancellationToken)
        {
            limit = Math.Clamp(limit, 1, 500);

            var connection = await GetOpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = $"select * from {_quotedSchemaName}.voucher_hdr order by voucher_key desc limit @limit";
            command.Parameters.AddWithValue("limit", limit);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(ReadRow(reader));
            }

            return rows;
        }

        public async Task<VoucherAggregateDto?> GetVoucherAsync(int voucherKey, CancellationToken cancellationToken)
        {
            var connection = await GetOpenConnectionAsync(cancellationToken);

            var header = await GetSingleByKeysAsync(connection, "voucher_hdr", new Dictionary<string, object?>
            {
                ["voucher_key"] = voucherKey
            }, cancellationToken);

            if (header is null)
            {
                return null;
            }

            var lines = await GetRowsByKeysAsync(connection, "voucher_ln", new Dictionary<string, object?>
            {
                ["voucher_key"] = voucherKey
            }, cancellationToken);

            var aggregateLines = new List<VoucherLineAggregateDto>();
            foreach (var line in lines)
            {
                var voucherLnKey = Convert.ToInt32(line["voucher_ln_key"]);
                var childKeys = new Dictionary<string, object?>
                {
                    ["voucher_key"] = voucherKey,
                    ["voucher_ln_key"] = voucherLnKey
                };

                aggregateLines.Add(new VoucherLineAggregateDto
                {
                    Line = line,
                    Accounts = await GetVoucherAccountsWithNamesAsync(connection, voucherKey, voucherLnKey, cancellationToken),
                    LabVendors = await GetRowsByKeysAsync(connection, "voucher_lab_vendor", childKeys, cancellationToken)
                });
            }

            return new VoucherAggregateDto
            {
                Header = header,
                Lines = aggregateLines
            };
        }

        private async Task<List<Dictionary<string, object?>>> GetVoucherAccountsWithNamesAsync(
            NpgsqlConnection connection,
            int voucherKey,
            int voucherLnKey,
            CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"""
                select vla.*, a.acct_name, o.org_name
                from {_quotedSchemaName}.voucher_ln_account vla
                left join {_quotedSchemaName}.account a on a.acct_id = vla.account_id
                left join {_quotedSchemaName}.organization o on o.org_id = vla.org_id
                where vla.voucher_key = @voucher_key and vla.voucher_ln_key = @voucher_ln_key
                """;
            command.Parameters.AddWithValue("voucher_key", voucherKey);
            command.Parameters.AddWithValue("voucher_ln_key", voucherLnKey);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(ReadRow(reader));
            }

            return rows;
        }

        public async Task<IReadOnlyList<Dictionary<string, object?>>> GetVendorEmployeesAsync(CancellationToken cancellationToken)
        {
            var connection = await GetOpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = $"select * from {_quotedSchemaName}.vendor_employee order by vend_empl_id limit 500";

            var rows = new List<Dictionary<string, object?>>();
            try
            {
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    rows.Add(ReadRow(reader));
                }
            }
            catch
            {
                // Return empty list if table not available
            }

            return rows;
        }

        public async Task<IReadOnlyList<Dictionary<string, object?>>> GetPlcCodesAsync(CancellationToken cancellationToken)
        {
            var connection = await GetOpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = $"select * from {_quotedSchemaName}.plc_codes limit 500";

            var rows = new List<Dictionary<string, object?>>();
            try
            {
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    rows.Add(ReadRow(reader));
                }
            }
            catch
            {
                try
                {
                    await using var fallbackCmd = connection.CreateCommand();
                    fallbackCmd.CommandText = $"select * from {_quotedSchemaName}.bill_lab_cat limit 500";
                    await using var reader = await fallbackCmd.ExecuteReaderAsync(cancellationToken);
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        rows.Add(ReadRow(reader));
                    }
                }
                catch
                {
                    // Return empty list if table not available
                }
            }

            return rows;
        }

        public async Task<IReadOnlyList<Dictionary<string, object?>>> GetGlcCodesAsync(CancellationToken cancellationToken)
        {
            var connection = await GetOpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = $"select * from {_quotedSchemaName}.genl_lab_cat limit 500";

            var rows = new List<Dictionary<string, object?>>();
            try
            {
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    rows.Add(ReadRow(reader));
                }
            }
            catch
            {
                // Return empty list if table not available
            }

            return rows;
        }

        public async Task<int> CreateVoucherAsync(VoucherWriteRequest request, CancellationToken cancellationToken)
        {
            if (request.Header.Count == 0)
            {
                throw new ArgumentException("header object is required.");
            }

            var connection = await GetOpenConnectionAsync(cancellationToken);
            var schemas = await LoadSchemaAsync(connection, cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                await NormalizeFiscalPeriodAsync(connection, transaction, request.Header, cancellationToken);
                ApplyHeaderDefaults(request.Header);

                var voucherKey = await InsertRowAsync(
                    connection,
                    transaction,
                    schemas["voucher_hdr"],
                    request.Header,
                    forcedValues: null,
                    returningColumn: "voucher_key",
                    cancellationToken);

                foreach (var lineRequest in request.Lines)
                {
                    ApplyLineDefaults(lineRequest.Line);
                    var voucherLnKey = await InsertRowAsync(
                        connection,
                        transaction,
                        schemas["voucher_ln"],
                        lineRequest.Line,
                        new Dictionary<string, object?> { ["voucher_key"] = voucherKey },
                        "voucher_ln_key",
                        cancellationToken);

                    foreach (var account in lineRequest.Accounts)
                    {
                        ApplyAccountDefaults(account);
                        await InsertRowAsync(
                            connection,
                            transaction,
                            schemas["voucher_ln_account"],
                            account,
                            new Dictionary<string, object?>
                            {
                                ["voucher_key"] = voucherKey,
                                ["voucher_ln_key"] = voucherLnKey
                            },
                            "voucher_ln_account_key",
                            cancellationToken);
                    }

                    foreach (var labVendor in lineRequest.LabVendors)
                    {
                        ApplyLabVendorDefaults(labVendor);
                        await InsertRowAsync(
                            connection,
                            transaction,
                            schemas["voucher_lab_vendor"],
                            labVendor,
                            new Dictionary<string, object?>
                            {
                                ["voucher_key"] = voucherKey,
                                ["voucher_ln_key"] = voucherLnKey
                            },
                            "voucher_ln_vendor_key",
                            cancellationToken);
                    }
                }

                await transaction.CommitAsync(cancellationToken);
                return voucherKey;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<VoucherUpdateStatus> UpdateVoucherAsync(
            int voucherKey,
            VoucherWriteRequest request,
            CancellationToken cancellationToken)
        {
            if (request.Header.Count == 0)
            {
                throw new ArgumentException("header object is required.");
            }

            var connection = await GetOpenConnectionAsync(cancellationToken);
            var schemas = await LoadSchemaAsync(connection, cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                await NormalizeFiscalPeriodAsync(connection, transaction, request.Header, cancellationToken);
                ApplyHeaderDefaults(request.Header);

                var existingHeader = await GetSingleByKeysAsync(connection, "voucher_hdr", new Dictionary<string, object?>
                {
                    ["voucher_key"] = voucherKey
                }, cancellationToken);

                if (existingHeader is null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return VoucherUpdateStatus.NotFound;
                }

                if (existingHeader.TryGetValue("posted_ap_fl", out var postedApFlag) &&
                    string.Equals(Convert.ToString(postedApFlag), "Y", StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return VoucherUpdateStatus.Posted;
                }

                await UpdateRowAsync(
                    connection,
                    transaction,
                    schemas["voucher_hdr"],
                    request.Header,
                    new Dictionary<string, object?> { ["voucher_key"] = voucherKey },
                    cancellationToken);

                await DeleteByKeysAsync(connection, transaction, "voucher_lab_vendor", new Dictionary<string, object?> { ["voucher_key"] = voucherKey }, cancellationToken);
                await DeleteByKeysAsync(connection, transaction, "voucher_ln_account", new Dictionary<string, object?> { ["voucher_key"] = voucherKey }, cancellationToken);
                await DeleteByKeysAsync(connection, transaction, "voucher_ln", new Dictionary<string, object?> { ["voucher_key"] = voucherKey }, cancellationToken);

                foreach (var lineRequest in request.Lines)
                {
                    ApplyLineDefaults(lineRequest.Line);
                    var voucherLnKey = await InsertRowAsync(
                        connection,
                        transaction,
                        schemas["voucher_ln"],
                        lineRequest.Line,
                        new Dictionary<string, object?> { ["voucher_key"] = voucherKey },
                        "voucher_ln_key",
                        cancellationToken);

                    foreach (var account in lineRequest.Accounts)
                    {
                        ApplyAccountDefaults(account);
                        await InsertRowAsync(
                            connection,
                            transaction,
                            schemas["voucher_ln_account"],
                            account,
                            new Dictionary<string, object?>
                            {
                                ["voucher_key"] = voucherKey,
                                ["voucher_ln_key"] = voucherLnKey
                            },
                            "voucher_ln_account_key",
                            cancellationToken);
                    }

                    foreach (var labVendor in lineRequest.LabVendors)
                    {
                        ApplyLabVendorDefaults(labVendor);
                        await InsertRowAsync(
                            connection,
                            transaction,
                            schemas["voucher_lab_vendor"],
                            labVendor,
                            new Dictionary<string, object?>
                            {
                                ["voucher_key"] = voucherKey,
                                ["voucher_ln_key"] = voucherLnKey
                            },
                            "voucher_ln_vendor_key",
                            cancellationToken);
                    }
                }

                await transaction.CommitAsync(cancellationToken);
                return VoucherUpdateStatus.Updated;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<bool> DeleteVoucherAsync(int voucherKey, CancellationToken cancellationToken)
        {
            var connection = await GetOpenConnectionAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                await DeleteByKeysAsync(connection, transaction, "voucher_lab_vendor", new Dictionary<string, object?> { ["voucher_key"] = voucherKey }, cancellationToken);
                await DeleteByKeysAsync(connection, transaction, "voucher_ln_account", new Dictionary<string, object?> { ["voucher_key"] = voucherKey }, cancellationToken);
                await DeleteByKeysAsync(connection, transaction, "voucher_ln", new Dictionary<string, object?> { ["voucher_key"] = voucherKey }, cancellationToken);
                var deleted = await DeleteByKeysAsync(connection, transaction, "voucher_hdr", new Dictionary<string, object?> { ["voucher_key"] = voucherKey }, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return deleted > 0;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task NormalizeFiscalPeriodAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            Dictionary<string, JsonElement> header,
            CancellationToken cancellationToken)
        {
            var companyId = GetJsonString(header, "company_id") ?? "1";
            var fiscalYear = GetJsonString(header, "fy_cd");
            var periodNo = GetJsonInt(header, "period_no");
            var subPeriodNo = GetJsonInt(header, "sub_period_no");

            var resolved = await FindSubPeriodAsync(
                connection,
                transaction,
                companyId,
                fiscalYear,
                periodNo,
                subPeriodNo,
                exactOnly: true,
                cancellationToken);

            resolved ??= await FindSubPeriodAsync(
                connection,
                transaction,
                companyId,
                fiscalYear,
                periodNo,
                subPeriodNo,
                exactOnly: false,
                cancellationToken);

            if (resolved is null)
            {
                throw new ArgumentException("No valid fiscal period exists in public.sub_period for the selected company.");
            }

            header["company_id"] = JsonSerializer.SerializeToElement(resolved.CompanyId);
            header["fy_cd"] = JsonSerializer.SerializeToElement(resolved.FiscalYear);
            header["period_no"] = JsonSerializer.SerializeToElement(resolved.PeriodNo);
            header["sub_period_no"] = JsonSerializer.SerializeToElement(resolved.SubPeriodNo);
        }

        private static void ApplyHeaderDefaults(Dictionary<string, JsonElement> header)
        {
            SetDefault(header, "approved_fl", "N");
            SetDefault(header, "posted_ap_fl", "N");
            SetDefault(header, "disc_pct_rt", 0);
            SetDefault(header, "cst_amt", 0);
            SetDefault(header, "sales_tax_amt", 0);
            SetDefault(header, "invc_amt", 0);
            SetDefault(header, "disc_amt", 0);
            SetDefault(header, "due_amt", GetDecimal(header, "invc_amt") ?? 0);
            SetDefault(header, "taxable_fl", "N");
            SetDefault(header, "chk_amt", 0);
            SetDefault(header, "chk_no", 0);
            SetDefault(header, "rtn_rt", 0);
            SetDefault(header, "rtn_nt", "");
            SetDefault(header, "sep_chk_fl", "N");
            SetDefault(header, "notes", "");
            SetDefault(header, "s_sales_tax_src_cd", "N");
            SetDefault(header, "ext_po_id", "");
            SetDefault(header, "s_jnl_cd", "AP");
            SetDefault(header, "voucher_no", 0);
            SetDefault(header, "hold_voucher_fl", "N");
            SetDefault(header, "recur_fl", "N");
            SetDefault(header, "disc_taken_amt", 0);
            SetDefault(header, "dflt_ps_id", "");
            SetDefault(header, "use_tax_amt", 0);
            SetDefault(header, "pay_when_paid_fl", "N");
            SetDefault(header, "s_taxable_cd", "N");
            SetDefault(header, "s_recpt_discr_cd", "N");
            SetDefault(header, "s_po_discr_cd", "N");
            SetDefault(header, "dm_fl", "N");
            SetDefault(header, "dm_prntd_fl", "N");
            SetDefault(header, "auto_create_fl", "N");
            SetDefault(header, "print_note_fl", "N");
            SetDefault(header, "recur_tmplt_fl", "N");
            SetDefault(header, "recur_par_voucher_no", 0);
            SetDefault(header, "s_invc_type", "N");
            SetDefault(header, "ship_amt", 0);
            SetDefault(header, "ovr_bud_fl", "N");
            SetDefault(header, "s_subctr_pay_cd", "N");
            SetDefault(header, "paywpd_amt", 0);
            SetDefault(header, "trn_cst_amt", GetDecimal(header, "cst_amt") ?? 0);
            SetDefault(header, "trn_disc_amt", GetDecimal(header, "disc_amt") ?? 0);
            SetDefault(header, "trn_due_amt", GetDecimal(header, "due_amt") ?? 0);
            SetDefault(header, "trn_invc_amt", GetDecimal(header, "invc_amt") ?? 0);
            SetDefault(header, "trn_sales_tax_amt", GetDecimal(header, "sales_tax_amt") ?? 0);
            SetDefault(header, "trn_ship_amt", 0);
            SetDefault(header, "trn_use_tax_amt", GetDecimal(header, "use_tax_amt") ?? 0);
            SetDefault(header, "trn_crncy_cd", "USD");
            SetDefault(header, "pay_crncy_cd", "USD");
            SetDefault(header, "trn_to_eur_rt", 1);
            SetDefault(header, "eur_to_func_rt", 1);
            SetDefault(header, "func_to_eur_rt", 1);
            SetDefault(header, "eur_to_pay_rt", 1);
            SetDefault(header, "trn_freeze_rt_fl", "N");
            SetDefault(header, "pay_freeze_rt_fl", "N");
            SetDefault(header, "time_stamp", DateTime.UtcNow);
            SetDefault(header, "entr_dtt", DateTime.UtcNow);
            SetDefault(header, "modified_by", GetJsonString(header, "entr_user_id") ?? "SYSTEM");
        }

        private static void ApplyLineDefaults(Dictionary<string, JsonElement> line)
        {
            var extCost = GetDecimal(line, "ext_cst_amt") ?? GetDecimal(line, "net_amt") ?? 0;
            var tax = GetDecimal(line, "sales_tax_amt") ?? 0;
            var net = GetDecimal(line, "net_amt") ?? extCost + tax;
            SetDefault(line, "qty", 1);
            SetDefault(line, "ext_cst_amt", extCost);
            SetDefault(line, "ln_chg_cst_amt", 0);
            SetDefault(line, "sales_tax_amt", tax);
            SetDefault(line, "ln_chg_tax_amt", 0);
            SetDefault(line, "tot_bef_disc_amt", GetDecimal(line, "tot_bef_disc_amt") ?? net);
            SetDefault(line, "net_amt", net);
            SetDefault(line, "taxable_fl", "N");
            SetDefault(line, "sales_tax_nt", "");
            SetDefault(line, "notes", "");
            SetDefault(line, "voucher_ln_desc", "Line");
            SetDefault(line, "s_po_ln_type", "M");
            SetDefault(line, "discr_unit_prc_amt", 0);
            SetDefault(line, "discr_unit_prc_rt", 0);
            SetDefault(line, "discr_qty_rt", 0);
            SetDefault(line, "unit_cst_amt", extCost);
            SetDefault(line, "use_tax_amt", 0);
            SetDefault(line, "ln_chg_use_tax_amt", 0);
            SetDefault(line, "disc_amt", 0);
            SetDefault(line, "s_taxable_cd", "N");
            SetDefault(line, "discr_tot_amt", 0);
            SetDefault(line, "rma_no_id", "");
            SetDefault(line, "modified_by", "SYSTEM");
            SetDefault(line, "time_stamp", DateTime.UtcNow);
            SetDefault(line, "trn_discr_tot_amt", 0);
            SetDefault(line, "trn_discr_unit_amt", 0);
            SetDefault(line, "trn_disc_amt", GetDecimal(line, "disc_amt") ?? 0);
            SetDefault(line, "trn_ext_cst_amt", extCost);
            SetDefault(line, "trn_ln_chg_cst_amt", 0);
            SetDefault(line, "trn_ln_chg_tax_amt", 0);
            SetDefault(line, "trn_ln_chg_use_amt", 0);
            SetDefault(line, "trn_net_amt", net);
            SetDefault(line, "trn_sales_tax_amt", tax);
            SetDefault(line, "trn_tot_bef_dc_amt", GetDecimal(line, "tot_bef_disc_amt") ?? net);
            SetDefault(line, "trn_unit_cst_amt", extCost);
            SetDefault(line, "trn_use_tax_amt", GetDecimal(line, "use_tax_amt") ?? 0);
            SetDefault(line, "trn_recovery_amt", 0);
            SetDefault(line, "recovery_amt", 0);
            SetDefault(line, "recovery_rt", 0);
        }

        private static void ApplyAccountDefaults(Dictionary<string, JsonElement> account)
        {
            var cost = GetDecimal(account, "cst_amt") ?? GetDecimal(account, "net_amt") ?? 0;
            var tax = GetDecimal(account, "sales_tax_amt") ?? 0;
            var net = GetDecimal(account, "net_amt") ?? cost + tax;
            SetDefault(account, "project_abbrv_cd", "");
            SetDefault(account, "org_abbrv_cd", "");
            SetDefault(account, "project_account_abbrv_cd", "");
            SetDefault(account, "cst_amt_pct_rt", 100);
            SetDefault(account, "cst_amt", cost);
            SetDefault(account, "sales_tax_amt", tax);
            SetDefault(account, "ln_chg_cst_amt", 0);
            SetDefault(account, "tot_bef_disc_amt", GetDecimal(account, "tot_bef_disc_amt") ?? net);
            SetDefault(account, "disc_amt", 0);
            SetDefault(account, "net_amt", net);
            SetDefault(account, "taxable_fl", "N");
            SetDefault(account, "use_tax_amt", 0);
            SetDefault(account, "s_taxable_cd", "N");
            SetDefault(account, "modified_by", "SYSTEM");
            SetDefault(account, "time_stamp", DateTime.UtcNow);
            SetDefault(account, "ap_1099_fl", "N");
            SetDefault(account, "trn_cst_amt", cost);
            SetDefault(account, "trn_disc_amt", GetDecimal(account, "disc_amt") ?? 0);
            SetDefault(account, "trn_ln_chg_cst_amt", 0);
            SetDefault(account, "trn_net_amt", net);
            SetDefault(account, "trn_sales_tax_amt", tax);
            SetDefault(account, "trn_tot_bef_dc_amt", GetDecimal(account, "tot_bef_disc_amt") ?? net);
            SetDefault(account, "trn_use_tax_amt", GetDecimal(account, "use_tax_amt") ?? 0);
            SetDefault(account, "trn_recovery_amt", 0);
            SetDefault(account, "recovery_amt", 0);
        }

        private static void ApplyLabVendorDefaults(Dictionary<string, JsonElement> labVendor)
        {
            var amount = GetDecimal(labVendor, "vendor_amt") ?? 0;
            SetDefault(labVendor, "vendor_hrs", 0);
            SetDefault(labVendor, "vendor_amt", amount);
            SetDefault(labVendor, "modified_by", "SYSTEM");
            SetDefault(labVendor, "time_stamp", DateTime.UtcNow);
            SetDefault(labVendor, "trn_vendor_amt", amount);
            SetDefault(labVendor, "trn_recovery_amt", 0);
            SetDefault(labVendor, "recovery_amt", 0);
            SetDefault(labVendor, "effect_bill_dt", DateTime.UtcNow);
            SetDefault(labVendor, "sales_tax_fl", "N");
        }

        private static void SetDefault(Dictionary<string, JsonElement> values, string key, object value)
        {
            if (!values.TryGetValue(key, out var element) || IsNullJson(element) || IsBlankString(element))
            {
                values[key] = JsonSerializer.SerializeToElement(value);
            }
        }

        private async Task<SubPeriodKey?> FindSubPeriodAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            string companyId,
            string? fiscalYear,
            int? periodNo,
            int? subPeriodNo,
            bool exactOnly,
            CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;

            if (exactOnly && fiscalYear is not null && periodNo is not null && subPeriodNo is not null)
            {
                command.CommandText = $"""
                    select company_id, fy_cd, period_no, sub_period_no
                    from {_quotedSchemaName}.sub_period
                    where company_id = @companyId
                      and fy_cd = @fyCd
                      and period_no = @periodNo
                      and sub_period_no = @subPeriodNo
                    limit 1
                    """;
                command.Parameters.AddWithValue("companyId", companyId);
                command.Parameters.AddWithValue("fyCd", fiscalYear);
                command.Parameters.AddWithValue("periodNo", periodNo.Value);
                command.Parameters.AddWithValue("subPeriodNo", subPeriodNo.Value);
            }
            else
            {
                command.CommandText = $"""
                    select company_id, fy_cd, period_no, sub_period_no
                    from {_quotedSchemaName}.sub_period
                    where company_id = @companyId
                    order by fy_cd desc, period_no desc, sub_period_no desc
                    limit 1
                    """;
                command.Parameters.AddWithValue("companyId", companyId);
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new SubPeriodKey(
                reader.GetString("company_id"),
                reader.GetString("fy_cd"),
                reader.GetInt32("period_no"),
                reader.GetInt32("sub_period_no"));
        }

        private async Task<Dictionary<string, TableSchema>> LoadSchemaAsync(
            NpgsqlConnection connection,
            CancellationToken cancellationToken)
        {
            var schemas = VoucherTables.ToDictionary(
                table => table,
                table => new TableSchema(table),
                StringComparer.OrdinalIgnoreCase);

            await using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    select table_name, ordinal_position, column_name, data_type, is_nullable, is_identity,
                           numeric_precision, numeric_scale
                    from information_schema.columns
                    where table_schema = @schema
                      and table_name = any(@tables)
                    order by table_name, ordinal_position
                    """;
                command.Parameters.AddWithValue("schema", _schemaName);
                command.Parameters.AddWithValue("tables", VoucherTables);

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    var tableName = reader.GetString("table_name");
                    schemas[tableName].Columns[reader.GetString("column_name")] = new ColumnSchema(
                        reader.GetInt32("ordinal_position"),
                        reader.GetString("column_name"),
                        reader.GetString("data_type"),
                        reader.GetString("is_nullable") == "YES",
                        reader.GetString("is_identity") == "YES",
                        reader.IsDBNull(reader.GetOrdinal("numeric_precision")) ? null : reader.GetInt32("numeric_precision"),
                        reader.IsDBNull(reader.GetOrdinal("numeric_scale")) ? null : reader.GetInt32("numeric_scale"));
                }
            }

            await using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    select tc.table_name, kcu.column_name
                    from information_schema.table_constraints tc
                    join information_schema.key_column_usage kcu
                      on kcu.constraint_schema = tc.constraint_schema
                     and kcu.constraint_name = tc.constraint_name
                     and kcu.table_schema = tc.table_schema
                     and kcu.table_name = tc.table_name
                    where tc.table_schema = @schema
                      and tc.table_name = any(@tables)
                      and tc.constraint_type = 'PRIMARY KEY'
                    order by tc.table_name, kcu.ordinal_position
                    """;
                command.Parameters.AddWithValue("schema", _schemaName);
                command.Parameters.AddWithValue("tables", VoucherTables);

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    schemas[reader.GetString("table_name")].PrimaryKey.Add(reader.GetString("column_name"));
                }
            }

            return schemas;
        }

        private async Task<int> InsertRowAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            TableSchema schema,
            Dictionary<string, JsonElement> values,
            Dictionary<string, object?>? forcedValues,
            string returningColumn,
            CancellationToken cancellationToken)
        {
            var row = BuildColumnValues(schema, values, forcedValues);

            if (row.Count == 0)
            {
                throw new ArgumentException($"No valid columns supplied for {schema.TableName}.");
            }

            var columns = row.Keys.ToList();
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"""
                insert into {_quotedSchemaName}.{QuoteIdentifier(schema.TableName)}
                ({string.Join(", ", columns.Select(QuoteIdentifier))})
                values ({string.Join(", ", columns.Select((_, i) => $"@p{i}"))})
                returning {QuoteIdentifier(returningColumn)}
                """;

            for (var i = 0; i < columns.Count; i++)
            {
                command.Parameters.AddWithValue($"p{i}", row[columns[i]] ?? DBNull.Value);
            }

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        private async Task<int> UpdateRowAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            TableSchema schema,
            Dictionary<string, JsonElement> values,
            Dictionary<string, object?> keys,
            CancellationToken cancellationToken)
        {
            var row = BuildColumnValues(schema, values, forcedValues: null);

            foreach (var key in keys.Keys.Concat(schema.PrimaryKey).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                row.Remove(key);
            }

            foreach (var identityColumn in schema.Columns.Values.Where(column => column.IsIdentity))
            {
                row.Remove(identityColumn.ColumnName);
            }

            if (row.Count == 0)
            {
                return 0;
            }

            var columns = row.Keys.ToList();
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"""
                update {_quotedSchemaName}.{QuoteIdentifier(schema.TableName)}
                set {string.Join(", ", columns.Select((column, i) => $"{QuoteIdentifier(column)} = @p{i}"))}
                where {BuildWhereClause(keys, columns.Count)}
                """;

            for (var i = 0; i < columns.Count; i++)
            {
                command.Parameters.AddWithValue($"p{i}", row[columns[i]] ?? DBNull.Value);
            }
            AddParameters(command, keys.Values, columns.Count);

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        private async Task<int> DeleteByKeysAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            string tableName,
            Dictionary<string, object?> keys,
            CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"delete from {_quotedSchemaName}.{QuoteIdentifier(tableName)} where {BuildWhereClause(keys, 0)}";
            AddParameters(command, keys.Values);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        private async Task<Dictionary<string, object?>?> GetSingleByKeysAsync(
            NpgsqlConnection connection,
            string tableName,
            Dictionary<string, object?> keys,
            CancellationToken cancellationToken)
        {
            var rows = await GetRowsByKeysAsync(connection, tableName, keys, cancellationToken);
            return rows.FirstOrDefault();
        }

        private async Task<List<Dictionary<string, object?>>> GetRowsByKeysAsync(
            NpgsqlConnection connection,
            string tableName,
            Dictionary<string, object?> keys,
            CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"select * from {_quotedSchemaName}.{QuoteIdentifier(tableName)} where {BuildWhereClause(keys, 0)}";
            AddParameters(command, keys.Values);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(ReadRow(reader));
            }

            return rows;
        }

        private static Dictionary<string, object?> BuildColumnValues(
            TableSchema schema,
            Dictionary<string, JsonElement> values,
            Dictionary<string, object?>? forcedValues)
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var (columnName, element) in values)
            {
                if (!schema.Columns.TryGetValue(columnName, out var column))
                {
                    throw new ArgumentException($"Column '{columnName}' does not exist in table '{schema.TableName}'.");
                }

                if (column.IsIdentity && IsNullJson(element))
                {
                    continue;
                }

                row[column.ColumnName] = ConvertJsonValue(element, column);
            }

            if (forcedValues is not null)
            {
                foreach (var (columnName, value) in forcedValues)
                {
                    if (!schema.Columns.ContainsKey(columnName))
                    {
                        throw new ArgumentException($"Column '{columnName}' does not exist in table '{schema.TableName}'.");
                    }

                    row[columnName] = value;
                }
            }

            return row;
        }

        private static object? ConvertJsonValue(JsonElement element, ColumnSchema column)
        {
            if (IsNullJson(element))
            {
                return null;
            }

            if (IsBlankString(element) && column.DataType != "character varying")
            {
                return null;
            }

            return column.DataType switch
            {
                "integer" => element.ValueKind == JsonValueKind.Number ? element.GetInt32() : int.Parse(element.GetString()!),
                "numeric" => NormalizeNumericValue(element.ValueKind == JsonValueKind.Number ? element.GetDecimal() : decimal.Parse(element.GetString()!), column),
                "timestamp without time zone" => element.ValueKind == JsonValueKind.String ? DateTime.Parse(element.GetString()!) : element.GetDateTime(),
                "character varying" => element.ValueKind == JsonValueKind.True ? "Y" :
                    element.ValueKind == JsonValueKind.False ? "N" : element.ToString(),
                _ => element.ToString()
            };
        }

        private static decimal NormalizeNumericValue(decimal value, ColumnSchema column)
        {
            if (column.NumericPrecision == 5 && column.NumericScale == 4 && Math.Abs(value) > 9.9999m)
            {
                value /= 100m;
            }

            return column.NumericScale is null ? value : Math.Round(value, column.NumericScale.Value);
        }

        private static bool IsNullJson(JsonElement element)
        {
            return element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined;
        }

        private static bool IsBlankString(JsonElement element)
        {
            return element.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(element.GetString());
        }

        private static string BuildWhereClause(Dictionary<string, object?> keys, int parameterOffset)
        {
            return string.Join(" and ", keys.Keys.Select((key, i) => $"{QuoteIdentifier(key)} = @p{i + parameterOffset}"));
        }

        private static void AddParameters(NpgsqlCommand command, IEnumerable<object?> values, int offset = 0)
        {
            var index = offset;
            foreach (var value in values)
            {
                command.Parameters.AddWithValue($"p{index}", value ?? DBNull.Value);
                index++;
            }
        }

        private static Dictionary<string, object?> ReadRow(IDataRecord reader)
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            return row;
        }

        private static string? GetJsonString(Dictionary<string, JsonElement> values, string key)
        {
            if (!values.TryGetValue(key, out var element) || IsNullJson(element))
            {
                return null;
            }

            return element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString();
        }

        private static int? GetJsonInt(Dictionary<string, JsonElement> values, string key)
        {
            if (!values.TryGetValue(key, out var element) || IsNullJson(element) || IsBlankString(element))
            {
                return null;
            }

            return element.ValueKind == JsonValueKind.Number ? element.GetInt32() : int.Parse(element.GetString()!);
        }

        private static decimal? GetDecimal(Dictionary<string, JsonElement> values, string key)
        {
            if (!values.TryGetValue(key, out var element) || IsNullJson(element) || IsBlankString(element))
            {
                return null;
            }

            return element.ValueKind == JsonValueKind.Number ? element.GetDecimal() : decimal.Parse(element.GetString()!);
        }

        private static string QuoteIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("Identifier cannot be empty.", nameof(identifier));
            }

            return "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
        }

        private sealed record SubPeriodKey(string CompanyId, string FiscalYear, int PeriodNo, int SubPeriodNo);

        private sealed class TableSchema
        {
            public TableSchema(string tableName)
            {
                TableName = tableName;
            }

            public string TableName { get; }
            public Dictionary<string, ColumnSchema> Columns { get; } = new(StringComparer.OrdinalIgnoreCase);
            public List<string> PrimaryKey { get; } = [];
        }

        private sealed record ColumnSchema(
            int OrdinalPosition,
            string ColumnName,
            string DataType,
            bool IsNullable,
            bool IsIdentity,
            int? NumericPrecision,
            int? NumericScale);
    }
}
