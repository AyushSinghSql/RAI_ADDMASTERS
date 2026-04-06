using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Services
{
    public interface IFinancialControlService
    {
        Task<bool> CanPostAsync(string journalCode, string fyCd, int periodNo, string companyId);
        Task<string?> ValidatePostAsync(string journalCode, string fyCd, int periodNo, string companyId);
        Task<string?> ValidateAsync(string fyCd, int periodNo, int subPeriodNo,
                                string journalCode, string companyId);
    }
    public class FinancialControlService : IFinancialControlService
    {
        private readonly MydatabaseContext _context;

        public FinancialControlService(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ Simple boolean check
        public async Task<bool> CanPostAsync(string journalCode, string fyCd, int periodNo, string companyId)
        {
            var periodOpen = await _context.AccountingPeriods
                .AnyAsync(x => x.FyCd == fyCd &&
                               x.PeriodNo == periodNo &&
                               x.StatusCd == "O");

            if (!periodOpen) return false;

            var journalOpen = await _context.JournalStatuses
                .AnyAsync(x => x.FyCd == fyCd &&
                               x.PeriodNo == periodNo &&
                               x.CompanyId == companyId &&
                               x.JournalCode == journalCode &&
                               x.IsOpen == "Y");

            return journalOpen;
        }

        // ✅ Detailed validation (better for APIs)
        public async Task<string?> ValidatePostAsync(string journalCode, string fyCd, int periodNo, string companyId)
        {
            var period = await _context.AccountingPeriods
                .FirstOrDefaultAsync(x => x.FyCd == fyCd && x.PeriodNo == periodNo);

            if (period == null)
                return "Invalid fiscal year or period";

            if (period.StatusCd != "O")
                return "Accounting period is closed";

            var journal = await _context.JournalStatuses
                .FirstOrDefaultAsync(x =>
                    x.JournalCode == journalCode &&
                    x.FyCd == fyCd &&
                    x.PeriodNo == periodNo &&
                    x.CompanyId == companyId);

            if (journal == null || journal.IsOpen != "Y")
                return "Journal is closed";

            return null; // ✅ valid
        }

        public async Task<string?> ValidateAsync(
       string fyCd, int periodNo, int subPeriodNo,
       string journalCode, string companyId)
        {
            // 🔒 1. Fiscal Year
            var fy = await _context.FiscalYears
                .FirstOrDefaultAsync(x => x.FyCd == fyCd);

            if (fy == null)
                return "Invalid fiscal year";

            if (fy.StatusCd != "O")
                return "Fiscal year is closed";

            // 🔒 2. Period
            var period = await _context.AccountingPeriods
                .FirstOrDefaultAsync(x => x.FyCd == fyCd && x.PeriodNo == periodNo);

            if (period == null)
                return "Invalid period";

            if (period.StatusCd != "O")
                return "Accounting period is closed";

            // 🔒 3. Sub Period
            var subPeriod = await _context.SubPeriods
                .FirstOrDefaultAsync(x =>
                    x.FyCd == fyCd &&
                    x.PeriodNo == periodNo &&
                    x.SubPeriodNo == subPeriodNo);

            if (subPeriod == null)
                return "Invalid sub-period";

            if (subPeriod.StatusCd != "O")
                return "Sub-period is closed";

            // 🔒 4. Journal
            var journal = await _context.JournalStatuses
                .FirstOrDefaultAsync(x =>
                    x.JournalCode == journalCode &&
                    x.FyCd == fyCd &&
                    x.PeriodNo == periodNo &&
                    x.CompanyId == companyId);

            if (journal == null || journal.IsOpen != "Y")
                return "Journal is closed";

            return null; // ✅ All good
        }
    }
}
