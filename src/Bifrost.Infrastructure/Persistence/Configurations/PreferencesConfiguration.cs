using Bifrost.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bifrost.Infrastructure.Persistence.Configurations;

public class PreferencesConfiguration : EntityConfiguration<Preferences>
{
    public override void Configure(EntityTypeBuilder<Preferences> builder)
    {
        base.Configure(builder);
        builder.OwnsOne(p => p.SalaryRange);
        builder.Property(p => p.JobType).IsRequired();
        builder.Property(p => p.NeedSponsorship).IsRequired();
        builder.Property(p => p.NeedRelocation).IsRequired();
    }
}
