using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Services
{
    public class VendorTransactionService
    {
        private readonly MydatabaseContext _context;

        public VendorTransactionService(MydatabaseContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateOrUpdateFullVendorAsync(
            Vendor vendor,
            List<VendorAddress>? addresses,
            List<Vendor1099Detail>? taxDetails,
            List<VendorEmployee>? employees)
        {
            using var trx = await _context.Database.BeginTransactionAsync();

            try
            {
                // 🔹 1. UPSERT VENDOR
                var existingVendor = await _context.Vendors
                    .FirstOrDefaultAsync(x =>
                        x.VendId == vendor.VendId &&
                        x.CompanyId == vendor.CompanyId);

                if (existingVendor == null)
                {
                    vendor.TimeStamp = DateTime.UtcNow;
                    await _context.Vendors.AddAsync(vendor);
                }
                else
                {
                    _context.Entry(existingVendor).CurrentValues.SetValues(vendor);
                    existingVendor.TimeStamp = DateTime.UtcNow;
                }

                // 🔹 2. ADDRESSES
                if (addresses != null)
                {
                    var existingAddresses = await _context.VendorAddresses
                        .Where(x => x.VendorId == vendor.VendId)
                        .ToListAsync();

                    foreach (var addr in addresses)
                    {
                        addr.VendorId = vendor.VendId;

                        var existing = existingAddresses
                            .FirstOrDefault(x => x.AddrCode == addr.AddrCode);

                        if (existing == null)
                        {
                            //addr.TimeStamp = DateTime.UtcNow;
                            await _context.VendorAddresses.AddAsync(addr);
                        }
                        else
                        {
                            _context.Entry(existing).CurrentValues.SetValues(addr);
                            //existing.TimeStamp = DateTime.UtcNow;
                        }
                    }

                    var toDelete = existingAddresses
                        .Where(x => !addresses.Any(a => a.AddrCode == x.AddrCode))
                        .ToList();

                    if (toDelete.Any())
                        _context.VendorAddresses.RemoveRange(toDelete);
                }

                // 🔹 3. 1099 DETAILS
                if (taxDetails != null)
                {
                    var existingTax = await _context.Vendor1099Details
                        .Where(x => x.PayVendorId == vendor.VendId &&
                                    x.CompanyId == vendor.CompanyId)
                        .ToListAsync();

                    foreach (var tax in taxDetails)
                    {
                        tax.PayVendorId = vendor.VendId;
                        tax.CompanyId = vendor.CompanyId;

                        var existing = existingTax.FirstOrDefault(x =>
                            x.TaxableEntityId == tax.TaxableEntityId &&
                            x.CalendarYear == tax.CalendarYear &&
                            x.Form1099TypeCode == tax.Form1099TypeCode);

                        if (existing == null)
                        {
                            tax.CreatedAt = DateTime.UtcNow;
                            await _context.Vendor1099Details.AddAsync(tax);
                        }
                        else
                        {
                            _context.Entry(existing).CurrentValues.SetValues(tax);
                            existing.UpdatedAt = DateTime.UtcNow;
                        }
                    }

                    var toDelete = existingTax.Where(x =>
                        !taxDetails.Any(t =>
                            t.TaxableEntityId == x.TaxableEntityId &&
                            t.CalendarYear == x.CalendarYear &&
                            t.Form1099TypeCode == x.Form1099TypeCode))
                        .ToList();

                    if (toDelete.Any())
                        _context.Vendor1099Details.RemoveRange(toDelete);
                }

                // 🔹 4. EMPLOYEES (✅ ADDED)
                if (employees != null)
                {
                    var existingEmployees = await _context.VendorEmployees
                        .Where(x => x.VendId == vendor.VendId &&
                                    x.CompanyId == vendor.CompanyId)
                        .ToListAsync();

                    foreach (var emp in employees)
                    {
                        emp.VendId = vendor.VendId;
                        emp.CompanyId = vendor.CompanyId;

                        var existing = existingEmployees.FirstOrDefault(x =>
                            x.VendEmplId == emp.VendEmplId);

                        if (existing == null)
                        {
                            emp.TimeStamp = DateTime.UtcNow;
                            await _context.VendorEmployees.AddAsync(emp);
                        }
                        else
                        {
                            _context.Entry(existing).CurrentValues.SetValues(emp);
                            existing.TimeStamp = DateTime.UtcNow;
                        }
                    }

                    // ✅ DELETE removed employees
                    var toDelete = existingEmployees
                        .Where(x => !employees.Any(e => e.VendEmplId == x.VendEmplId))
                        .ToList();

                    if (toDelete.Any())
                        _context.VendorEmployees.RemoveRange(toDelete);
                }

                // 🔹 SAVE
                await _context.SaveChangesAsync();
                await trx.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                throw new Exception($"Vendor transaction failed: {ex.Message}", ex);
            }
        }

        // ✅ GET WITH PAGINATION + SORTING
        public async Task<PagedResultDTO<object>> GetVendorsAsync(
            int page = 1,
            int pageSize = 20,
            string? sortBy = "vend_id",
            string? sortOrder = "asc")
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;

            var query = _context.Vendors
                .AsNoTracking()
                .AsQueryable();

            // ✅ Sorting
            query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("vend_id", "desc") => query.OrderByDescending(x => x.VendId),
                ("vend_id", _) => query.OrderBy(x => x.VendId),

                ("vendor_name", "desc") => query.OrderByDescending(x => x.VendName),
                ("vendor_name", _) => query.OrderBy(x => x.VendName),

                ("company_id", "desc") => query.OrderByDescending(x => x.CompanyId),
                ("company_id", _) => query.OrderBy(x => x.CompanyId),

                _ => query.OrderBy(x => x.VendId)
            };

            var totalRecords = await query.CountAsync();

            var vendors = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vendIds = vendors.Select(v => v.VendId).ToList();

            var addresses = await _context.VendorAddresses
                .Where(a => vendIds.Contains(a.VendorId))
                .AsNoTracking()
                .ToListAsync();

            var result = vendors.Select(v => new
            {
                vendor = v,
                addresses = addresses.Where(a => a.VendorId == v.VendId).ToList()
            });

            return new PagedResultDTO<object>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                Data = result
            };
        }

        public async Task<object> GetALLVendorsAsync(
    int page,
    int pageSize,
    string? sortBy,
    string? sortOrder,
    string? search)
        {
            var query = _context.Vendors.AsQueryable();

            // ✅ SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();

                query = query.Where(v =>
                    v.VendId.ToLower().Contains(search) ||
                    v.VendName.ToLower().Contains(search)
                );
            }

            // ✅ SORTING
            query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("vend_id", "desc") => query.OrderByDescending(x => x.VendId),
                ("vend_id", _) => query.OrderBy(x => x.VendId),

                ("vend_name", "desc") => query.OrderByDescending(x => x.VendName),
                ("vend_name", _) => query.OrderBy(x => x.VendName),

                _ => query.OrderBy(x => x.VendId)
            };

            // ✅ PAGINATION
            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new
                {
                    v.VendId,
                    v.VendName,
                    v.VendApprvlCd,
                    v.VendGrpCd
                })
                .ToListAsync();

            return new
            {
                TotalRecords = totalRecords,
                Page = page,
                PageSize = pageSize,
                Data = data
            };
        }
    }
}
