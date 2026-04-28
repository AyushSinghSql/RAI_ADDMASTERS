namespace PlanningAPI.Helpers
{
    public static class AuditHelper
    {
        public static void ApplyCreate(object entity)
        {
            var prop = entity.GetType().GetProperty("TimeStamp");
            if (prop != null)
                prop.SetValue(entity, DateTime.UtcNow);
        }
    }
}
